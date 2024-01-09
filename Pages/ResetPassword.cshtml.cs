using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PracAssignment.Model;
using PracAssignment.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PracAssignment.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<AceJobAgencyUser> userManager;
        private readonly SignInManager<AceJobAgencyUser> signInManager;
        private readonly ILogger<ResetPassword> _logger;


        [BindProperty]
        public ResetPassword RPModel { get; set; }

        public bool EmailSent { get; set; }

        public ResetPasswordModel(UserManager<AceJobAgencyUser> userManager, SignInManager<AceJobAgencyUser> signInManager, ILogger<ResetPassword> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _logger = logger;
            RPModel = new ResetPassword();
        }

        public void OnGet(string userEmail, string token)
        {
            RPModel.UserEmail = userEmail;
            RPModel.Token = token;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(RPModel.UserEmail);

                if (user != null)
                {
                    // Add Old Password to the Password Policy List
                    var passwordHasher = new PasswordHasher<AceJobAgencyUser>();

                    if (IsPasswordReused(user, passwordHasher, RPModel.NewPassword))
                    {
                        ModelState.AddModelError(nameof(RPModel.NewPassword), "Password cannot be one of the last two passwords.");
                        return Page();
                    }

                    // Reset the user's password
                    var tempOldPassowrd = user.PasswordHash;
                    var result = await userManager.ResetPasswordAsync(user, RPModel.Token, RPModel.NewPassword);

                    if (result.Succeeded)
                    {
                        user.LastPasswordChangedDate = DateTime.Now;
                        user.OldPass2 = user.OldPass1;
                        user.OldPass1 = tempOldPassowrd;

                        await userManager.UpdateAsync(user);
                        return RedirectToPage("Index");
                    }

                    if (result.Succeeded)
                    {
                        // Redirect to login or show a confirmation message
                        return RedirectToPage("Index");
                    }

                    // Password reset failed, handle errors
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    // User not found, for security reasons, don't disclose this information
                    ModelState.AddModelError(string.Empty, "User Not found");
                }
            }

            return Page();
        }
        private bool IsPasswordReused(AceJobAgencyUser user, PasswordHasher<AceJobAgencyUser> passwordHasher, string newPassword)
        {
            // Check if the new password matches any of the old passwords or the current password
            return passwordHasher.VerifyHashedPassword(user, user.PasswordHash, newPassword) == PasswordVerificationResult.Success ||
                   passwordHasher.VerifyHashedPassword(user, user.OldPass1, newPassword) == PasswordVerificationResult.Success ||
                   passwordHasher.VerifyHashedPassword(user, user.OldPass2, newPassword) == PasswordVerificationResult.Success;
        }
    }
}

