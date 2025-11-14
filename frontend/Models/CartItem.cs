namespace frontend.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        // This is a calculated property
        public decimal LineTotal
        {
            get { return UnitPrice * Quantity; }
        }
    }
}