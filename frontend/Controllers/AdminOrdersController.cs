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
}