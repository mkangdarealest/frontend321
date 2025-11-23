/* FILE: /frontend/Controllers/CheckoutController.cs (NEW FILE) */

using frontend.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace frontend.Controllers
{
    // This entire controller requires the user to be logged in.
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private LongChauDbEntities db = new LongChauDbEntities();
        private const string CartSession = "CartSession";

        // GET: /Checkout/Index
        // This action shows the final checkout page
        public ActionResult Index()
        {
            // 1. Get the cart from session
            var cart = GetCart();
            if (cart.Count == 0)
            {
                // Can't check out with an empty cart
                TempData["Error"] = "Giỏ hàng của bạn đang rỗng.";
                return RedirectToAction("Index", "GioHang");
            }

            // 2. Get the logged-in user's details
            var currentUsername = User.Identity.Name;
            var customer = db.Customers.FirstOrDefault(c => c.UserName == currentUsername);
            if (customer == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "Customer record not found");
            }

            // 3. Get cart totals and discount from Session
            decimal subTotal = cart.Sum(item => item.LineTotal);
            decimal discountAmount = (decimal)(Session["DiscountAmount"] ?? 0m);
            decimal finalTotal = subTotal - discountAmount;

            // 4. Create the ViewModel
            var model = new CheckoutViewModel
            {
                CustomerDetails = customer,
                CartItems = cart,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                TotalAmount = finalTotal
            };

            return View(model);
        }

        // POST: /Checkout/PlaceOrder
        // This action saves the cart to the database
        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder()
        {
            var cart = GetCart();
            var currentUsername = User.Identity.Name;
            var customer = db.Customers.FirstOrDefault(c => c.UserName == currentUsername);

            if (cart.Count == 0 || customer == null)
            {
                return RedirectToAction("Index", "TrangChu");
            }

            // --- 1. PRE-CHECK STOCK (VALIDATION) ---
            // Before creating an order, ensure ALL items have enough stock.
            foreach (var item in cart)
            {
                var productCheck = db.Products.Find(item.ProductId);
                if (productCheck == null || productCheck.Quantity < item.Quantity)
                {
                    TempData["Error"] = $"Sản phẩm '{item.ProductName}' chỉ còn {productCheck?.Quantity ?? 0} sản phẩm. Vui lòng cập nhật giỏ hàng.";
                    return RedirectToAction("Index", "GioHang");
                }
            }

            decimal subTotal = cart.Sum(item => item.LineTotal);
            decimal discountAmount = (decimal)(Session["DiscountAmount"] ?? 0m);
            decimal finalTotal = subTotal - discountAmount;

            using (var transaction = db.Database.BeginTransaction()) // Use transaction for safety
            {
                try
                {
                    // 2. Create the Order
                    var order = new Order
                    {
                        CustomerId = customer.Id,
                        OrderDate = DateTime.Now,
                        StatusId = 1, // Pending
                        SubTotal = subTotal,
                        Discount = discountAmount,
                        ShippingFee = 0,
                        Total = finalTotal,
                        PaymentMethod = "COD"
                    };
                    db.Orders.Add(order);
                    db.SaveChanges();

                    // 3. Process Order Items & Update Stock/Sold
                    foreach (var cartItem in cart)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = cartItem.ProductId,
                            ProductName = cartItem.ProductName,
                            UnitPrice = cartItem.UnitPrice,
                            Quantity = cartItem.Quantity
                        };
                        db.OrderItems.Add(orderItem);

                        // UPDATE PRODUCT METRICS
                        var product = db.Products.Find(cartItem.ProductId);
                        if (product != null)
                        {
                            product.Quantity -= cartItem.Quantity;      // Decrease Stock
                            product.SoldQuantity += cartItem.Quantity;  // Increase Sold Count (FIX)
                            db.Entry(product).State = EntityState.Modified;
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit(); // Commit only if everything succeeds

                    // 4. Cleanup
                    ClearCart();
                    Session.Remove("DiscountAmount");

                    TempData["SuccessMessage"] = "Đặt hàng thành công! Cảm ơn bạn.";
                    return RedirectToAction("OrderHistory", "Customer");
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Undo everything if error occurs
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }

        // --- HELPER METHODS ---
        private List<CartItem> GetCart()
        {
            var cart = Session[CartSession] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session[CartSession] = cart;
            }
            return cart;
        }

        private void ClearCart()
        {
            Session[CartSession] = new List<CartItem>();
        }
    }
}