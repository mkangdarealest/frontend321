using System;

namespace frontend.Models
{
    public class ProductSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal? Price { get; set; }
        public string ImageUrl { get; set; }
        public string ShortDescription { get; set; }
    }
}