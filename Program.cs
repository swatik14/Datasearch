using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataSearch.Data;
using DataSearch.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/Login";
});

builder.Services.AddRazorPages(options =>
{
    // Protect search page
    options.Conventions.AuthorizePage("/Search");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Ensure DB created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // Seed admin
    var um = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var r in new[] { "Admin", "Customer", "Agency" })
        if (!rm.RoleExistsAsync(r).Result) rm.CreateAsync(new IdentityRole(r)).Wait();
    if (um.FindByNameAsync("admin").Result == null)
    {
        var admin = new ApplicationUser { UserName = "admin", Email = "admin@local", FullName = "Administrator", Role = "Admin", Plan = "Unlimited", SubscriptionExpiry = DateTime.UtcNow.AddYears(100) };
        um.CreateAsync(admin, "admin123").Wait();
        um.AddToRoleAsync(admin, "Admin").Wait();
    }
}

app.Run();