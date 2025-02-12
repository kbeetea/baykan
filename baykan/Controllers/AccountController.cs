using System;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using baykan.Models;

namespace baykan.Controllers
{
    public class AccountController : Controller
    {
        private ecdataEntities db = new ecdataEntities();

        public ActionResult Login()
        {
            return View();
        }

        // POST: Login Logic (Handles AJAX login)
        [HttpPost]
        public JsonResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Json(new { success = false, message = "Please enter both username and password!" });
            }

            // Check Merchant Login
            var merchant = db.Merchants.FirstOrDefault(m => m.MerchantUsername == username && m.MerchantPassword == password);
            if (merchant != null)
            {
                Session["UserId"] = merchant.MerchantId;
                Session["Username"] = merchant.MerchantUsername;
                Session["Role"] = "Merchant";
                return Json(new { success = true, redirectUrl = Url.Action("MerchantHome", "Merchant") });
            }

            var user = db.Customers.FirstOrDefault(u => u.CustomerEmail == email && u.CustomerPassword == password);
            if (user != null)
            {
                Session["UserId"] = user.CustomerId;
                Session["Username"] = user.CustomerName;
                Session["Role"] = "Customer";
                return Json(new { success = true });
            }
            return Json(new { success = false });

            // Invalid login, return error message
            return Json(new { success = false, message = "Invalid username or password!" });
        }

        // Logout Functionality
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}
