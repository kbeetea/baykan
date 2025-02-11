using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using baykan.Models;
using db = baykan.Models;

namespace baykan.Controllers
{
    public class AccountController : Controller
    {
        private ecdataEntities db = new ecdataEntities();

        // GET: Login Page
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login Logic
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Check Merchant Login
            var merchant = db.Merchants.FirstOrDefault(m => m.MerchantUsername == username && m.MerchantPassword == password);
            if (merchant != null)
            {
                Session["UserId"] = merchant.MerchantId;
                Session["Username"] = merchant.MerchantUsername;
                Session["Role"] = "Merchant";
                return Json(new { success = true, redirectUrl = Url.Action("MerchantHome", "Merchant") });
            }

            // Check Customer Login
            var customer = db.Customers.FirstOrDefault(c => c.CustomerEmail == username && c.CustomerPassword == password);
            if (customer != null)
            {
                Session["UserId"] = customer.CustomerId;
                Session["Username"] = customer.CustomerName;
                Session["Role"] = "Customer";
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }

            // Invalid login, return error message
            return Json(new { success = false, message = "Invalid username or password!" });
        }


        // Logout Functionality
        public ActionResult Logout()
        {
            Session.Clear(); // Clear session
            Session.Abandon(); // End session
            return RedirectToAction("Index", "Home");
        }
    }
}
