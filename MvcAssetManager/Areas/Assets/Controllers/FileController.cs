using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcAssetManager.Areas.Assets.Controllers
{
    public class FileController : Controller
    {
        //
        // GET: /Assets/File/

        public ActionResult Index()
        {
            return View();
        }

    }
}
