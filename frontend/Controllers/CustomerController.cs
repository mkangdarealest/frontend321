using frontend.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

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
    public ActionResult Index(Customer model)
    {
        if (ModelState.IsValid)
        {
            // 1. Load the original user from the database
            var userToUpdate = db.Customers.Find(model.Id);

            if (userToUpdate == null)
            {
                return HttpNotFound();
            }

            // 2. Update ONLY the fields we want to allow changing
            userToUpdate.FirstName = model.FirstName;
            userToUpdate.LastName = model.LastName;
            userToUpdate.Email = model.Email;
            userToUpdate.Phone = model.Phone;
            userToUpdate.AddressLine = model.AddressLine;

            // 3. Mark as modified and save
            db.Entry(userToUpdate).State = EntityState.Modified;
            db.SaveChanges();

            // 4. Show a success message (optional)
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