using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PracAssignment.Helper;
using PracAssignment.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AceJobAgencyDbContext>();

// Enable Email Sender
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Password Requirements
builder.Services.AddIdentity<AceJobAgencyUser, IdentityRole>(options =>
{
	// Configure password policy
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

})
.AddEntityFrameworkStores<AceJobAgencyDbContext>()
.AddDefaultTokenProviders();

// Redirect to Login/Logout
builder.Services.ConfigureApplicationCookie(Config =>
{
	Config.LoginPath = "/Login";
	Config.LogoutPath = "/Logout";
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{	
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.Run();
