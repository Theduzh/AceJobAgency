using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PracAssignment.ViewModels;
using PracAssignment.Model;
using System.Net;
using System.Text.Json;
using PracAssignment.Helper;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace PracAssignment.Pages
{
    public class LoginModel : PageModel
    {
		[BindProperty]
		public Login LModel { get; set; }
        public bool CodeSent { get; set; }

		private readonly SignInManager<AceJobAgencyUser> signInManager;
        private readonly UserManager<AceJobAgencyUser> userManager;
        private readonly AceJobAgencyDbContext aceJobAgencyDbContext;
        private readonly IEmailSender emailSender;
        private readonly ILogger<Login> _logger;
		private readonly IConfiguration _configuration;
		private readonly string SessionKey = "_SessionId";

        public LoginModel(SignInManager<AceJobAgencyUser> signInManage, UserManager<AceJobAgencyUser> userManager, 
            AceJobAgencyDbContext aceJobAgencyDbContext, IEmailSender emailSender, ILogger<Login> _logger, IConfiguration configuration)
		{
			this.signInManager = signInManage;
            this.userManager = userManager;
            this.aceJobAgencyDbContext = aceJobAgencyDbContext;
            this.emailSender = emailSender;
            this._logger = _logger;
            this._configuration = configuration;
        }
		public void OnGet()
        {

        }

		public bool ValidateGoogleCaptcha()
		{
			string Response = Request.Form["g-recaptcha-response"];
            string SecretKey = _configuration["GoogleCaptchaSiteSecretKey"];

            bool Valid = false;

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=" + SecretKey + "&response=" + Response);

			try
			{
				using (WebResponse wResponse = req.GetResponse())
				{
					using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
					{
						string jsonResponse = readStream.ReadToEnd();

						ReCaptcha data = JsonSerializer.Deserialize<ReCaptcha>(jsonResponse);

						Valid = data.success;
					}
				}
				return Valid;
			}
			catch (WebException ex)
			{
				throw ex;
			}

		}


        public async Task<IActionResult> OnPostAsync()
		{
            if (!ValidateGoogleCaptcha())
            {
                ModelState.AddModelError("", "Captcha Failed");
                return Page();
            }

			if (ModelState.IsValid)
			{
				var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password,
				LModel.RememberMe, lockoutOnFailure: true);

                if (identityResult.Succeeded)
                {
                    // Add to Audit Logs
                    var user = await userManager.FindByEmailAsync(LModel.Email);

                    if (user != null)
                    {
                        if (user.TwoFactorEnabled)
                        {

                            var auditLogEntry = new AuditLog
                            {
                                UserId = user.Id,
                                Timestamp = DateTime.UtcNow,
                                Action = "Login",
                                Details = "Password Sign In Complete"
                            };

                            aceJobAgencyDbContext.AuditLogs.Add(auditLogEntry);
                            await aceJobAgencyDbContext.SaveChangesAsync();

                            // Add Session Id;
                            var SessionId = Guid.NewGuid().ToString();
                            user.SessionVersion = SessionId;
                            HttpContext.Session.SetString(SessionKey, SessionId);
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
                            // Log successful login in the AuditLog table
                            var auditLogEntry = new AuditLog
                            {
                                UserId = user.Id,
                                Timestamp = DateTime.UtcNow,
                                Action = "Login",
                                Details = "2FA Started"
                            };

                            aceJobAgencyDbContext.AuditLogs.Add(auditLogEntry);
                            await aceJobAgencyDbContext.SaveChangesAsync();
                            await signInManager.SignOutAsync();

                            var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
                            var message = $"Your authentication code is: {token}";

                            emailSender.SendEmailAsync(user.Email, "2FA Token", message);

                            return RedirectToPage("/TwoFactor", new { userId = user.Id });
                        }
					}
				}

                if (identityResult.IsLockedOut)
                {
                    var user = await userManager.FindByEmailAsync(LModel.Email);

                    if (user != null)
                    {
                        // Log Account Lock Out in the AuditLog table
                        var auditLogEntry = new AuditLog
                        {
                            UserId = user.Id,
                            Timestamp = DateTime.UtcNow,
                            Action = "Login",
                            Details = "Account Locked Out"
                        };

                        aceJobAgencyDbContext.AuditLogs.Add(auditLogEntry);
                        await aceJobAgencyDbContext.SaveChangesAsync();
                    }

                    // Handle account lockout
                    ModelState.AddModelError("", "Account locked out. Please try again later.");
                    return Page();
                }

				ModelState.AddModelError("", "Username or Password incorrect");
			}
			return Page();
		}
	}
}
