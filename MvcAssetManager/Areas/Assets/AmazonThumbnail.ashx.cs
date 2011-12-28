using System.Web;
using System.Configuration;
using System.IO;
using System.Drawing;
using System;
using System.Drawing.Imaging;

namespace AssetManager {
	public class AmazonThumbnail : IHttpHandler {

		public string StorageRoot {
			get { return ConfigurationManager.AppSettings["Assets_LocalStorageRoot"]; }
		}

		public void ProcessRequest (HttpContext context) {
			var filename = Uri.UnescapeDataString(context.Request["f"]);
			var filePath = StorageRoot + filename;

			if (File.Exists(filePath)) {
                Image img = Image.FromFile(filePath);
                var thumbnailImage = img.GetThumbnailImage(48, 48, new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
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

                // return byte array to caller with image type
                context.Response.ContentType = "image/png";
                context.Response.BinaryWrite(imageContent);

			} else
            {
				context.Response.StatusCode = 404;
                }

		}

		public bool IsReusable { get { return false; } }
        public bool ThumbnailCallback()
        {
            return true;
        }	
    }

}
