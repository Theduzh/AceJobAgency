using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using PracAssignment.Helper;

namespace PracAssignment.ViewModels
{

    public class Register
	{
		[Required]
		[DataType(DataType.Text)]
		[ValidationHelper.NoSpecialCharacters]
		public string FirstName { get; set; }

		[Required]
		[DataType(DataType.Text)]
		[ValidationHelper.NoSpecialCharacters]
		public string LastName { get; set; }

		[Required]
		[DataType(DataType.Text)]
		[ValidationHelper.NoSpecialCharacters]
		public string Gender { get; set; }

		[Required]
		[DataType(DataType.Text)]
		[ValidationHelper.NricValidation]
		public string Nric { get; set; }

		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
		public string ConfirmPassword { get; set; }

		[Required]
		[DataType(DataType.Date)]
		[ValidationHelper.BirthDateBeforeToday]
		public DateTime BirthDate { get; set; }

		[DataType(DataType.Upload)]
		public IFormFile Resume { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string WhoAmI { get; set; }

	}
}
