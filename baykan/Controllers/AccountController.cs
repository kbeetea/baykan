using System;
using System.Linq;
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
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Please enter both username and password!";
                return View();
            }

            // Check Merchant Login
            var merchant = db.Merchants.FirstOrDefault(m => m.MerchantUsername == username && m.MerchantPassword == password);
            if (merchant != null)
            {
                Session["UserId"] = merchant.MerchantId;
                Session["Username"] = merchant.MerchantUsername;
                Session["Role"] = "Merchant";
                return RedirectToAction("MerchantHome", "Merchant");
            }

            // Check Customer Login
            var customer = db.Customers.FirstOrDefault(c => c.CustomerEmail == username && c.CustomerPassword == password);
            if (customer != null)
            {
                Session["UserId"] = customer.CustomerId;
                Session["Username"] = customer.CustomerName;
                Session["Role"] = "Customer";
                return RedirectToAction("Index", "Home");
            }

            // Invalid login, show error message
            ViewBag.ErrorMessage = "Invalid username or password!";
            return View();
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
