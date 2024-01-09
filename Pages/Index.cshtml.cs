using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PracAssignment.Model;
using System.Net;

namespace PracAssignment.Pages
{
	public class IndexModel : PageModel
    {
		private readonly string SessionKey = "_SessionId";

		private readonly IConfiguration _configuration;
        private readonly UserManager<AceJobAgencyUser> userManager;
		private readonly SignInManager<AceJobAgencyUser> signInManager;
        private readonly IEmailSender emailSender;
        private readonly ILogger<IndexModel> _logger;
        public bool EmailSent = false;

		public AceJobAgencyUser user { get; set; }

        public IndexModel(IConfiguration configuration, UserManager<AceJobAgencyUser> userManager, SignInManager<AceJobAgencyUser> signInManager, 
            IEmailSender emailSender, ILogger<IndexModel> logger)
        {
            this._configuration = configuration;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            user = await userManager.GetUserAsync(User);

            var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
            var protector = dataProtectionProvider.CreateProtector(_configuration["SecretKey"]);

            // Decrypt NRIC
            if (user != null)
            {
                user.Nric = protector.Unprotect(user.Nric);
                user.WhoAmI = WebUtility.HtmlDecode(user.WhoAmI);
                user.Resume = WebUtility.HtmlDecode(user.Resume);

                if (HttpContext.Session.GetString(SessionKey) != user.SessionVersion)
				{
					// Log the user out or perform other actions
					await signInManager.SignOutAsync();
					RedirectToAction("Login");
				}
			}
        }
        public async Task<IActionResult> OnPostDownloadResumeAsync()
        {
            var user = await userManager.GetUserAsync(User);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", user.Resume);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", user.Resume);
        }
    }
}