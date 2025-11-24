using frontend.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;

namespace frontend.Controllers
{
    public class TrangChuController : Controller
    {
        [ChildActionOnly] // This means it can only be called from another view
        public ActionResult CategoryNavigation()
        {
            var allCategories = db.Categories.ToList();
            var parentCategories = allCategories
                                     .Where(c => c.ParentCategoryId == null)
                                     .OrderBy(c => c.Name) // You can change this to an "Order" column if you add one later
                                     .ToList();

            ViewBag.AllCategories = allCategories;

            // 4. Pass *only the parents* as the main Model to the view.
            return PartialView("_CategoryNavigation", parentCategories);
        }
        public ActionResult CategoryNavigationMobile()
        {
            var allCategories = db.Categories.ToList();
            var parentCategories = allCategories
                                     .Where(c => c.ParentCategoryId == null)
                                     .OrderBy(c => c.Name)
                                     .ToList();

            ViewBag.AllCategories = allCategories;
            return PartialView("_CategoryNavigationMobile", parentCategories);
        }
        //Database to View
        LongChauDbEntities db = new LongChauDbEntities();
        public ActionResult LongChauClone()
        {
            // 1. Get Top 8 Newest Products (Hàng Mới)
            var newProducts = db.Products
                .Where(p => p.Quantity > 0) // In stock only
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Include(p => p.ProductImages)
                .ToList();

            // 2. Get Top 8 Best Selling Products (Bán Chạy) - Using the new SoldQuantity field
            var bestSellers = db.Products
                .Where(p => p.Quantity > 0)
                .OrderByDescending(p => p.SoldQuantity) // Sort by sales
                .Take(8)
                .Include(p => p.ProductImages)
                .ToList();
            var allProductsPreview = db.Products
                .Where(p => p.Quantity > 0)
                .OrderBy(p => p.Name) // You can change this to Guid.NewGuid() for random if supported
                .Take(12)
                .Include(p => p.ProductImages)
                .ToList();

            ViewBag.BestSellers = bestSellers;
            ViewBag.NewProducts = newProducts;
            ViewBag.AllProductsPreview = allProductsPreview; // <--- New ViewBag

            return View(); // We don't pass a single model anymore, we use ViewBag
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
                .Include(p => p.Reviews.Select(r => r.Customer))
                .SingleOrDefault(p => p.Slug == slug);

            if (product == null)
            {
                return HttpNotFound();
            }

            var productCategory = product.Categories.FirstOrDefault();
            if (productCategory != null)
            {
                var similarProducts = db.Products
                    .Where(p => p.Categories.Any(c => c.Id == productCategory.Id) // In the same category
                                && p.Id != product.Id // Not the product itself
                                && p.Quantity > 0)    // In stock
                    .Include(p => p.ProductImages) // Get images for display
                    .OrderByDescending(p => p.CreatedAt) // Or OrderBy(p => Guid.NewGuid()) for random
                    .Take(5) // Get top 5
                    .ToList();

                ViewBag.SimilarProducts = similarProducts;
            }
            else
            {
                ViewBag.SimilarProducts = new List<Product>(); // Pass an empty list
            }
            // === END NEW LOGIC ===


            // Go directly to the "Details" view and pass it the product
            // The URL in the browser STAYS as .../ViewProduct/your-product-slug
            return View("Details", product);
        }
        
        public ActionResult ProductsByCategory(string slug, int? page, string searchString, string sortBy, decimal? minPrice, decimal? maxPrice, string origin)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var category = db.Categories.FirstOrDefault(c => c.Slug == slug);
            if (category == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryName = category.Name;
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSlug = slug;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentOrigin = origin;

            var distinctOrigins = category.Products
                                          .Where(p => p.Origin != null && p.Origin != "")
                                          .Select(p => p.Origin)
                                          .Distinct()
                                          .OrderBy(o => o)
                                          .ToList();
            ViewBag.AllOrigins = distinctOrigins;

            var productsQuery = category.Products.AsQueryable();
            productsQuery = productsQuery.Where(p => p.Quantity > 0);

            // --- 1. Search Logic ---
            if (!String.IsNullOrEmpty(searchString))
            {
                // Check if Name/Brand is NOT null before checking Contains
                productsQuery = productsQuery.Where(p =>
                    (p.Name != null && p.Name.Contains(searchString)) ||
                    (p.Brand != null && p.Brand.Contains(searchString))
                );
            }

            // --- 2. New Price Filter Logic ---
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }
            if (!string.IsNullOrEmpty(origin))
            {
                productsQuery = productsQuery.Where(p => p.Origin == origin);
            }
            // --- 3. Sorting Logic ---
            IOrderedQueryable<Product> orderedProducts;
            switch (sortBy)
            {
                case "price_asc":
                    orderedProducts = productsQuery.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    orderedProducts = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "name_desc": // Optional: Add Name Z-A
                    orderedProducts = productsQuery.OrderByDescending(p => p.Name);
                    break;
                default:
                    // Default sort by Name A-Z
                    orderedProducts = productsQuery.OrderBy(p => p.Name);
                    break;
            }

