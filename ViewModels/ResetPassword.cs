using System.ComponentModel.DataAnnotations;

namespace PracAssignment.ViewModels
{
	public class ResetPassword
	{
        public string UserEmail { get; set; }

        public string Token { get; set; }

        [Required]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(NewPassword), ErrorMessage = "Password and confirmation password does not match")]
		public string ConfirmPassword { get; set; }
	}
}
