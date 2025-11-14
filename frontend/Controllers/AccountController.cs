using frontend.Models;
using System;
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
            // We remove the [Bind] attribute and manually check the model
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.PasswordUser))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View(model);
            }

            // WARNING: Still checking plain text password.
            var user = db.Admins.FirstOrDefault(u => u.Username == model.Username && u.PasswordUser == model.PasswordUser);

            if (user != null)
            {
                // --- THIS IS THE NEW PART ---
                // 1. Create a "ticket" that holds the user's name AND their role
                var ticket = new FormsAuthenticationTicket(
                    1,                                  // version
                    user.Username,                      // user name
                    DateTime.Now,                       // issue time
                    DateTime.Now.AddMinutes(30),        // expiration
                    false,                              // isPersistent
                    user.Role                 // <-- THIS IS THE ROLE
                );

                // 2. Encrypt the ticket
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                // 3. Create a cookie to hold the encrypted ticket
                var authCookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                // 4. Add the cookie to the response
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
                // --- END NEW PART ---

                // Redirect to the admin page or the page they were trying to access
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Products");
                }
            }
            else
            {
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