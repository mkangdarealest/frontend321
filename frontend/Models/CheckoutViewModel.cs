/* FILE: /frontend/Models/CheckoutViewModel.cs (NEW FILE) */

using System.Collections.Generic;

namespace frontend.Models
{
    // This is NOT a database table.
    // It's a helper class to pass data to the Checkout view.
    public class CheckoutViewModel
    {
        public Customer CustomerDetails { get; set; }
        public List<CartItem> CartItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}