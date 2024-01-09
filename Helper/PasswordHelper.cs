using Microsoft.AspNetCore.Identity;
using PracAssignment.Model;

namespace PracAssignment.Helper
{
    public class PasswordHelper
    {
        public bool IsPasswordReused(AceJobAgencyUser user, PasswordHasher<AceJobAgencyUser> passwordHasher, string newPassword)
        {
            // Check if the new password matches any of the old passwords or the current password
            return passwordHasher.VerifyHashedPassword(user, user.PasswordHash, newPassword) == PasswordVerificationResult.Success ||
                   passwordHasher.VerifyHashedPassword(user, user.OldPass1, newPassword) == PasswordVerificationResult.Success ||
                   passwordHasher.VerifyHashedPassword(user, user.OldPass2, newPassword) == PasswordVerificationResult.Success;
        }

        public async Task<IdentityResult> ValidatePasswordAsync(UserManager<AceJobAgencyUser> userManager, AceJobAgencyUser user, string newPassword)
        {
            var passwordValidator = userManager.PasswordValidators.FirstOrDefault();
            if (passwordValidator != null)
            {
                return await passwordValidator.ValidateAsync(userManager, user, newPassword);
            }

            return IdentityResult.Success;
        }
    }
}
