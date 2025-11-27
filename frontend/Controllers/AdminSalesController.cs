using frontend.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Collections.Generic;

namespace frontend.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSalesController : Controller
    {
        private LongChauDbEntities db = new LongChauDbEntities();

        // GET: AdminSales
        // Merged Index with Tabs
        public ActionResult Index(string activeTab, int? statusId)
        {
            // Default tab is "orders"
            if (string.IsNullOrEmpty(activeTab)) activeTab = "orders";
            ViewBag.ActiveTab = activeTab;

            // --- DATA FOR TAB 1: ORDERS ---
            var ordersQuery = db.Orders.Include(o => o.Customer).Include(o => o.OrderStatu).AsQueryable();
            if (statusId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.StatusId == statusId.Value);
            }
            ViewBag.Orders = ordersQuery.OrderByDescending(o => o.OrderDate).ToList();

            ViewBag.StatusList = db.OrderStatus.ToList(); // For the dropdown
            ViewBag.CurrentStatus = statusId;

            // --- DATA FOR TAB 2: CUSTOMERS ---
            // Only fetch if tab is active to save performance (optional optimization)
            ViewBag.Customers = db.Customers.OrderByDescending(c => c.CreatedAt).ToList();

            return View();
        }

        // ==========================
        // ORDER ACTIONS
        // ==========================

        public ActionResult OrderDetails(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var order = db.Orders.Include(o => o.OrderItems.Select(p => p.Product))
                                 .Include(o => o.Customer).Include(o => o.OrderStatu)
                                 .FirstOrDefault(o => o.Id == id);
            if (order == null) return HttpNotFound();

            // Reuse your existing Admin Invoice partial
            return View(order);
        }

        public ActionResult OrderEdit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var order = db.Orders.Include(o => o.Customer).FirstOrDefault(o => o.Id == id);
            if (order == null) return HttpNotFound();

            ViewBag.StatusList = new SelectList(db.OrderStatus.ToList(), "Id", "Name", order.StatusId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OrderEdit(int id, int StatusId)
        {
            var orderToUpdate = db.Orders.Find(id);
            if (orderToUpdate.StatusId == 3) // Locked if Completed
            {
                TempData["Error"] = "Đơn hàng đã hoàn thành, không thể sửa.";
                return RedirectToAction("Index", new { activeTab = "orders" });
            }

            orderToUpdate.StatusId = StatusId;
            db.Entry(orderToUpdate).State = EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật đơn hàng thành công!";
            return RedirectToAction("Index", new { activeTab = "orders" });
        }

        // ==========================
        // CUSTOMER ACTIONS
        // ==========================

        public ActionResult CustomerDetails(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var customer = db.Customers.Include(c => c.Orders.Select(o => o.OrderStatu))
                                       .FirstOrDefault(c => c.Id == id);
            if (customer == null) return HttpNotFound();

            return View(customer);
        }
    }
}