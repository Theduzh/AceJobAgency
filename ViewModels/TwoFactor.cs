using System.ComponentModel.DataAnnotations;

namespace PracAssignment.ViewModels
{
    public class TwoFactor
    {
        [Required]
        [DataType(DataType.Text)]
        public string TwoFactorCode { get; set; }
    }
}
