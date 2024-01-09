using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace PracAssignment.Model
{
	public class AceJobAgencyUser: IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Gender { get; set; }
		public string Nric { get; set; }
		public DateTime BirthDate { get; set; }
		public string Resume { get; set; }
		public string WhoAmI { get; set; }
		public string SessionVersion { get; set; } = string.Empty;
		public string OldPass1 { get; set; } = string.Empty;
        public string OldPass2 { get; set; } = string.Empty;
        public DateTime LastPasswordChangedDate { get; set; } = DateTime.Now;
    }
}
