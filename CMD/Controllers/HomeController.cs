using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using CMD.SystemClass;

namespace CMD.Controllers
{
    public class HomeController : Controller
    {
        [CheckSessionFilterAttribute]
        [CustomAuthorize]   
        public ActionResult Index(string s_class)
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}