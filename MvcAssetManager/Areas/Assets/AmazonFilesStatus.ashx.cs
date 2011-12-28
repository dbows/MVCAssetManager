using System.IO;
using System.Configuration;
using System.Web;
using System;
using Amazon.S3.Model;
using System.Text.RegularExpressions;

namespace AssetManager {
	public class AmazonFilesStatus {
		public string HandlerPath = VirtualPathUtility.ToAbsolute(ConfigurationManager.AppSettings["Assets_Amazon_HandlerPath"]);
        public string IconPath = VirtualPathUtility.RemoveTrailingSlash(VirtualPathUtility.ToAbsolute(ConfigurationManager.AppSettings["Assets_Amazon_DefaultThumbPath"]));
        public int IconSize = int.Parse(ConfigurationManager.AppSettings["Assets_Amazon_IconSize"]);
        public bool ShowImageIcons = bool.TrueString.ToLower() == ConfigurationManager.AppSettings["Assets_Amazon_ShowImageIcons"].ToLower() ? true : false;

		public string group { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public int size { get; set; }
		public string progress { get; set; }
		public string url { get; set; }
		public string thumbnail_url { get; set; }
		public string delete_url { get; set; }
		public string delete_type { get; set; }
		public string error { get; set; }
        public bool isimage {get;set;}
        public int imgheight {get;set;}
        public int imgwidth {get;set;}

        public AmazonFilesStatus(S3Object s3File,string bucket,string prefix)
        {

            var filename = s3File.Key.Replace(prefix,"");
            var fileExt = filename.Remove(0,filename.LastIndexOf('.'));
            type =  getContentTypeByExtension(fileExt);
            isimage =  Regex.Match(filename.ToLower(),AmazonHelper.ImgExtensions()).Success;
            var client = AmazonHelper.GetS3Client();
            var metareq = new GetObjectMetadataRequest().WithBucketName(bucket).WithKey(s3File.Key);
            var meta = client.GetObjectMetadata(metareq);                
            var height = 0;
            var width = 0;
            if(isimage){
                height = String.IsNullOrWhiteSpace(meta.Headers["x-amz-meta-height"])? 0 : Int32.Parse(meta.Headers["x-amz-meta-height"]);
                 width = String.IsNullOrWhiteSpace(meta.Headers["x-amz-meta-width"])? 0 : Int32.Parse(meta.Headers["x-amz-meta-width"]);

            }
            
            var size = Convert.ToInt32(s3File.Size);
            
            SetValues(filename,bucket,prefix,size,height,width);
        }
        public AmazonFilesStatus (string filename,string bucket,string prefix,Int32 filesize,Int32 height = 0,Int32 width = 0)
        {
            SetValues(filename,bucket,prefix,filesize,height,width);
        }

        public void SetValues (string filename,string bucket,string prefix,Int32 filesize,Int32 height = 0,Int32 width = 0){
            
            var baseUrl = ConfigurationManager.AppSettings["Assets_Amazon_BaseUrl"];
            baseUrl = String.Format(baseUrl,bucket,prefix);
            var fileExt = filename.Remove(0,filename.LastIndexOf('.'));
            type =  getContentTypeByExtension(fileExt);
            var thumbFile = getIconByExtension(fileExt);
            name = filename;
            progress = "1.0";
            size = filesize;
            url = VirtualPathUtility.RemoveTrailingSlash(baseUrl) + "/" + filename;
            delete_type = "DELETE";
            delete_url = VirtualPathUtility.RemoveTrailingSlash(HandlerPath) + "/" + "AmazonTransferHandler.ashx?f=" + HttpUtility.UrlEncode(filename);
            type = type;
            imgheight = height;
            imgwidth = width;
            thumbnail_url = thumbFile == "image" ? VirtualPathUtility.RemoveTrailingSlash(baseUrl) + "/thumbs/" + filename.ToLower().Replace(fileExt.ToLower(),".png") : VirtualPathUtility.RemoveTrailingSlash(IconPath) + "/" + thumbFile;       
        }


        private string getIconByExtension(string strExtension,int size = 48)
        {
            string returnTemplate = "plain_file_{0}.png";

            switch (strExtension)
            {
                case ".pdf": 
                    returnTemplate =  "pdf_{0}.jpg";
                    break;
                case ".doc": 
                    returnTemplate =  "word_{0}.jpg";
                    break;
                 case ".docx": 
                    returnTemplate =  "word_{0}.jpg";
                    break;                             
                 case ".ppt": 
                    returnTemplate =  "ppt_{0}.jpg";
                    break;               
                  case ".pptx": 
                    returnTemplate =  "ppt_{0}.jpg";
                    break;              
                  case ".vsd": 
                    returnTemplate =  "visio_{0}.jpg";
                    break;             
                  case ".xls": 
                    returnTemplate =  "excel_{0}.jpg";
                    break;             
                  case ".xlsx": 
                    returnTemplate =  "excel_{0}.jpg";
                    break;             
                  case ".ai": 
                    returnTemplate =  "ai_{0}.jpg";
                    break;             
                  case ".psd": 
                    returnTemplate =  "ps_{0}.jpg";
                    break;                       
                  case ".csv": 
                    returnTemplate =  "text_file_{0}.jpg";
                    break;        

                case ".aiff":
                    returnTemplate =  "disk_{0}.png";
                    break;
                case ".au":
                    returnTemplate =  "disk_{0}.png";
                    break;
                case ".mid":
                    returnTemplate =  "disk_{0}.png";
                    break;
                case ".mp3":
                    returnTemplate =  "disk_{0}.png";
                    break;
                case ".m3u":
                    returnTemplate =  "disk_{0}.png";
                    break;
                case ".wav":
                    returnTemplate =  "disk_{0}.png";
                    break;
                case ".wax":
                    returnTemplate =  "disk_{0}.png";
                    break;
                case ".wma":
                    returnTemplate =  "disk_{0}.png";
                    break;
                case ".bmp":
                    returnTemplate =  "image_{0}.png";
                    break;
                case ".gif":
                    returnTemplate =  ShowImageIcons ? "image" : "image_{0}.png";
                    break;
                case ".jpg":
                    returnTemplate =   ShowImageIcons ? "image" : "image_{0}.png";
                    break;
                case ".png":
                    returnTemplate =   ShowImageIcons ? "image" : "image_{0}.png";
                    break;
                case ".tiff":
                    returnTemplate =  "image_{0}.png";
                    break;
                case ".ico":
                    returnTemplate =  "image_{0}.png";
                    break;
                //case ".htm":
                //    return "text/html";
                case ".txt":
                    returnTemplate =  "text_file_{0}.png";
                    break;
                //case ".vcf":
                //    return "text/x-vcard";
                //case ".avi":
                //    return "video/avi";
                //case ".mpeg":
                //    return "video/mpeg";
                //case ".wm":
                //    return "video/x-ms-wm";
                //case ".wmv":
                //    return "video/x-ms-wmv";
                //case ".wmx":
                //    return "video/x-ms-wmx";
                //case ".wvx":
                //    return "video/x-ms-wvx";
                default:
                    break;
            }
            return string.Format(returnTemplate,size);
        }

        private string getContentTypeByExtension(string strExtension)
        {


            switch (strExtension)
            {

                case ".pdf": 
                    return "application/pdf";
                case ".ps":
                    return "application/postscript";
                case ".xps":
                    return "application/vnd.ms-xpsdocument";
                case ".tgz":
                    return "application/x-compressed";
                case ".gz":
                    return "application/x-gzip";
                case ".ins":
                    return "application/x-internet-signup";
                case ".iii":
                    return "application/x-iphone";
                case ".jtx":
                    return "application/x-jtx+xps";
                case ".asx":
                    return "application/x-mplayer2";
                case ".application":
                    return "application/x-ms-application";
                case ".wmd":

                    return "application/x-ms-wmd";


                case ".wmz":

                    return "application/x-ms-wmz";

                case ".xbap":

                    return "application/x-ms-xbap";



                case ".tar":

                    return "application/x-tar";



                case ".zip":

                    return "application/x-zip-compressed";


                case ".xml":

                    return "application/xml";


                case ".aiff":

                    return "audio/aiff";


                case ".au":

                    return "audio/basic";


                case ".mid":

                    return "audio/mid";



                case ".mp3":

                    return "audio/mp3";



                case ".m3u":

                    return "audio/mpegurl";



                case ".wav":

                    return "audio/wav";


                case ".wax":

                    return "audio/x-ms-wax";


                case ".wma":

                    return "audio/x-ms-wma";



                case ".bmp":

                    return "image/bmp";


                case ".gif":

                    return "image/gif";


                case ".jpg":

                    return "image/jpeg";



                case ".png":

                    return "image/png";


                case ".tiff":

                    return "image/tiff";


                case ".ico":

                    return "image/x-icon";



                case ".htm":

                    return "text/html";


                case ".txt":

                    return "text/plain";


                case ".vcf":

                    return "text/x-vcard";


                case ".avi":

                    return "video/avi";


                case ".mpeg":

                    return "video/mpeg";



                case ".wm":

                    return "video/x-ms-wm";


                case ".wmv":

                    return "video/x-ms-wmv";


                case ".wmx":

                    return "video/x-ms-wmx";


                case ".wvx":

                    return "video/x-ms-wvx";


                default:
                    return "text/text";
            }
        }
	}
}