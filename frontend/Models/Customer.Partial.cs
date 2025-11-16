namespace frontend.Models
{
	using System.ComponentModel.DataAnnotations;

	// This links our validation rules to the auto-generated Customer class
	[MetadataType(typeof(CustomerMetadata))]
	public partial class Customer
	{
		// This adds the "retype password" field to the model
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		[DataType(DataType.Password)]
		[Display(Name = "Retype Password")]
		public string ConfirmPassword{ get; set; }
	}

	// This class holds all the validation rules from your text
	public class CustomerMetadata
	{
		public string UserName { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập Tên")]
		[StringLength(50)]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập Họ")]
		[StringLength(50)]
		public string LastName { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập Email")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		[StringLength(100)]
		public string Email { get; set; }

		// PASSWORD IS NOW OPTIONAL on the edit page.
		// We only validate its length if the user types something.
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		[DataType(DataType.Password)]
		[Display(Name = "New Password")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
		public string Phone { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
		public string AddressLine { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập thành phố")]
		public string City { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập quận/huyện")]
		public string District { get; set; }
	}
}