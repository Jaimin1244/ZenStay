using Learn_Auth.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Learn_Auth.Models;
using Learn_Auth.ViewModel;

namespace Learn_Auth.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuthDbContext _context = new AuthDbContext();
        public ActionResult Index()
        {
            ViewBag.ActivePage = "Home";
            var hotels = _context.Hotels.ToList();
            return View(hotels);
        }

        public ActionResult About()
        {
            ViewBag.ActivePage = "About";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.ActivePage = "Contact";

            return View();
        }
    }
}