            int pageSize = 12;
            int pageNumber = (page ?? 1);

            return View(orderedProducts.ToPagedList(pageNumber, pageSize));
        }
        // GET: /TrangChu/AllProducts
        // GET: /TrangChu/AllProducts
        public ActionResult AllProducts(int? page, string searchString, string sortBy, decimal? minPrice, decimal? maxPrice, string origin)
        {
            // 1. Start with ALL products (including Images for the thumbnail)
            var productsQuery = db.Products.Include(p => p.ProductImages).AsQueryable();

            // 2. Filter: Only show In-Stock items
            productsQuery = productsQuery.Where(p => p.Quantity > 0);
            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.PageTitle = $"Kết quả tìm kiếm: '{searchString}'";
            }
            else
            {
                ViewBag.PageTitle = "Tất cả sản phẩm";
            }
            // 3. Filter: Search by Name or Brand
            if (!String.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => (p.Name != null && p.Name.Contains(searchString))
                                                      || (p.Brand != null && p.Brand.Contains(searchString)));
            }

            // 4. Filter: Price Range
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            // 5. Filter: Origin (Xuất xứ)
            if (!string.IsNullOrEmpty(origin))
            {
                productsQuery = productsQuery.Where(p => p.Origin == origin);
            }

            // 6. Sorting Logic
            switch (sortBy)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "name_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Name);
                    break;
                case "best_selling":
                    productsQuery = productsQuery.OrderByDescending(p => p.SoldQuantity);
                    break;
                case "newest":
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    // Default: A-Z
                    productsQuery = productsQuery.OrderBy(p => p.Name);
                    break;
            }

            // 7. Prepare ViewBag for the View (to keep filter inputs filled)
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentOrigin = origin;

            // Get list of ALL distinct origins from the database for the dropdown
            ViewBag.AllOrigins = db.Products
                                   .Where(p => p.Origin != null && p.Origin != "")
                                   .Select(p => p.Origin)
                                   .Distinct()
                                   .OrderBy(o => o)
                                   .ToList();

            // 8. Pagination
            int pageSize = 12;
            int pageNumber = (page ?? 1);

            return View(productsQuery.ToPagedList(pageNumber, pageSize));
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
            // We only need username and password to log in
            if (ModelState.IsValidField("Username") && ModelState.IsValidField("Password"))
            {
                var user = db.Customers.FirstOrDefault(u => u.UserName == model.UserName && u.Password == model.Password);

                if (user != null)
                {
                    Session["UserAvatar"] = user.AvatarUrl;
                    var ticket = new FormsAuthenticationTicket(
                        1,                                  // version
                        user.UserName,                      // user name
                        DateTime.Now,                       // issue time
                        DateTime.Now.AddMinutes(30),        // expiration
                        false,                              // isPersistent
                        "Customer"                          // <-- HARD-CODED ROLE
                    );

                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    var authCookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
                    // --- END NEW PART ---

                    return RedirectToAction("LongChauClone", "TrangChu");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

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
                //FormsAuthentication.SetAuthCookie(model.UserName, false);

                // Send them to the home page
                //return RedirectToAction("LongChauClone", "TrangChu");
                return RedirectToAction("Login", "TrangChu");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")] // Only logged-in customers can review
        public ActionResult AddReview(string slug, int rating, string comment)
        {
            // 1. Get the product and the currently logged-in customer
            var product = db.Products.FirstOrDefault(p => p.Slug == slug);
            var currentUsername = User.Identity.Name;
            var customer = db.Customers.FirstOrDefault(c => c.UserName == currentUsername);

            if (product == null || customer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 2. Create the new Review object
            var newReview = new Review
            {
                ProductId = product.Id,
                CustomerId = customer.Id,
                Rating = rating,
                Body = comment,
                CreatedAt = DateTime.Now
            };

            // 3. Save the new review
            db.Reviews.Add(newReview);
            db.SaveChanges(); // Save this first

            // 4. Recalculate the product's average rating and count
            var allReviewsForProduct = db.Reviews.Where(r => r.ProductId == product.Id);

            if (allReviewsForProduct.Any())
            {
                // This calculates the new average. The (decimal?) cast is crucial.
                product.Rating = (decimal?)allReviewsForProduct.Average(r => r.Rating);
                product.ReviewsCount = allReviewsForProduct.Count();
            }
            else
            {
                product.Rating = null;
                product.ReviewsCount = 0;
            }

            // 5. Save the updated product
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();

            // 6. Redirect back to the product page
            return RedirectToAction("ViewProduct", new { slug = slug });
        }

        // POST: TrangChu/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Sign the user out
            FormsAuthentication.SignOut();
            Session.Remove("UserAvatar");
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