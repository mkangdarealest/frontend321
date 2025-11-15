using frontend.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace frontend.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private LongChauDbEntities db = new LongChauDbEntities();

        // GET: Products
        public ActionResult Index(string searchString,int? page)
        {
            var products = db.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Categories) 
                .AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString)
                                            || p.Brand.Contains(searchString));
            }

            // 4. Save the search query to show in the textbox
            ViewBag.CurrentFilter = searchString;
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            var orderedProducts = products.OrderBy(p => p.Name);

            return View(orderedProducts.ToPagedList(pageNumber, pageSize));
            //return View(products.OrderBy(p => p.Name).ToList());
        }
        //fetch slug and put in address bar
        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLowerInvariant();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Add .Include() to get the images
            Product product = db.Products
                    .Include(p => p.ProductImages) // Get images
                    .Include(p => p.Categories)    // <-- ADD THIS to get categories
                    .SingleOrDefault(p => p.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.AllCategories = db.Categories.ToList();
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Notice the new parameter: 'IEnumerable<ProductImage> productImages'
        // 2. We removed 'ProductImages' from the [Bind] list.
        public ActionResult Create([Bind(Include = "Id,Name,Brand,ShortDescription,Description,Price,OriginalPrice,Rating,ReviewsCount,Ingredients,UsageInfo,Origin,Packaging,Quantity")] Product product, IEnumerable<ProductImage> productImages, int[] selectedCategoryIds, IEnumerable<HttpPostedFileBase> ImageFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set auto-properties
                    product.CreatedAt = DateTime.Now;
                    product.Slug = GenerateSlug(product.Name);
                    product.ProductImages = null;

                    // Add the main product to the database FIRST
                    db.Products.Add(product);
                    db.SaveChanges(); // <-- This save generates the new 'product.Id'

                    if (selectedCategoryIds != null)
                    {
                        foreach (var catId in selectedCategoryIds)
                        {
                            var category = db.Categories.Find(catId);
                            if (category != null)
                            {
                                product.Categories.Add(category);
                            }
                        }
                        db.SaveChanges(); // Save the new category relationships
                    }
                    // Now, link and save the images that were sent from the form
                    if (productImages != null)
                    {
                        foreach (var image in productImages)
                        {
                            // Assign the new ProductId to each image
                            image.ProductId = product.Id;
                            db.ProductImages.Add(image);
                        }

                        // Save the images to the database
                        db.SaveChanges();
                    }
                    if (ImageFiles != null)
                    {
                        foreach (var file in ImageFiles)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                // Generate a unique file name to avoid overwrites
                                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                                // Define the save path (you have a /hinh/ folder)
                                // Let's create a subfolder for product uploads
                                string savePath = Path.Combine(Server.MapPath("~/hinh/products/"), fileName);

                                // Create directory if it doesn't exist
                                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                                // Save the file
                                file.SaveAs(savePath);

                                // Create a new ProductImage entry for the database
                                var newDbImage = new ProductImage
                                {
                                    ProductId = product.Id,
                                    Url = "~/hinh/products/" + fileName, // Save the relative path
                                    IsPrimary = false // You can add logic for this
                                };
                                db.ProductImages.Add(newDbImage);
                            }
                        }
                    }

                    return RedirectToAction("Index");
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                // (This is your debug code, it's good to keep)
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }

            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Categories)
                                 .SingleOrDefault(p => p.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.AllCategories = db.Categories.ToList();
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Notice the new parameter: 'IEnumerable<ProductImage> productImages'
        public ActionResult Edit(Product product, IEnumerable<ProductImage> productImages, int[] selectedCategoryIds, IEnumerable<HttpPostedFileBase> ImageFiles) // This is the product from the form
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Load the ORIGINAL product from DB, INCLUDING its old images AND categories
                    var productToUpdate = db.Products
                        .Include(p => p.ProductImages)
                        .Include(p => p.Categories) // <-- Load existing categories
                        .SingleOrDefault(p => p.Id == product.Id);

                    if (productToUpdate == null)
                    {
                        return HttpNotFound();
                    }

                    // 2. Manually update text properties
                    productToUpdate.Name = product.Name;
                    productToUpdate.Brand = product.Brand;
                    productToUpdate.ShortDescription = product.ShortDescription;
                    productToUpdate.Description = product.Description;
                    productToUpdate.Price = product.Price;
                    productToUpdate.OriginalPrice = product.OriginalPrice;
                    productToUpdate.Rating = product.Rating;
                    productToUpdate.ReviewsCount = product.ReviewsCount;
                    productToUpdate.Ingredients = product.Ingredients;
                    productToUpdate.UsageInfo = product.UsageInfo;
                    productToUpdate.Origin = product.Origin;
                    productToUpdate.Packaging = product.Packaging;
                    productToUpdate.Quantity = product.Quantity;
                    productToUpdate.Slug = GenerateSlug(product.Name);

                    // 3. --- NEW CATEGORY UPDATE LOGIC ---
                    // The simplest way: clear all old categories and add the new selected ones.
                    productToUpdate.Categories.Clear();
                    if (selectedCategoryIds != null)
                    {
                        foreach (var catId in selectedCategoryIds)
                        {
                            var category = db.Categories.Find(catId);
                            if (category != null)
                            {
                                productToUpdate.Categories.Add(category);
                            }
                        }
                    }
                    // --- END CATEGORY LOGIC ---

                    // 4. --- EXISTING IMAGE LOGIC ---
                    var newImageUrls = (productImages ?? new List<ProductImage>()).Select(i => i.Url).ToList();
                    var oldImages = productToUpdate.ProductImages.ToList();
                    foreach (var oldImage in oldImages)
                    {
                        if (!newImageUrls.Contains(oldImage.Url))
                        {
                            db.ProductImages.Remove(oldImage);
                        }
                    }
                    if (productImages != null)
                    {
                        foreach (var newImage in productImages)
                        {
                            var existingImage = productToUpdate.ProductImages.SingleOrDefault(i => i.Url == newImage.Url);
                            if (existingImage != null)
                            {
                                existingImage.IsPrimary = newImage.IsPrimary;
                            }
                            else
                            {
                                newImage.ProductId = productToUpdate.Id;
                                db.ProductImages.Add(newImage);
                            }
                        }
                    }
                    if (ImageFiles != null)
                    {
                        foreach (var file in ImageFiles)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                // Generate a unique file name to avoid overwrites
                                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                                // Define the save path (you have a /hinh/ folder)
                                // Let's create a subfolder for product uploads
                                string savePath = Path.Combine(Server.MapPath("~/hinh/products/"), fileName);

                                // Create directory if it doesn't exist
                                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                                // Save the file
                                file.SaveAs(savePath);

                                // Create a new ProductImage entry for the database
                                var newDbImage = new ProductImage
                                {
                                    ProductId = product.Id,
                                    Url = "~/hinh/products/" + fileName, // Save the relative path
                                    IsPrimary = false // You can add logic for this
                                };
                                db.ProductImages.Add(newDbImage);
                            }
                        }
                    }
                    // --- END IMAGE LOGIC ---

                    db.SaveChanges(); // This saves EVERYTHING (product details, category links, image links)
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    // (Your debug code)
                    Exception raise = dbEx;
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}", validationErrors.Entry.Entity.ToString(), validationError.ErrorMessage);
                            raise = new InvalidOperationException(message, raise);
                        }
                    }
                    throw raise;
                }
            }

            // If we fail, re-populate the ViewBag for the view to reload
            ViewBag.AllCategories = db.Categories.ToList();
            return View(product);
        }
        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Add .Include() to get the images
            Product product = db.Products.Include(p => p.ProductImages)
                                         .SingleOrDefault(p => p.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //Load the product AND all its relationships
            Product product = db.Products
                .Include(p => p.OrderItems)    
                .Include(p => p.ProductImages) 
                .Include(p => p.Categories)   
                .SingleOrDefault(p => p.Id == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            //CONSTRAINT CHECK
            if (product.OrderItems.Any())
            {
                TempData["Error"] = $"Không thể xóa sản phẩm '{product.Name}' vì đã có trong đơn hàng của khách. Bạn có thể 'Sửa' sản phẩm để ẩn nó đi.";
                return RedirectToAction("Index");
            }
            //Clean up many-to-many category relationships
            product.Categories.Clear();

            //Clean up one-to-many image relationships
            foreach (var image in product.ProductImages.ToList())
            {
                db.ProductImages.Remove(image);
            }

            //delete the product itself
            db.Products.Remove(product);
            db.SaveChanges();

            return RedirectToAction("Index");
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
