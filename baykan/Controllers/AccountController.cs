using System;
using System.Linq;
using System.Web.Mvc;
using baykan.Models;

namespace baykan.Controllers
{
    public class AccountController : Controller
    {
        private ecdataEntities db = new ecdataEntities();

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string UserType, string Name, string Email, string Password, string Address)
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.ErrorMessage = "All fields are required!";
                return View();
            }

            if (UserType == "Customer")
            {
                // Check if email is already registered
                var existingCustomer = db.Customers.FirstOrDefault(c => c.CustomerEmail == Email);
                if (existingCustomer != null)
                {
                    ViewBag.ErrorMessage = "Email is already registered!";
                    return View();
                }

                var newCustomer = new Customer
                {
                    CustomerName = Name,
                    CustomerEmail = Email,
                    CustomerPassword = Password,
                    CustomerAddress = Address
                };

                db.Customers.Add(newCustomer);
            }
            else if (UserType == "Merchant")
            {
                // Check if username is already taken
                var existingMerchant = db.Merchants.FirstOrDefault(m => m.MerchantUsername == Name);
                if (existingMerchant != null)
                {
                    ViewBag.ErrorMessage = "Username is already taken!";
                    return View();
                }

                var newMerchant = new Merchant
                {
                    MerchantUsername = Name,
                    MerchantPassword = Password
                };

                db.Merchants.Add(newMerchant);
            }

            try
            {
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error saving to the database: " + ex.Message;
                return View();
            }
        }


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
