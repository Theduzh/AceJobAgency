using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PracAssignment.Helper;
using PracAssignment.Model;
using PracAssignment.ViewModels;
using System.Web;

namespace PracAssignment.Pages
{
    public class EnterEmailModel : PageModel
    {
        [BindProperty]
        public EnterEmail EModel { get; set; }
		private readonly IEmailSender emailSender;
		private readonly ILogger<EnterEmailModel> _logger;
		private readonly UserManager<AceJobAgencyUser> userManager;

        public EnterEmailModel(IEmailSender emailSender, ILogger<EnterEmailModel> logger, UserManager<AceJobAgencyUser> userManager)
        {
            this._logger = logger;
            this.emailSender = emailSender;
			this.userManager = userManager;
        }
		public void OnGet()
        {
        }

		public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
				// Verify if the email exists in your user database
				var user = await userManager.FindByEmailAsync(EModel.Email);

				if (user != null)
				{
					// Generate a password reset token
					var token = await userManager.GeneratePasswordResetTokenAsync(user);

					// Generate a password reset link with the token
					var userEmail = user.Email;
					/*	var resetLink = Url.PageLink("ResetPassword", values: new { userEmail, token });*/
					var resetLink = $"{Request.Scheme}://{Request.Host}/ResetPassword?userEmail={userEmail}&token={HttpUtility.UrlEncode(token)}";

					// Send the reset link via email
					await emailSender.SendEmailAsync(user.Email, "Password Reset", $"Click here to reset your password: {resetLink}");
				}

				ModelState.AddModelError("", "A link to change password is sent to that email");
			}

            return Page();
        }

	}
}
