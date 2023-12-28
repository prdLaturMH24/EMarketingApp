using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EMarketingApp.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult About() //View Path= Home/About (About)
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact() //View Path= Home/Contact (Contact)
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Error(ArgumentNullException ex)
        {
            ViewBag.Message = ex.Message;
            ViewBag.Error = ex.StackTrace ?? "";
            return View();
        }
    }
}