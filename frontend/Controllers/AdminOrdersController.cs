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
    public ActionResult Index()
    {
        // Get all orders, include the Customer and Status
        var orders = db.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderStatu)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

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
        // This fulfills the "Tạo chung 1 Partial View" checklist item.
        return View("~/Views/Customer/OrderDetails.cshtml", order);
    }
    public ActionResult Edit(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // Get the order and its related info
        Order order = db.Orders
            .Include(o => o.Customer)
            .FirstOrDefault(o => o.Id == id);

        if (order == null)
        {
            return HttpNotFound();
        }

        // Get all possible statuses (e.g., "Pending", "Delivered")
        // and create a SelectList for the dropdown.
        // We pass "Id", "Name", and the "order.StatusId" as the currently selected value.
        ViewBag.StatusList = new SelectList(db.OrderStatus.ToList(), "Id", "Name", order.StatusId);

        return View(order);
    }

    // POST: AdminOrders/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, int StatusId) 
    {
        // 1. Find the original order in the database
        var orderToUpdate = db.Orders.Find(id);
        if (orderToUpdate == null)
        {
            return HttpNotFound();
        }

        // 2. Update ONLY the StatusId
        orderToUpdate.StatusId = StatusId;

        // 3. Mark it as modified and save
        db.Entry(orderToUpdate).State = EntityState.Modified;
        db.SaveChanges();

        // 4. Send the admin back to the order list
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
