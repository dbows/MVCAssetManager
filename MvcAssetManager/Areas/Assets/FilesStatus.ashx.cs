using System.IO;
using System.Configuration;
using System.Web;
using System;

namespace AssetManager {
	public class FilesStatus {
		public string HandlerPath = VirtualPathUtility.ToAbsolute(ConfigurationManager.AppSettings["Assets_HandlerPath"]);
        public string IconPath = VirtualPathUtility.RemoveTrailingSlash(VirtualPathUtility.ToAbsolute(ConfigurationManager.AppSettings["Assets_DefaultThumbPath"]));
        public int IconSize = int.Parse(ConfigurationManager.AppSettings["Assets_IconSize"]);

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

		public FilesStatus () { }

		public FilesStatus (FileInfo fileInfo) { SetValues(fileInfo.Name, (int)fileInfo.Length); }

		public FilesStatus (string fileName, int fileLength) { SetValues(fileName, fileLength); }
		public FilesStatus (string fileName, int fileLength,string mimeType) { SetValues(fileName, fileLength,mimeType); }

		public void SetValues (string fileName, int fileLength,string mimeType = "") {
            fileName = fileName.ToLower();
			name = fileName;
			var fileExt = fileName.Remove(0,fileName.LastIndexOf('.'));
			type = string.IsNullOrWhiteSpace(mimeType) ? getContentTypeByExtension(fileExt) : mimeType;
			size = fileLength;
			progress = "1.0";
			url = HandlerPath + "FileTransferHandler.ashx?f=" + HttpUtility.UrlEncode(fileName);
            var thumbFile = getIconByExtension(fileExt);
            //If the thumbFile is "image" then thumbnail the image else just use the default icon for the file type.
			thumbnail_url = thumbFile == "image" ? HandlerPath + "Thumbnail.ashx?f=" + HttpUtility.UrlEncode(fileName) : IconPath + "/" + thumbFile;
			delete_url = HandlerPath + "FileTransferHandler.ashx?f=" + HttpUtility.UrlEncode(fileName);
			delete_type = "DELETE";
		}
        private string getIconByExtension(string strExtension,int size = 48)
        {
            string returnTemplate = "plain_file_{0}.png";

            switch (strExtension)
            {
                case ".pdf": 
                    returnTemplate =  "rich_text_file_{0}.png";
                    break;
                //case ".tar":
                //    return "application/x-tar";
                //case ".zip":
                //    return "application/x-zip-compressed";

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
                    returnTemplate =  "image";
                    break;
                case ".jpg":
                    returnTemplate =  "image";
                    break;
                case ".png":
                    returnTemplate =  "image";
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