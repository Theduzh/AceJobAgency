using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using PracAssignment.Model;
using PracAssignment.ViewModels;

namespace PracAssignment.Pages
{
	[Authorize]
	public class ChangePasswordModel : PageModel
    {

        [BindProperty]
        public ChangePassword CPModel { get; set; }
        private UserManager<AceJobAgencyUser> userManager { get; }
        private SignInManager<AceJobAgencyUser> signInManager { get; }
        private readonly IConfiguration _configuration;
        public AceJobAgencyUser user { get; set; }
        private readonly string SessionKey = "_SessionId";

        public ChangePasswordModel(UserManager<AceJobAgencyUser> userManager,
        SignInManager<AceJobAgencyUser> signInManager, IConfiguration configuration)

        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                user = await userManager.GetUserAsync(User);

                if (user != null)
                {
                    if (user?.LastPasswordChangedDate.AddDays(1) > DateTime.Now)
                    {
                        // Password change to recently, please wait another day
                        ModelState.AddModelError("", "Password change is too recent");
                        return Page();
                    }

                    if (HttpContext.Session.GetString(SessionKey) != user.SessionVersion)
                    {
                        // Log the user out or perform other actions
                        await signInManager.SignOutAsync();
                        RedirectToAction("Login");
                    }

                    // Check if the old password is correct
                    var result = await userManager.CheckPasswordAsync(user, CPModel.OldPassword);

                    if (!result)
                    {
                        // Old password is incorrect
                        ModelState.AddModelError(nameof(CPModel.OldPassword), "Incorrect old password");
                        return Page();
                    }

                    // Old password is correct, validate new password
                    var passwordValidator = userManager.PasswordValidators.FirstOrDefault();
                    if (passwordValidator != null)
                    {
                        var passwordValidationResult = await passwordValidator.ValidateAsync(userManager, user, CPModel.NewPassword);
                        if (!passwordValidationResult.Succeeded)
                        {
                            // New password validation failed
                            foreach (var error in passwordValidationResult.Errors)
                            {
                                ModelState.AddModelError(nameof(CPModel.NewPassword), error.Description);
                            }
                            return Page();
                        }
                    }

                    // Add Old Password to the Password Policy List
                    var passwordHasher = new PasswordHasher<AceJobAgencyUser>();

                    if (IsPasswordReused(user, passwordHasher, CPModel.NewPassword))
                    {
                        ModelState.AddModelError(nameof(CPModel.NewPassword), "Password cannot be one of the last two passwords.");
                        return Page();
                    }


                    // Update user's password
                    var tempOldPassowrd = user.PasswordHash;
                    var changePasswordResult = await userManager.ChangePasswordAsync(user, CPModel.OldPassword, CPModel.NewPassword);

                    if (changePasswordResult.Succeeded)
                    {
                        user.OldPass2 = user.OldPass1;
                        user.OldPass1 = tempOldPassowrd;

                        await userManager.UpdateAsync(user);

                        return RedirectToPage("Index");
                    }
                    else
                    {
                        // Password change failed
                        foreach (var error in changePasswordResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                else
                {
                    // User not found
                    return NotFound();
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
