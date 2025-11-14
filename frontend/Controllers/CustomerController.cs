using frontend.Models;
using System.Data.Entity;
using System.Linq;
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
}