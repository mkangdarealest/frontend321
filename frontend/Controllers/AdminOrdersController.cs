using frontend.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

// 1. Secure the ENTIRE controller for Admins only
[Authorize(Roles = "Admin")]
public class AdminOrdersController : Controller
{
    private LongChauDbEntities db = new LongChauDbEntities();

    // 2. (Checklist) Trang Index: lưu giữ danh sách đơn hàng
    // GET: /AdminOrders/Index
    public ActionResult Index(int? statusId)
    {
        // 1. Start the query
        var ordersQuery = db.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderStatu)
            .AsQueryable();

        // 2. Apply Filter if selected
        if (statusId.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.StatusId == statusId.Value);
        }

        // 3. Get data for the Dropdown (ViewBag)
        ViewBag.StatusList = db.OrderStatus.ToList();
        ViewBag.CurrentStatus = statusId;

        // 4. Execute and Return
        var orders = ordersQuery.OrderByDescending(o => o.OrderDate).ToList();
        return View(orders);
    }

    // 3. (Checklist) Trang Detail: hiển thị Hóa đơn mua hàng
    // GET: /AdminOrders/Details/5
    public ActionResult Details(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        var order = db.Orders
            .Include(o => o.OrderItems.Select(oi => oi.Product)) // Include OrderItems
            .Include(o => o.Customer) // Include the customer info
            .Include(o => o.OrderStatu) // Include the status
            .FirstOrDefault(o => o.Id == id);

        if (order == null)
        {
            return HttpNotFound();
        }

        // We re-use the *exact same view* as the customer's order details
        return View(order);
    }
    public ActionResult Edit(int? id)
    {
        if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        var order = db.Orders.Include(o => o.Customer).FirstOrDefault(o => o.Id == id);
        if (order == null) return HttpNotFound();

        // --- LOCK CHECK ---
        // If order is already Completed (3) or Cancelled (4), we might want to warn the user
        // or disable the dropdown in the View.
        // We pass this status list as usual.
        ViewBag.StatusList = new SelectList(db.OrderStatus.ToList(), "Id", "Name", order.StatusId);

        return View(order);
    }

    // POST: AdminOrders/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, int StatusId)
    {
        var orderToUpdate = db.Orders.Find(id);
        if (orderToUpdate == null) return HttpNotFound();

        // --- LOGIC: PREVENT CHANGING "COMPLETED" ORDERS ---
        if (orderToUpdate.StatusId == 3) // 3 = Completed
        {
            // If it was ALREADY completed, you cannot change it back or cancel it.
            // We return the view with an error message.
            TempData["Error"] = "Đơn hàng này đã hoàn thành. Không thể thay đổi trạng thái được nữa.";
            return RedirectToAction("Index");
        }

        // Allow update
        orderToUpdate.StatusId = StatusId;
        db.Entry(orderToUpdate).State = EntityState.Modified;
        db.SaveChanges();

        TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
        return RedirectToAction("Index");
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
