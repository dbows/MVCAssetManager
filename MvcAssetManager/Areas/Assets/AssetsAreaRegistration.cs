using System.Web.Mvc;

namespace MvcAssetManager.Areas.Assets
{
    public class AssetsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Assets";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Assets_default",
                "Assets/{controller}/{action}/{id}",
                new {controller="File", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
