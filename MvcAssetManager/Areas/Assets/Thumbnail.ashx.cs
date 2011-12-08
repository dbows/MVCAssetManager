using System.Web;
using System.Configuration;

namespace AssetManager {
	public class Thumbnail : IHttpHandler {

		public void ProcessRequest (HttpContext context) {
            var defaultMimePath = VirtualPathUtility.ToAbsolute(ConfigurationManager.AppSettings["Assets_DefaultThumbPath"]);
			context.Response.ContentType = "image/jpg";
			context.Response.WriteFile(VirtualPathUtility.RemoveTrailingSlash(defaultMimePath) + "/" + "default_thumb.jpg");
		}

		public bool IsReusable { get { return false; } }
	}
}
