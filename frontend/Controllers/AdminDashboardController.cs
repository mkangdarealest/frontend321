using frontend.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace frontend.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private LongChauDbEntities db = new LongChauDbEntities();

        // GET: AdminDashboard
        public ActionResult Index()
        {
            // --- 1. KEY METRICS ---
            // Total Revenue (Only from Completed orders, StatusId = 3)
            decimal totalRevenue = db.Orders
                                     .Where(o => o.StatusId == 3)
                                     .Sum(o => (decimal?)o.Total) ?? 0;

            // Counts
            int totalOrders = db.Orders.Count();
            int pendingOrders = db.Orders.Count(o => o.StatusId == 1); // 1 = Pending
            int totalCustomers = db.Customers.Count();
            int lowStockProducts = db.Products.Count(p => p.Quantity < 10); // Alert threshold

            // --- 2. PASS DATA TO VIEW ---
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.LowStockCount = lowStockProducts;

            // --- 3. DATABASE STATUS (Fun Info) ---
            ViewBag.DbName = db.Database.Connection.Database;
            ViewBag.DbServer = db.Database.Connection.DataSource;
            ViewBag.DbStatus = db.Database.Exists() ? "Online" : "Offline";

            return View();
        }
    }
}