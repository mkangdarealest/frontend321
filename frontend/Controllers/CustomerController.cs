using frontend.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.IO;
using System.Web;
// 1. We protect the ENTIRE controller.
// Only logged-in users can access any page here.
[Authorize(Roles = "Customer")]
public class CustomerController : Controller
{
    private LongChauDbEntities db = new LongChauDbEntities();

    // GET: Customer/Index
    // This will be our "User Settings" page
    public ActionResult Index()
    {
        // 1. Get the username of the person who is currently logged in.
        var currentUsername = User.Identity.Name;

        // 2. Find that user's record in the database.
        var customer = db.Customers.FirstOrDefault(c => c.UserName == currentUsername);

        if (customer == null)
        {
            // This should not happen if they are logged in, but it's good to check.
            return HttpNotFound();
        }

        // 3. Pass that user's data to the view.
        return View(customer);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    // THÊM HttpPostedFileBase AvatarFile
    public ActionResult Index(Customer model, HttpPostedFileBase AvatarFile)
    {
        if (ModelState.IsValid)
        {
            var userToUpdate = db.Customers.Find(model.Id);
            if (userToUpdate == null)
            {
                return HttpNotFound();
            }

            // Cập nhật thông tin text
            userToUpdate.FirstName = model.FirstName;
            userToUpdate.LastName = model.LastName;
            userToUpdate.Email = model.Email;
            userToUpdate.Phone = model.Phone;
            userToUpdate.AddressLine = model.AddressLine;
            userToUpdate.City = model.City;
            userToUpdate.District = model.District;
            userToUpdate.PostalCode = model.PostalCode;

            // --- BẮT ĐẦU LOGIC UPLOAD ẢNH MỚI ---
            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                // Xóa ảnh cũ nếu có (tránh rác server)
                if (!string.IsNullOrEmpty(userToUpdate.AvatarUrl))
                {
                    var oldPath = Server.MapPath(userToUpdate.AvatarUrl);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Lưu ảnh mới
                string fileName = Path.GetFileNameWithoutExtension(AvatarFile.FileName);
                string extension = Path.GetExtension(AvatarFile.FileName);
                fileName = fileName + "_" + userToUpdate.Id + extension; // Tạo tên file unique

                string savePath = Path.Combine(Server.MapPath("~/hinh/avatars/"), fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)); // Tạo thư mục nếu chưa có
                AvatarFile.SaveAs(savePath);

                // Cập nhật URL trong database
                userToUpdate.AvatarUrl = "~/hinh/avatars/" + fileName;
            }
            db.Entry(userToUpdate).State = EntityState.Modified;
            db.SaveChanges();

            // Cập nhật avatar trong Session
            Session["UserAvatar"] = userToUpdate.AvatarUrl;

            ViewBag.SuccessMessage = "Cập nhật thông tin thành công!";
            return View(userToUpdate);
        }

        return View(model);
    }
    // GET: Customer/OrderHistory
    public ActionResult OrderHistory()
    {
        // Get the current user
        var currentUsername = User.Identity.Name;
        var customer = db.Customers.FirstOrDefault(c => c.UserName == currentUsername);
        if (customer == null)
        {
            return HttpNotFound();
        }

        // Get all orders for THIS customer
        // We include OrderStatu to show "Pending", "Completed", etc.
        var orders = db.Orders
            .Include(o => o.OrderStatu)
            .Where(o => o.CustomerId == customer.Id)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View(orders); // Pass the list of orders to a new view
    }

    // GET: Customer/OrderDetails/5
    public ActionResult OrderDetails(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // Get the current user (for security)
        var currentUsername = User.Identity.Name;
        var customerId = db.Customers.FirstOrDefault(c => c.UserName == currentUsername)?.Id;

        // Find the order, including its items
        var order = db.Orders
            .Include(o => o.OrderItems.Select(oi => oi.Product)) // Include OrderItems
            .Include(o => o.Customer) // Include the customer info
            .Include(o => o.OrderStatu) // Include the status
            .FirstOrDefault(o => o.Id == id);

        if (order == null)
        {
            return HttpNotFound();
        }

        // SECURITY CHECK: Ensure the logged-in user is the one who owns this order
        if (order.CustomerId != customerId)
        {
            return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You do not have access to this order.");
        }

        return View(order); // Pass the single order to a new view
    }
}