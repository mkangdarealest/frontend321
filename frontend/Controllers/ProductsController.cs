using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Text.RegularExpressions;
using frontend.Models;

namespace frontend.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private LongChauDbEntities db = new LongChauDbEntities();

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.ProductImages).ToList();
            return View(products);
        }
        //fetch slug and put in address bar
        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLowerInvariant();
            // Remove invalid chars
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // Convert multiple spaces into one space
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // Cutto max 45 chars
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            // Replace spaces with hyphens
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
            Product product = db.Products.Include(p => p.ProductImages)
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
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Notice the new parameter: 'IEnumerable<ProductImage> productImages'
        // 2. We removed 'ProductImages' from the [Bind] list.
        public ActionResult Create([Bind(Include = "Id,Name,Brand,ShortDescription,Description,Price,OriginalPrice,Rating,ReviewsCount,Ingredients,UsageInfo,Origin,Packaging")] Product product, IEnumerable<ProductImage> productImages)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set auto-properties
                    product.CreatedAt = DateTime.Now;
                    product.Slug = GenerateSlug(product.Name);

                    // Add the main product to the database FIRST
                    db.Products.Add(product);
                    db.SaveChanges(); // <-- This save generates the new 'product.Id'

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
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Notice the new parameter: 'IEnumerable<ProductImage> productImages'
        public ActionResult Edit(Product product, IEnumerable<ProductImage> productImages) // This is the product from the form
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Load the ORIGINAL product from the database, INCLUDING its old images
                    var productToUpdate = db.Products
                        .Include(p => p.ProductImages)
                        .SingleOrDefault(p => p.Id == product.Id);

                    if (productToUpdate == null)
                    {
                        return HttpNotFound();
                    }

                    // 2. Manually update the text properties
                    productToUpdate.Name = product.Name;
                    productToUpdate.Brand = product.Brand;
                    // ... (etc. - this is your safe update logic from before)
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
                    productToUpdate.Slug = GenerateSlug(product.Name);

                    // 3. --- NEW IMAGE LOGIC ---
                    // This is the new list of images from the form
                    var newImageUrls = (productImages ?? new List<ProductImage>())
                                        .Select(i => i.Url)
                                        .ToList();

                    // This is the old list of images from the database
                    var oldImages = productToUpdate.ProductImages.ToList();

                    // 3a. REMOVE old images that are NO LONGER checked
                    foreach (var oldImage in oldImages)
                    {
                        if (!newImageUrls.Contains(oldImage.Url))
                        {
                            db.ProductImages.Remove(oldImage);
                        }
                    }

                    // 3b. ADD/UPDATE images that ARE checked
                    if (productImages != null)
                    {
                        foreach (var newImage in productImages)
                        {
                            var existingImage = productToUpdate.ProductImages
                                                .SingleOrDefault(i => i.Url == newImage.Url);

                            if (existingImage != null)
                            {
                                // This image already exists, just update its 'IsPrimary' status
                                existingImage.IsPrimary = newImage.IsPrimary;
                            }
                            else
                            {
                                // This is a brand new image, link and add it
                                newImage.ProductId = productToUpdate.Id;
                                db.ProductImages.Add(newImage);
                            }
                        }
                    }

                    db.Entry(productToUpdate).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges(); // This saves everything

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
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity.ToString(),
                                validationError.ErrorMessage);
                            raise = new InvalidOperationException(message, raise);
                        }
                    }
                    throw raise;
                }
            }
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
            Product product = db.Products.Find(id);
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
