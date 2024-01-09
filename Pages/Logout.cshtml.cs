using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PracAssignment.Model;

namespace PracAssignment.Pages
{
    [Authorize]
    public class LogoutModel : PageModel
    {
		private readonly SignInManager<AceJobAgencyUser> signInManager;
		private readonly UserManager<AceJobAgencyUser> userManager;
        private readonly AceJobAgencyDbContext aceJobAgencyDbContext;

        public AceJobAgencyUser user { get; set; }
		public LogoutModel(SignInManager<AceJobAgencyUser> signInManager, UserManager<AceJobAgencyUser> userManager, AceJobAgencyDbContext aceJobAgencyDbContext)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.aceJobAgencyDbContext = aceJobAgencyDbContext;
		}
		public void OnGet()
        {
        }
		public async Task<IActionResult> OnPostLogoutAsync()
		{
			user = await userManager.GetUserAsync(User);

			// Audit Log for Logout
            var auditLogEntry = new AuditLog
            {
                UserId = user.Id,
                Timestamp = DateTime.UtcNow,
                Action = "Logout",
                Details = "Sign Out Success"
            };

            aceJobAgencyDbContext.AuditLogs.Add(auditLogEntry);
            await aceJobAgencyDbContext.SaveChangesAsync();

			// Clear Session
			HttpContext.Session.Clear();

            // Set An Invalid Session Id to user
            user.SessionVersion = Guid.NewGuid().ToString();
			await userManager.UpdateAsync(user);

			await signInManager.SignOutAsync();
			return RedirectToPage("Login");
		}
		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}
	}
}
