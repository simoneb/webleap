using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebLeap.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
	        ViewBag.WebSocketUrl = MvcApplication.WebSocketServer.Location;
            return View();
        }

    }
}
