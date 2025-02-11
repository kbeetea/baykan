using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using baykan.Models;


namespace baykan.Controllers
{
    public class MerchantController : Controller
    {
        ecdataEntities db = new ecdataEntities();


        /*public ActionResult MerchantHome()
        {
            if (Session["Role"]?.ToString() != "Merchant" || Session["MerchantId"] == null)
                return RedirectToAction("Index", "Home");

            int merchantId = Convert.ToInt32(Session["MerchantId"]);

            var products = db.Products.Where(p => p.MerchantId == merchantId).ToList();

            return View(products);
        }*/

        public ActionResult MerchantHome()
        {
            if (Session["Role"]?.ToString() != "Merchant")
                return RedirectToAction("Index", "Home");

            int merchantId = Convert.ToInt32(Session["UserId"]);
            var products = db.Products.Where(p => p.MerchantId == merchantId).ToList(); // Fetch only merchant's products
            var lowStockProducts = products.Where(p => p.StockQuantity < 5).ToList(); // Products with low stock

            ViewBag.LowStockProducts = lowStockProducts;
            return View(products);
        }


        // Ensure only merchants can access this page

        public ActionResult Index()
        {
            if (Session["Role"]?.ToString() != "Merchant")
                return RedirectToAction("Index", "Home");

            int merchantId = Convert.ToInt32(Session["UserId"]);
            var products = db.Products.Where(p => p.MerchantId == merchantId).ToList();

            return View(products);
        }



        // GET: Merchant/Create (Load Create Product Page)
        [HttpGet]
        public ActionResult Create()
        {
            if (Session["Role"]?.ToString() != "Merchant")
                return RedirectToAction("Login", "Account");

            ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName");
            ViewBag.Brands = new SelectList(db.Brands, "BrandId", "BrandName");

            return View();
        }

        // POST: Merchant/Create (Handle Product Submission)
        [HttpPost]
        public ActionResult Create(Product product, string CategoryName, string BrandName, HttpPostedFileBase ImageFile)
        {
            if (Session["Role"]?.ToString() != "Merchant")
                return RedirectToAction("Login", "Account");

            if (Session["UserId"] == null)
            {
                ModelState.AddModelError("", "Merchant ID is missing from the session.");
                return View(product);
            }

            int merchantId = Convert.ToInt32(Session["UserId"]);
            Console.WriteLine("Merchant ID from session: " + merchantId); // Debugging line

            product.MerchantId = merchantId; // ✅ Assign the logged-in merchant's ID

            if (!string.IsNullOrEmpty(CategoryName))
            {
                var existingCategory = db.Categories.FirstOrDefault(c => c.CategoryName == CategoryName);
                if (existingCategory == null)
                {
                    var newCategory = new Category { CategoryName = CategoryName };
                    db.Categories.Add(newCategory);
                    db.SaveChanges();
                    product.CategoryId = newCategory.CategoryId;
                }
                else
                {
                    product.CategoryId = existingCategory.CategoryId;
                }
            }

            if (!string.IsNullOrEmpty(BrandName))
            {
                var existingBrand = db.Brands.FirstOrDefault(b => b.BrandName == BrandName);
                if (existingBrand == null)
                {
                    var newBrand = new Brand { BrandName = BrandName, CategoryId = product.CategoryId };
                    db.Brands.Add(newBrand);
                    db.SaveChanges();
                    product.BrandId = newBrand.BrandId;
                }
                else
                {
                    product.BrandId = existingBrand.BrandId;
                }
            }

            if (product.CategoryId == 0 || product.BrandId == 0)
            {
                ModelState.AddModelError("", "Category and Brand are required.");
                ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName");
                ViewBag.Brands = new SelectList(db.Brands, "BrandId", "BrandName");
                return View(product);
            }

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                using (var binaryReader = new BinaryReader(ImageFile.InputStream))
                {
                    product.ProductImage = binaryReader.ReadBytes(ImageFile.ContentLength);
                }
            }

            try
            {
                db.Products.Add(product);
                db.SaveChanges();
                Console.WriteLine("Product created successfully with MerchantId: " + product.MerchantId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(product);
            }

            return RedirectToAction("Index");
        }


        public ActionResult Edit(long id)
        {
            Product product = db.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewBag.Brands = new SelectList(db.Brands, "BrandId", "BrandName", product.BrandId);

            return View(product);
        }

        [HttpPost]
        public ActionResult Edit(Product P, HttpPostedFileBase ImageFile)
        {
            Product product = db.Products.FirstOrDefault(p => p.ProductId == P.ProductId);
            if (product == null)
            {
                return HttpNotFound();
            }

            product.ProductName = P.ProductName;
            product.ProductDescription = P.ProductDescription;
            product.ProductPrice = P.ProductPrice;
            product.CategoryId = P.CategoryId;
            product.BrandId = P.BrandId;

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                using (var binaryReader = new System.IO.BinaryReader(ImageFile.InputStream))
                {
                    product.ProductImage = binaryReader.ReadBytes(ImageFile.ContentLength);
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            ecdataEntities db = new ecdataEntities();
            Product product = db.Products.FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            ecdataEntities db = new ecdataEntities();
            Product product = db.Products.FirstOrDefault(p => p.ProductId == id);

            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }


        public ActionResult Details(int id)
        {
            ecdataEntities db = new ecdataEntities();
            Product product = db.Products
                .Include("Category")
                .Include("Brand")
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

    }
}
