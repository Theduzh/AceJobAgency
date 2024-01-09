using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PracAssignment.Model
{
	public class AceJobAgencyDbContext: IdentityDbContext<AceJobAgencyUser>
	{
		private readonly IConfiguration _configuration;
        public DbSet<AuditLog> AuditLogs { get; set; }
        public AceJobAgencyDbContext(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string connectionString = _configuration.GetConnectionString("AceJobAgencyConnectionString"); 
			optionsBuilder.UseSqlServer(connectionString);
		}
	}
}
