using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using baykan.Models;

public class ProductsController : Controller
{
    private ecdataEntities db = new ecdataEntities();

    public ActionResult Index(int? categoryId, int page = 1, int pageSize = 8)
    {
        var products = db.Products.AsQueryable();

        // If a category is selected, filter products
        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value);
        }

        int totalProducts = products.Count();
        var pagedProducts = products
                            .OrderBy(p => p.ProductId)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
        ViewBag.SelectedCategory = categoryId;

        // Fetch categories for the category menu
        ViewBag.Categories = db.Categories.ToList();

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
