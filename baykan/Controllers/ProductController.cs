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
        var product = db.Products.Find(id);
        if (product == null)
        {
            return HttpNotFound();
        }
        return View(product);
    }
}
