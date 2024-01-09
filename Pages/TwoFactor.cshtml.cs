using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PracAssignment.Model;
using PracAssignment.ViewModels;

namespace PracAssignment.Pages
{
	public class TwoFactorModel : PageModel
	{
		[BindProperty]
		public TwoFactor TFModel { get; set; }
		[BindProperty(SupportsGet = true)]
		public string UserId { get; set; }

		private readonly SignInManager<AceJobAgencyUser> signInManager;
		private readonly UserManager<AceJobAgencyUser> userManager;
		private readonly ILogger<TwoFactorModel> logger;
		private readonly string SessionKey = "_SessionId";

		public TwoFactorModel(SignInManager<AceJobAgencyUser> signInManager, UserManager<AceJobAgencyUser> userManager, ILogger<TwoFactorModel> logger)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.logger = logger;
		}

		public void OnGet(string userId)
		{
			UserId = userId;
		}

		public async Task<IActionResult> OnPostTwoFactorCodeAsync()
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByIdAsync(UserId);

				if (user != null)
				{
					var isTokenValid = await userManager.VerifyTwoFactorTokenAsync(user, "Email", TFModel.TwoFactorCode);
					if (isTokenValid)
					{
                        // Add Session Id;
                        await signInManager.SignInAsync(user, false);
                        var SessionId = Guid.NewGuid().ToString();
                        user.SessionVersion = SessionId;
                        HttpContext.Session.SetString(SessionKey, SessionId);

						user.TwoFactorEnabled = true;
                        await userManager.UpdateAsync(user);

                        if (user?.LastPasswordChangedDate.AddDays(90) < DateTime.Now)
                        {
                            // Password has expired
                            // Redirect user to change password page
                            return Redirect("ChangePassword");
                        } 
						else
						{
                            return RedirectToPage("Index");
                        }
					}
					else
					{
						ModelState.AddModelError("", "2FA Code is incorrect");
					}
				}
			}

			return Page();
		}
	}

}
