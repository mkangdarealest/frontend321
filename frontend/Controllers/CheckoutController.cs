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

            decimal subTotal = cart.Sum(item => item.LineTotal);
            decimal discountAmount = (decimal)(Session["DiscountAmount"] ?? 0m);
            decimal finalTotal = subTotal - discountAmount;

            try
            {
                // 1. Create the Order
                var order = new Order
                {
                    CustomerId = customer.Id,
                    OrderDate = DateTime.Now,
                    StatusId = 1, // 1 = "Pending" from your LC_Store.sql
                    SubTotal = subTotal,
                    Discount = discountAmount,
                    ShippingFee = 0, // You can add this later
                    Total = finalTotal,
                    PaymentMethod = "COD" // Hard-coded for now
                };
                db.Orders.Add(order);
                db.SaveChanges(); // Save to get the new 'order.Id'

                // 2. Create OrderItems and update stock
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

                    // 3. Update product stock (Quantity)
                    var product = db.Products.Find(cartItem.ProductId);
                    if (product != null)
                    {
                        product.Quantity -= cartItem.Quantity;
                        db.Entry(product).State = EntityState.Modified;
                    }
                }

                // 4. Save all changes
                db.SaveChanges();

                // 5. Clear the cart and discount
                ClearCart();
                Session.Remove("DiscountAmount");

                // 6. Redirect to a "Thank You" page (we'll use Order History)
                TempData["SuccessMessage"] = "Đặt hàng thành công! Cảm ơn bạn.";
                return RedirectToAction("OrderHistory", "Customer");
            }
            catch (Exception ex)
            {
                // Handle errors
                TempData["Error"] = "Có lỗi xảy ra khi đặt hàng: " + ex.Message;
                return RedirectToAction("Index");
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