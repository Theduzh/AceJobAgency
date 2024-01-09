using System.ComponentModel.DataAnnotations;

namespace PracAssignment.ViewModels
{
    public class EnterEmail
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
