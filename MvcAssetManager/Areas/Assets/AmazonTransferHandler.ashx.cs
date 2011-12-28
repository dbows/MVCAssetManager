using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Auth;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace AssetManager {
	public class AmazonTransferHandler : IHttpHandler {
		private readonly JavaScriptSerializer js = new JavaScriptSerializer();

		public string StorageRoot {
			get { return ConfigurationManager.AppSettings["Assets_Amazon_Bucket"]; }
		}
        public string StoragePrefix {
			get { return ConfigurationManager.AppSettings["Assets_Amazon_Prefix"]; }
        }


		public bool IsReusable { get { return false; } }

		public void ProcessRequest (HttpContext context) {
			context.Response.AddHeader("Pragma", "no-cache");
			context.Response.AddHeader("Cache-Control", "private, no-cache");

			HandleMethod(context);
		}

		// Handle request based on method
		private void HandleMethod (HttpContext context) {
			switch (context.Request.HttpMethod) {
				case "HEAD":
				case "GET":
					if (GivenFilename(context)) DeliverFile(context);
					else ListCurrentFiles(context);
					break;

				case "POST":
				case "PUT":
					UploadFile(context);
					break;

				case "DELETE":
					DeleteFile(context);
					break;

				case "OPTIONS":
					ReturnOptions(context);
					break;

				default:
					context.Response.ClearHeaders();
					context.Response.StatusCode = 405;
					break;
			}
		}

		private void ReturnOptions(HttpContext context) {
			context.Response.AddHeader("Allow", "DELETE,GET,HEAD,POST,PUT,OPTIONS");
			context.Response.StatusCode = 200;
		}

		// Delete file from the server
		private void DeleteFile (HttpContext context) {
                var _getlen = 10;
                var fileName = context.Request["f"];
                var fileExt = fileName.Remove(0,fileName.LastIndexOf('.')).ToLower();
                var hasThumb =  Regex.Match(fileName.ToLower(),AmazonHelper.ImgExtensions()).Success; 
                var keyName = GetKeyName(context,HttpUtility.UrlDecode(context.Request["f"]));
                var client = AmazonHelper.GetS3Client(); 
                var extrequest = new GetObjectRequest()
                                        .WithByteRange(0,_getlen)
                                        .WithKey(keyName)
                                        .WithBucketName(StorageRoot);
                var extresponse = client.GetObject(extrequest);
                var length = extresponse.ContentLength;
                extresponse.Dispose();
                if(length == _getlen + 1){

                    var delrequest = new DeleteObjectRequest()
                                            .WithKey(keyName)
                                            .WithBucketName(StorageRoot);
                    var delresponse = client.DeleteObject(delrequest);
                    delresponse.Dispose();
                    if(hasThumb){
                        try
                        {
                            keyName = keyName.Replace(fileName,"thumbs/" + fileName.Replace(fileExt,".png"));
                            var thumbcheck = new GetObjectRequest()
                                                    .WithByteRange(0,_getlen)
                                                    .WithKey(keyName)
                                                    .WithBucketName(StorageRoot);
                            var thumbCheckResponse = client.GetObject(thumbcheck);
                            length = extresponse.ContentLength;
                            thumbCheckResponse.Dispose();
                            if(length == _getlen + 1){
                                var thumbdelrequest = new DeleteObjectRequest()
                                                        .WithKey(keyName)
                                                        .WithBucketName(StorageRoot);
                                var thumbdelresponse = client.DeleteObject(thumbdelrequest);
                                delresponse.Dispose();                            
                            }
                        }
                        catch (Exception ex)
                        {
                            
                           var messg = ex.Message;
                        }                        
                    }

                }
		}

		// Upload file to the server
		private void UploadFile (HttpContext context) {
			var statuses = new List<AmazonFilesStatus>();
			var headers = context.Request.Headers;

			if (string.IsNullOrEmpty(headers["X-File-Name"])) {
				UploadWholeFile(context, statuses);
			} else {
				//UploadPartialFile(headers["X-File-Name"], context, statuses);
			}

			WriteJsonIframeSafe(context, statuses);
		}

        private string getDocType(string key)
        {
                var fileExt = key.Remove(0,key.LastIndexOf('.')).ToLower();
                return  Regex.Match(key.ToLower(),AmazonHelper.ImgExtensions()).Success ? "image" : "doc";             
        }

		// Upload partial file - NOT COMPLETE
		private void UploadPartialFile (string fileName, HttpContext context, List<AmazonFilesStatus> statuses) {
            //if (context.Request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            //var inputStream = context.Request.Files[0].InputStream;
            //var fullName = StorageRoot + Path.GetFileName(fileName);

            //using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write)) {
            //    var buffer = new byte[1024];

            //    var l = inputStream.Read(buffer, 0, 1024);
            //    while (l > 0) {
            //        fs.Write(buffer, 0, l);
            //        l = inputStream.Read(buffer, 0, 1024);
            //    }
            //    fs.Flush();
            //    fs.Close();
            //}
            //statuses.Add(new AmazonFilesStatus(new FileInfo(fullName)));
		}

        private MemoryStream GetThumbnail(MemoryStream file)
        {
                var thumbHeight = 48;
                var thumbWidth = 120;
                Image img = Image.FromStream(file);
                // figure out the ratio - we want to keep the thumb being more than 120 w which is 2.5 * 48.
                var ratio = Math.Round((double)img.Height > (double)img.Width ? (double)img.Width / (double)img.Height : (double)img.Width / (double)img.Height,2);
                thumbWidth = ratio > 2.5 ? thumbWidth : Convert.ToInt32(Math.Round(thumbHeight * ratio,0));
                thumbHeight = ratio > 2.5 ? Convert.ToInt32(Math.Round((double)thumbWidth / (double)ratio,0)) : thumbHeight; 
                 
                var thumbnailImage = img.GetThumbnailImage(thumbWidth, thumbHeight, new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
                // make a memory stream to work with the image bytes
                MemoryStream imageStream = new MemoryStream();

                // put the image into the memory stream
                thumbnailImage.Save(imageStream,ImageFormat.Png);

                // make byte array the same size as the image
                byte[] imageContent = new Byte[imageStream.Length];

                // rewind the memory stream
                imageStream.Position = 0;

                // load the byte array with the image
                imageStream.Read(imageContent, 0, (int)imageStream.Length);

                return imageStream;
        }

        public bool ThumbnailCallback()
        {
            return true;
        }	
		// Upload entire file
		private void UploadWholeFile (HttpContext context, List<AmazonFilesStatus> statuses) {

			for (int i = 0; i < context.Request.Files.Count; i++) {
				var file = context.Request.Files[i];
                
                MemoryStream thumbStream = new MemoryStream();
                var fileExt = file.FileName.Remove(0,file.FileName.LastIndexOf('.')).ToLower();
                var needsThumb =  Regex.Match(file.FileName.ToLower(),AmazonHelper.ImgExtensions()).Success; 
                var keyName = GetKeyName(context,file.FileName.ToLower());
                var len = file.ContentLength;
                var height = 0;
                var width = 0;
                if(needsThumb){

                    file.InputStream.CopyTo(thumbStream);
                }
                var client = AmazonHelper.GetS3Client(); 
                var request = new PutObjectRequest()
                                        .WithKey(keyName)
                                        .WithBucketName(StorageRoot)
                                        .WithContentType(file.ContentType)
                                        .WithCannedACL(S3CannedACL.PublicRead)
                                        .WithAutoCloseStream(true)
                                        .WithStorageClass(S3StorageClass.ReducedRedundancy)
                                        .WithInputStream(file.InputStream);
                if (needsThumb)
                {
                    Image img = Image.FromStream(thumbStream); 
                    height = img.Height;
                    width = img.Width;
                    thumbStream.Position = 0; 
                                      
                    request.AddHeader("x-amz-meta-width",width.ToString());
                    request.AddHeader("x-amz-meta-height",height.ToString());
                    var orientation = width > height ? "h" : "v";
                    request.AddHeader("x-amz-meta-orient",orientation);
                    
                }      

                var response = client.PutObject((PutObjectRequest)request);
                response.Dispose();
                if(needsThumb){

                    
                    var thumbKeyName = VirtualPathUtility.RemoveTrailingSlash(keyName.Replace(file.FileName.ToLower(),"")) + "/thumbs/" + file.FileName.ToLower().Replace(fileExt,".png");
                    var thumbImageStream = GetThumbnail(thumbStream);
                    var thumbrequest = new PutObjectRequest()
                                            .WithKey(thumbKeyName)
                                            .WithBucketName(StorageRoot)
                                            .WithContentType("image/png")
                                            .WithCannedACL(S3CannedACL.PublicRead)
                                            .WithAutoCloseStream(true)
                                            .WithStorageClass(S3StorageClass.ReducedRedundancy)
                                            .WithInputStream(thumbImageStream);       

                    var thumbresponse = client.PutObject((PutObjectRequest)thumbrequest);  
                    thumbStream.Dispose();                  
                }
                var fileStatus = new AmazonFilesStatus(file.FileName.ToLower(),StorageRoot,GetPrefix(context),len,height,width );

				//string fullName = Path.GetFileName(file.FileName);
				statuses.Add(fileStatus);
			}
		}

		private void WriteJsonIframeSafe (HttpContext context, List<AmazonFilesStatus> statuses) {
			context.Response.AddHeader("Vary", "Accept");
			try {
				if (context.Request["HTTP_ACCEPT"].Contains("application/json"))
					context.Response.ContentType = "application/json";
				else
					context.Response.ContentType = "text/plain";
			} catch {
				context.Response.ContentType = "text/plain";
			}

			var jsonObj = js.Serialize(statuses.ToArray());
			context.Response.Write(jsonObj);
		}

		private bool GivenFilename (HttpContext context) {
			return !string.IsNullOrEmpty(context.Request["f"]);
		}

		private void DeliverFile (HttpContext context) {
			var filename = HttpUtility.UrlDecode(context.Request["f"]);
			var filePath = StorageRoot + filename;

			if (File.Exists(filePath)) {
				context.Response.AddHeader("Content-Disposition", "attachment, filename=\"" + filename + "\"");
				context.Response.ContentType = "application/octet-stream";
				context.Response.ClearContent();
				context.Response.WriteFile(filePath);
			} else
				context.Response.StatusCode = 404;
		}
        private string GetKeyName(HttpContext context,string filename){
            return GetPrefix(context) + filename;       
        }

        private string GetPrefix(HttpContext context){
            var urlPrefix = context.Request["prefix"];
            var prefix = VirtualPathUtility.RemoveTrailingSlash(StoragePrefix) + "/";
            if (!String.IsNullOrWhiteSpace(urlPrefix))
            {
                prefix = VirtualPathUtility.RemoveTrailingSlash(urlPrefix) + "/";
                prefix += !String.IsNullOrWhiteSpace(context.Request["prefixid"]) ?  context.Request["prefixid"] + "/" : "";
            }
            return prefix;            
        }

		private void ListCurrentFiles (HttpContext context) {
            
            var bucket = ConfigurationManager.AppSettings["Assets_Amazon_Bucket"];

            var prefix = GetPrefix(context);
            var doctype = context.Request["doctype"];
			var files = new List<AmazonFilesStatus>();
            var client = AmazonHelper.GetS3Client();
            var request = new ListObjectsRequest().WithBucketName(bucket).WithDelimiter("/").WithPrefix(prefix);

            var l = client.ListObjects(request);
            foreach (var fl in l.S3Objects)
            {
                
                if(fl.Size > 0)
                {
                    if(String.IsNullOrWhiteSpace(doctype) || (doctype == getDocType(fl.Key)))
                    {
                        var fs = new AmazonFilesStatus(fl,bucket,prefix);
                        files.Add(fs);
                    }
                }

            }
            var typeObj = new Dictionary<String,String>();
            typeObj.Add("doc",AmazonHelper.DocExtensions());
            typeObj.Add("image",AmazonHelper.ImgExtensions());
            typeObj.Add("media",AmazonHelper.MediaExtensions());
            var retObj = new FileReturn(){Files = files,AssetTypes = typeObj};
            
			string jsonObj = js.Serialize(retObj);
            context.Response.AddHeader("Content-Disposition", "inline, filename=\"files.json\"");
			context.Response.Write(jsonObj);
			context.Response.ContentType = "application/json";
		}

        struct FileReturn
        {
            public List<AmazonFilesStatus> Files;
            public Dictionary<String,String> AssetTypes;
        }


	}
}