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
    public ActionResult Index(Customer model, HttpPostedFileBase AvatarFile)
    {
        // --- NEW VALIDATION LOGIC ---
        // Check if the user is trying to change their password
        if (!string.IsNullOrEmpty(model.Password) || !string.IsNullOrEmpty(model.ConfirmPassword))
        {
            // If they are, manually check if the passwords match
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu và mật khẩu xác nhận không khớp.");
            }

            // Also, manually check if the new password meets length requirements
            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 12)
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 12 ký tự.");
            }
        }
        else
        {
            // If both fields are empty, the user is NOT changing their password.
            // We must remove any errors from the 'Password' and 'ConfirmPassword' fields
            // so that ModelState.IsValid becomes true.
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
        }
        // --- END NEW VALIDATION LOGIC ---

        if (ModelState.IsValid)
        {
            // 1. Load the original user from the database
            var userToUpdate = db.Customers.Find(model.Id);
            if (userToUpdate == null)
            {
                return HttpNotFound();
            }

            // 2. Update ALL text fields
            userToUpdate.FirstName = model.FirstName;
            userToUpdate.LastName = model.LastName;
            userToUpdate.Email = model.Email;
            userToUpdate.Phone = model.Phone;
            userToUpdate.AddressLine = model.AddressLine;
            userToUpdate.City = model.City;
            userToUpdate.District = model.District;
            userToUpdate.PostalCode = model.PostalCode;

            // 3. --- PASSWORD LOGIC ---
            // Only update the password IF the user entered a new one.
            if (!string.IsNullOrEmpty(model.Password))
            {
                // In a real app, you MUST HASH this password.
                userToUpdate.Password = model.Password;
            }

            // 4. --- AVATAR LOGIC ---
            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                if (!string.IsNullOrEmpty(userToUpdate.AvatarUrl))
                {
                    var oldPath = Server.MapPath(userToUpdate.AvatarUrl);
                    if (System.IO.File.Exists(oldPath)) { System.IO.File.Delete(oldPath); }
                }
                string fileName = Path.GetFileNameWithoutExtension(AvatarFile.FileName) + "_" + userToUpdate.Id + Path.GetExtension(AvatarFile.FileName);
                string savePath = Path.Combine(Server.MapPath("~/hinh/avatars/"), fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                AvatarFile.SaveAs(savePath);
                userToUpdate.AvatarUrl = "~/hinh/avatars/" + fileName;

                // Update avatar in session
                Session["UserAvatar"] = userToUpdate.AvatarUrl;
            }

            // 5. Save all changes
            db.Entry(userToUpdate).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.SuccessMessage = "Cập nhật thông tin thành công!";

            return View(userToUpdate);
        }

        // If model is not valid, return the form with errors
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