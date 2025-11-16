/* FILE: /frontend/Controllers/GioHangController.cs (NEW FILE) */

using frontend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace frontend.Controllers
{
    public class GioHangController : Controller
    {
        private LongChauDbEntities db = new LongChauDbEntities();
        private const string CartSession = "CartSession";

        // GET: /GioHang/Index
        // This is the main shopping cart page
        public ActionResult Index()
        {
            var cart = GetCart();

            // This passes the cart (a List<CartItem>) to the View
            return View(cart);
        }

        // POST: /GioHang/AddToCart
        // This action is called from the "MUA HÀNG" button
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity)
        {

            var product = db.Products
                .Include("ProductImages")
                .SingleOrDefault(p => p.Id == productId);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Get the current cart from the Session
            var cart = GetCart();

            // Check if the item is already in the cart
            CartItem existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                // If it is, just increase the quantity
                // Check against stock (số lượng tồn)
                int newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > product.Quantity)
                {
                    // If over stock, set to max stock and add an error
                    existingItem.Quantity = product.Quantity;
                    TempData["Error"] = $"Rất tiếc, bạn chỉ có thể mua tối đa {product.Quantity} sản phẩm {product.Name}.";
                }
                else
                {
                    existingItem.Quantity = newQuantity;
                }
            }
            else
            {
                // If not, create a new CartItem
                // Check against stock (số lượng tồn)
                if (quantity > product.Quantity)
                {
                    quantity = product.Quantity;
                    TempData["Error"] = $"Rất tiếc, bạn chỉ có thể mua tối đa {product.Quantity} sản phẩm {product.Name}.";
                }

                var newItem = new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImage = (product.ProductImages.FirstOrDefault(img => img.IsPrimary) ?? product.ProductImages.FirstOrDefault())?.Url, // Get primary, or else first
                    UnitPrice = product.Price ?? 0, // Handle nullable decimal
                    Quantity = quantity
                };
                cart.Add(newItem);
            }

            // Save the cart back into the Session
            SaveCart(cart);

            // Redirect back to the cart page to show what was added
            return RedirectToAction("Index");
        }

        // POST: /GioHang/UpdateCart
        // This is called from the quantity box on the cart page

        [HttpPost]
        public ActionResult UpdateCart(int productId, int newQuantity)
        {
            try
            {
                var cart = GetCart();
                var item = cart.FirstOrDefault(i => i.ProductId == productId);
                string errorMessage = null;
                int finalItemQuantity = 0;
                decimal itemLineTotalCalc = 0;

                if (item != null)
                {
                    if (newQuantity <= 0)
                    {
                        // Item is being removed
                        cart.Remove(item);
                        finalItemQuantity = 0;
                    }
                    else
                    {
                        // Item is being updated, check stock
                        var product = db.Products.Find(productId);

                        if (product == null) // <-- THIS IS THE CRITICAL CHECK
                        {
                            // Product was deleted from DB, remove from cart
                            cart.Remove(item);
                            finalItemQuantity = 0;
                            errorMessage = $"Sản phẩm {item.ProductName} không còn tồn tại.";
                        }
                        else if (newQuantity > product.Quantity)
                        {
                            // Out of stock check
                            item.Quantity = product.Quantity;
                            errorMessage = $"Rất tiếc, bạn chỉ có thể mua tối đa {product.Quantity} sản phẩm {product.Name}.";
                            finalItemQuantity = item.Quantity;
                        }
                        else
                        {
                            // All good, update quantity
                            item.Quantity = newQuantity;
                            finalItemQuantity = item.Quantity;
                        }
                    }

                    // Recalculate line total safely
                    itemLineTotalCalc = (item?.UnitPrice ?? 0) * finalItemQuantity;
                    SaveCart(cart);
                }

                // Prepare the JSON response
                decimal newTotalAmount = cart.Sum(c => c.LineTotal);
                int newCartItemCount = cart.Sum(c => c.Quantity);

                return Json(new
                {
                    success = true,
                    message = errorMessage,
                    itemQuantity = finalItemQuantity,
                    itemLineTotal = itemLineTotalCalc.ToString("N0") + " đ",
                    totalAmount = newTotalAmount.ToString("N0") + " đ",
                    cartItemCount = newCartItemCount
                });
            }
            catch (Exception ex)
            {
                // If anything else goes wrong, return a JSON error
                return Json(new { success = false, message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            // Prepare the JSON response
            decimal newTotalAmount = cart.Sum(c => c.LineTotal);
            int newCartItemCount = cart.Sum(c => c.Quantity);

            return Json(new
            {
                success = true,
                totalAmount = newTotalAmount.ToString("N0") + " đ",
                cartItemCount = newCartItemCount
            });
        }
        //1 for percentage, 2 for fixed amount
        [HttpPost]
        public ActionResult ApplyDiscount(string couponCode)
        {
            var cart = GetCart();
            decimal subTotal = cart.Sum(c => c.LineTotal);
            decimal discountAmount = 0;
            string successMessage = null;
            string errorMessage = null;

            // [NEW] Query the database for the coupon
            var coupon = db.DiscountCoupons.FirstOrDefault(c => c.Code == couponCode);

            if (coupon == null)
            {
                errorMessage = "Mã giảm giá không hợp lệ.";
            }
            else if (!coupon.IsActive)
            {
                errorMessage = "Mã giảm giá này đã bị vô hiệu hóa.";
            }
            else if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.Now)
            {
                errorMessage = "Mã giảm giá này đã hết hạn.";
            }
            else
            {
                // Coupon is valid, calculate the discount
                if (coupon.DiscountType == 1) // Type 1 = Percentage
                {
                    discountAmount = subTotal * (coupon.DiscountValue / 100m);
                    successMessage = $"Áp dụng mã {coupon.Code} (-{coupon.DiscountValue}%) thành công!";
                }
                else if (coupon.DiscountType == 2) // Type 2 = Fixed Amount
                {
                    discountAmount = coupon.DiscountValue;
                    if (discountAmount > subTotal) // Don't let discount be more than total
                    {
                        discountAmount = subTotal;
                    }
                    successMessage = $"Áp dụng mã {coupon.Code} (-{coupon.DiscountValue.ToString("N0")}đ) thành công!";
                }
            }
            // --- End database logic ---

            if (discountAmount > 0)
            {
                Session["DiscountAmount"] = discountAmount;
            }
            else
            {
                Session.Remove("DiscountAmount");
            }

            decimal finalTotal = subTotal - discountAmount;

            return Json(new
            {
                success = discountAmount > 0,
                message = discountAmount > 0 ? successMessage : errorMessage,
                discountAmountFormatted = (discountAmount).ToString("N0") + " đ",
                finalTotalFormatted = finalTotal.ToString("N0") + " đ"
            });
        }

        // This action provides the "Giỏ hàng (2)" text in your header
        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = GetCart();
            int itemCount = cart.Sum(item => item.Quantity);
            return PartialView("_CartSummary", itemCount);
        }


        // --- HELPER METHODS ---

        // Gets the cart from the session
        private List<CartItem> GetCart()
        {
            var cart = Session[CartSession] as List<CartItem>;
            if (cart == null)
            {
                // If there's no cart, create a new empty one
                cart = new List<CartItem>();
                SaveCart(cart);
            }
            return cart;
        }

        // Saves the cart back to the session
        private void SaveCart(List<CartItem> cart)
        {
            Session[CartSession] = cart;
        }
    }
}