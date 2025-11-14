using frontend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity; // <-- Make sure this is included

namespace frontend.Controllers
{
    // Secure this entire controller for Admins
    [Authorize(Roles = "Admin")]
    public class AdminStockController : Controller
    {
        private LongChauDbEntities db = new LongChauDbEntities();

        // GET: AdminStock/Index
        public ActionResult Index()
        {
            // 1. Get all products, but include Images to show a thumbnail
            var allProducts = db.Products
                                .Include(p => p.ProductImages)
                                .OrderBy(p => p.Name)
                                .ToList();

            // 2. Create the ViewModel
            var model = new AdminStockViewModel
            {
                // List 1: "Hàng tồn" (In-stock)
                InStockProducts = allProducts.Where(p => p.Quantity > 0).ToList(),

                // List 2: "Hàng cần nhập" (Out-of-stock)
                OutOfStockProducts = allProducts.Where(p => p.Quantity == 0).ToList()
            };

            // 3. Pass the single ViewModel (which contains both lists) to the View
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}