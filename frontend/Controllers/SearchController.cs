using frontend.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace frontend.Controllers
{
    public class SearchController : Controller
    {
        // 1. Add your database context
        private LongChauDbEntities db = new LongChauDbEntities();

        // 2. Create the Index action that accepts "string q"
        public ActionResult Index(string q)
        {
            // Start by getting all products, including their images
            var products = db.Products.Include(p => p.ProductImages).AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                // Find any product whose Name, Brand, or Description contains the query
                products = products.Where(p =>
                    p.Name.Contains(q) ||
                    p.Brand.Contains(q) ||
                    p.ShortDescription.Contains(q)
                );
            }

            // This passes the search term to the results page
            ViewBag.CurrentQuery = q;

            // Pass the final list of found products to the new view
            return View(products.ToList());
        }
    }
}