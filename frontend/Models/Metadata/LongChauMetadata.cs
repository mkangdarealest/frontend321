using System;
using System.ComponentModel.DataAnnotations;

namespace frontend.Models
{
    // ================== PRODUCT VALIDATION ==================
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {

    }

    public class ProductMetadata
    {
        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "{0} không được để trống.")]
        [StringLength(200, ErrorMessage = "{0} không được vượt quá {1} ký tự.")]
        public string Name { get; set; }

        [Display(Name = "Thương hiệu")]
        [Required(ErrorMessage = "{0} không được để trống.")]
        public string Brand { get; set; }

        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "{0} không được để trống.")]
        [Range(0, double.MaxValue, ErrorMessage = "{0} phải lớn hơn hoặc bằng 0.")]
        [DisplayFormat(DataFormatString = "{0:N0}đ", ApplyFormatInEditMode = false)]
        public decimal? Price { get; set; }

        [Display(Name = "Giá gốc")]
        [Range(0, double.MaxValue, ErrorMessage = "{0} phải lớn hơn hoặc bằng 0.")]
        public decimal? OriginalPrice { get; set; }

        [Display(Name = "Số lượng tồn")]
        [Required(ErrorMessage = "{0} không được để trống.")]
        [Range(0, 10000, ErrorMessage = "{0} phải từ 0 đến 10,000.")]
        public int Quantity { get; set; }

        [Display(Name = "Mô tả ngắn")]
        [StringLength(1000, ErrorMessage = "{0} quá dài (tối đa {1} ký tự).")]
        public string ShortDescription { get; set; }

        [Display(Name = "Xuất xứ")]
        public string Origin { get; set; }
    }

    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục.")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }
    }
}