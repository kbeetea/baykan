using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using baykan.Models;

namespace baykan.Controllers
{
    public class CartController : Controller
    {
        private ecdataEntities db = new ecdataEntities();
        List<Cart> cartItems = new List<Cart>();
        // View Cart
        public ActionResult Index()
        {
            List<Cart> cartItems = new List<Cart>();

            if (Session["UserId"] != null && Session["Role"]?.ToString() == "Customer")
            {
                int customerId = (int)Session["UserId"];
                cartItems = db.Carts.Where(c => c.CustomerId == customerId).ToList();
            }

            return View(cartItems);
        }

        // Add to Cart
        [HttpPost]
        public JsonResult AddToCart(int productId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { success = false, showLoginModal = true });
            }

            int customerId = (int)Session["UserId"];
            var existingCartItem = db.Carts.FirstOrDefault(c => c.CustomerId == customerId && c.ProductId == productId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1; // Increase quantity if already in cart
            }
            else
            {
                db.Carts.Add(new Cart { CustomerId = customerId, ProductId = productId, Quantity = 1 });
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        // Update Cart
        [HttpPost]
        public ActionResult UpdateCart(int cartId, int quantity)
        {
            var cartItem = db.Carts.Find(cartId);
            if (cartItem != null && quantity > 0)
            {
                cartItem.Quantity = quantity;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Remove from Cart
        public ActionResult RemoveFromCart(int cartId)
        {
            var cartItem = db.Carts.Find(cartId);
            if (cartItem != null)
            {
                db.Carts.Remove(cartItem);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public JsonResult GetCartCount()
        {
            int userId = Convert.ToInt32(Session["UserId"]); // Ensure UserId is available
            if (userId == 0)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }

            var cartItems = db.Carts.Where(c => c.CustomerId == userId).ToList();
            int count = (int)cartItems.Sum(c => c.Quantity);

            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to full login page
            }

            int userId = (int)Session["UserId"];
            var customer = db.Customers.Find(userId);
            if (customer == null) return RedirectToAction("Index", "Home");

            var cartItems = db.Carts.Where(c => c.CustomerId == userId).ToList();
            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            ViewBag.CustomerName = customer.CustomerName;
            ViewBag.CustomerAddress = customer.CustomerAddress;
            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = cartItems.Sum(c => c.Quantity * (c.Product?.ProductPrice ?? 0));

            return View();
        }



        [HttpPost]
        public ActionResult Checkout(string fullName, string address, string paymentMethod, bool updateAddress)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            var customer = db.Customers.Find(userId);
            if (customer == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (updateAddress)
            {
                customer.CustomerAddress = address; // Update the address in the database
                db.SaveChanges();
            }

            var cartItems = db.Carts
                .Where(c => c.CustomerId == userId)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                return Json(new { success = false, message = "Your cart is empty." });
            }

            decimal totalAmount = (decimal)cartItems.Sum(c => c.Quantity * (c.Product?.ProductPrice ?? 0));

            // Create Order
            Order order = new Order
            {
                CustomerId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                OrderStatus = "Pending"
            };

            db.Orders.Add(order);
            db.SaveChanges();

            // Save Order Details
            foreach (var item in cartItems)
            {
                OrderDetail orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product?.ProductPrice ?? 0
                };
                db.OrderDetails.Add(orderDetail);
            }

            // Clear the cart
            db.Carts.RemoveRange(cartItems);
            db.SaveChanges();

            return Json(new { success = true, orderId = order.OrderId });
        }

        public ActionResult OrderConfirmation(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var pastOrders = db.Orders
                .Where(o => o.CustomerId == order.CustomerId && o.OrderId != id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.OrderId = order.OrderId;
            ViewBag.OrderDate = order.OrderDate;
            ViewBag.PastOrders = pastOrders;

            return View();
        }

        public ActionResult PastOrders()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login page
            }

            int userId = (int)Session["UserId"];
            var orders = db.Orders
                .Where(o => o.CustomerId == userId)
                .Include(o => o.OrderDetails.Select(od => od.Product)) // Load order details & products
                .ToList();

            return View(orders);
        }


    }
}
