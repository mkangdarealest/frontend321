using frontend.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;

namespace frontend.Controllers
{
    public class TrangChuController : Controller
    {
        //Database to View
        LongChauDbEntities db = new LongChauDbEntities();
        public ActionResult LongChauClone()
        {
            var products = db.Products.Include(p => p.ProductImages).ToList();
            return View(products);
        }
        // GET: TrangChu/ViewProduct/your-product-slug
        public ActionResult ViewProduct(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Find the product by its slug, AND include all its related data
            Product product = db.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .SingleOrDefault(p => p.Slug == slug);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Go directly to the "Details" view and pass it the product
            // The URL in the browser STAYS as .../ViewProduct/your-product-slug
            return View("Details", product);
        }
        
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Customer model)
        {
            if (ModelState.IsValid)
            {
                // WARNING: This checks for plain text passwords.
                // This is INSECURE and only for learning.
                var user = db.Customers.FirstOrDefault(u => u.UserName == model.UserName && u.Password == model.Password);

                if (user != null)
                {
                    // Set the authentication cookie
                    FormsAuthentication.SetAuthCookie(user.UserName, false); // "false" = don't remember me

                    // Send them back to the home page
                    return RedirectToAction("LongChauClone", "TrangChu");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: TrangChu/Register (This just shows the register page)
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: TrangChu/Register (This handles the form submission)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Customer model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = db.Customers.FirstOrDefault(u => u.UserName == model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
                    return View(model);
                }

                // Set the creation date
                model.CreatedAt = DateTime.Now;

                // Save the new customer
                db.Customers.Add(model);
                db.SaveChanges();

                // Log the new user in automatically
                FormsAuthentication.SetAuthCookie(model.UserName, false);

                // Send them to the home page
                return RedirectToAction("LongChauClone", "TrangChu");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: TrangChu/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Sign the user out
            FormsAuthentication.SignOut();
            // Send them back to the home page
            return RedirectToAction("LongChauClone", "TrangChu");
        }
        //end
        //detail
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    // Eager-load Product, its Images, and its Reviews all in one query
        //    Product product = db.Products
        //        .Include(p => p.ProductImages)
        //        .Include(p => p.Reviews) // Make sure to include Reviews
        //        .SingleOrDefault(p => p.Id == id);

        //    if (product == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // Pass the single product (which now contains images and reviews) to the View
        //    return View(product);
        //}

        //end
        public ActionResult GioHang()
        {
            return View();
        }
        public ActionResult VitaminC()
        {
            return View();
        }
        public ActionResult Prospan()
        {
            return View();
        }
        public ActionResult ThuocXuongKhop()
        {
            return View();
        }
        public ActionResult Omega3()
        {
            return View();
        }
        public ActionResult Vitamin_khoangchat()
        {
            return View();
        }
        public ActionResult SucKhoeTimMach()
        {
            return View();
        }
        public ActionResult HoTroLamDep()
        {
            return View();
        }
        public ActionResult ThanKinhNao()
        {
            return View();
        }
        public ActionResult CoenzymeQ10()
        {
            return View();
        }
        public ActionResult MultiVitamin()
        {
            return View();
        }
        public ActionResult Magnesium()
        {
            return View();
        }
        public ActionResult Collagen()
        {
            return View();
        }
        public ActionResult Biotin()
        {
            return View();
        }
        public ActionResult Ginkgo()
        {
            return View();
        }
        public ActionResult Paracetamol()
        {
            return View();
        }
        public ActionResult ThuocNhoMat()
        {
            return View();
        }
        public ActionResult Phosphalugel()
        {
            return View();
        }
        public ActionResult Amoxicillin()
        {
            return View();
        }
        public ActionResult ThuocKeDon()
        {
            return View();
        }
        public ActionResult ThuocKhongKeDon()
        {
            return View();
        }
        public ActionResult NMNPremium21600()
        {
            return View();
        }  
        public ActionResult SiroLabebe()
        {
            return View();
        }
        public ActionResult MultiVitasLabWell()
        {
            return View();
        }
        public ActionResult BrauerDHA()
        {
            return View();
        }
        public ActionResult JexMax()
        {
            return View();
        }
        public ActionResult Osla()
        {
            return View();
        }
        public ActionResult CanxiDHC()
        {
            return View();
        }
        public ActionResult Thuoc()
        {
            return View();
        }
        public ActionResult GocSucKhoe()
        {
            return View();
        }
        public ActionResult TiemChung()
        {
             return View();
        }
        public ActionResult HeThongNhaThuoc()
        {
            return View();
        }
        public ActionResult BenhLy()
        {
            return View();
        }
        public ActionResult KhauTrang()
        {
            return View();
        }
        public ActionResult SoCuu()
        {
            return View();
        }
        public ActionResult TheoDoi()
        {
            return View();
        }
    }
}