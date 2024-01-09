using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using PracAssignment.Model;
using PracAssignment.ViewModels;
using System.Net;

namespace PracAssignment.Pages
{
	public class RegisterModel : PageModel
	{
		private UserManager<AceJobAgencyUser> userManager { get; }
		private SignInManager<AceJobAgencyUser> signInManager { get; }
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly ILogger<RegisterModel> _logger;
        private readonly string SessionKey = "_SessionId";


        [BindProperty]
		public Register RModel { get; set; }

		public RegisterModel(UserManager<AceJobAgencyUser> userManager,
		SignInManager<AceJobAgencyUser> signInManager, 
		IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
		ILogger<RegisterModel> logger)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
            this._configuration = configuration;
            this._webHostEnvironment = webHostEnvironment;
			this._logger = logger;
		}

        public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			_logger.LogInformation(RModel.Resume != null ? RModel.Resume.FileName : "None");

			if (ModelState.IsValid)
			{

				if (RModel.Resume == null || RModel.Resume.Length == 0)
				{
					ModelState.AddModelError("RModel.Resume", "The Resume field is required.");
					return Page();
				}

				var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
				var protector = dataProtectionProvider.CreateProtector(_configuration["SecretKey"]);

				var user = new AceJobAgencyUser()
				{
					UserName = RModel.Email,
					Email = RModel.Email,
					FirstName = RModel.FirstName,
					LastName = RModel.LastName,
					Gender = RModel.Gender,
					Nric = protector.Protect(RModel.Nric),
					BirthDate = RModel.BirthDate,
					Resume = WebUtility.HtmlEncode(ProcessUploadedFile(RModel.Resume)),
					WhoAmI = WebUtility.HtmlEncode(RModel.WhoAmI),
					OldPass1 = string.Empty,
					OldPass2 = string.Empty,
					SessionVersion = string.Empty
				};
				var result = await userManager.CreateAsync(user, RModel.Password);
				if (result.Succeeded)
				{
					await signInManager.SignInAsync(user, false);

                    // Add Session Id
                    var SessionId = Guid.NewGuid().ToString();
                    user.SessionVersion = SessionId;
                    HttpContext.Session.SetString(SessionKey, SessionId);
                    await userManager.UpdateAsync(user);
                    return RedirectToPage("Index");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return Page();
		}

		// Handle file upload
		private string ProcessUploadedFile(IFormFile file)
		{
			if (file != null && file.Length > 0)
			{
				var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					file.CopyTo(stream);
				}

				return fileName;
			}
			return null;
		}
	}
}
