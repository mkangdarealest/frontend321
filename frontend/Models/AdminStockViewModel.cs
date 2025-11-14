using System;
using System.Collections.Generic;

namespace frontend.Models
{
    // A new ViewModel to hold our two separate lists for the view
    public class AdminStockViewModel
    {
        public List<Product> InStockProducts { get; set; }
        public List<Product> OutOfStockProducts { get; set; }
    }
}