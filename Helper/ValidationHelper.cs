using Microsoft.AspNetCore.Identity;
using PracAssignment.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PracAssignment.Helper
{
    public class ValidationHelper
    {
        public class NoSpecialCharactersAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var input = (string)value;

                if (!string.IsNullOrEmpty(input))
                {
                    // Use regex to check if the input contains only letters and spaces
                    if (!Regex.IsMatch(input, "^[a-zA-Z ]+$"))
                    {
                        var displayName = validationContext.DisplayName;
                        return new ValidationResult($"Special characters are not allowed in the {displayName} field.");
                    }
                }

                return ValidationResult.Success;
            }
        }

        public class NricValidationAttribute : ValidationAttribute
        {
            private readonly string _pattern = "^[STFGstfg]\\d{7}[A-Za-z]$";

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var nric = (string)value;

                if (string.IsNullOrEmpty(nric) || !Regex.IsMatch(nric, _pattern))
                {
                    return new ValidationResult("Invalid NRIC format. Please enter a valid NRIC.");
                }

                return ValidationResult.Success;
            }
        }

        public class BirthDateBeforeTodayAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var birthDate = (DateTime)value;

                if (birthDate >= DateTime.Today)
                {
                    return new ValidationResult("BirthDate must be before today.");
                }

                return ValidationResult.Success;
            }
        }
    }
}
