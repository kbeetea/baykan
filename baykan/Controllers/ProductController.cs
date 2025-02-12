using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using baykan.Models;

public class ProductsController : Controller
{
    private ecdataEntities db = new ecdataEntities();

    public ActionResult Index(int page = 1, int pageSize = 8)
    {
        var products = db.Products.OrderBy(p => p.ProductId).ToList(); // Fetch products

        if (products == null || !products.Any())
        {
            products = new List<Product>(); // Ensure the list is never null
        }

        int totalProducts = products.Count;
        var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

        return View(pagedProducts);
    }

    public ActionResult Details(int id)
    {
        var product = db.Products.FirstOrDefault(p => p.ProductId == id);

        if (product == null)
        {
            return HttpNotFound();
        }

        // Manually load the Category (if needed)
        db.Entry(product).Reference(p => p.Category).Load();

        // Get related products from the same category (excluding the current product)
        var relatedProducts = db.Products
            .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
            .OrderBy(x => Guid.NewGuid()) // Randomize the selection
            .Take(4) // Show up to 4 related products
            .ToList();

        ViewBag.RelatedProducts = relatedProducts;
        return View(product);
    }




}
