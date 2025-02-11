using System;
using System.Linq;
using System.Web.Mvc;
using baykan.Models;

namespace baykan.Controllers
{
    public class HomeController : Controller
    {
        private ecdataEntities db = new ecdataEntities();

        public ActionResult Index()
        {
            var featuredProducts = db.Products.Take(4).ToList();
            return View(featuredProducts);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}
