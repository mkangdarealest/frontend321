using frontend.Models;
using System.Data.Entity; // <-- Required for .Include()
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace frontend.Controllers
{
    // Secure this entire controller for Admins
    [Authorize(Roles = "Admin")]
    public class AdminCustomersController : Controller
    {
        private LongChauDbEntities db = new LongChauDbEntities();

        // GET: AdminCustomers
        // Checklist: "Trang Index: lưu giữ danh sách khách hàng, không có nút Thêm/Sửa/Xóa"
        public ActionResult Index()
        {
            var customers = db.Customers.OrderBy(c => c.LastName).ToList();
            return View(customers);
        }

        // GET: AdminCustomers/Details/5
        // Checklist: "Trang Detail: hiển thị thông tin khách hàng + danh sách đơn hàng của khách hàng"
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Find the customer and "eagerly load" their related Orders
            // We also include the OrderStatu for each order
            Customer customer = db.Customers
                .Include(c => c.Orders.Select(o => o.OrderStatu))
                .FirstOrDefault(c => c.Id == id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            // Pass the single customer object (which now contains all their orders) to the view
            return View(customer);
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