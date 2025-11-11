using frontend.Models;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security; // Required for Forms Authentication

namespace frontend.Controllers
{
    public class AccountController : Controller
    {
        // Database context
        private LongChauDbEntities db = new LongChauDbEntities();

        // GET: Account/Login
        // This action shows the login page
        [AllowAnonymous] // Allow anyone to see the login page
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        // This action handles the form submission
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Admin model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // -----------------------------------------------------------------
            // WARNING: This checks for plain text passwords.
            // This is INSECURE and only for learning (based on your doc).
            // For a real app, you MUST hash and salt your passwords.
            // -----------------------------------------------------------------
            var user = db.Admins.FirstOrDefault(u => u.Username == model.Username && u.PasswordUser == model.PasswordUser);

            if (user != null)
            {
                // Set the authentication cookie
                FormsAuthentication.SetAuthCookie(user.Username, false); // "false" = don't remember me

                // Redirect to the admin page or the page they were trying to access
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    // Default redirect to the Admin Product List
                    return RedirectToAction("Index", "Products");
                }
            }
            else
            {
                // If login fails, show the form again with an error
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Sign the user out
            FormsAuthentication.SignOut();
            // Send them back to the public home page
            return RedirectToAction("LongChauClone", "TrangChu");
        }
    }
}