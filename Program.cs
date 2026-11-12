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

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
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
    if (db.Records.Count() < 100)
    {
        db.Records.AddRange(
            new Record { State="Karnataka", PhoneNumber="9876543210", OwnerName="Ravi Kumar", FatherName="Shivappa", Address="Bangalore", AadhaarId="123456789012", AlternativeNumber="9999999999", Circle="Airtel" },
            new Record { State="Karnataka", PhoneNumber="9123456789", OwnerName="Suresh Gowda", FatherName="Ramaiah", Address="Mysore", AadhaarId="987654321098", AlternativeNumber="8888888888", Circle="Jio" },
            new Record { State="Karnataka", PhoneNumber="9988001122", OwnerName="Priya Sharma", FatherName="Mahesh Sharma", Address="Hubli", AadhaarId="112233445566", AlternativeNumber="9900112233", Circle="Airtel" },
            new Record { State="Karnataka", PhoneNumber="9012345678", OwnerName="Anil Desai", FatherName="Vasant Desai", Address="Belgaum", AadhaarId="998877665544", AlternativeNumber="8899001122", Circle="VI" },
            new Record { State="Karnataka", PhoneNumber="9845012345", OwnerName="Kavya Nair", FatherName="Rajesh Nair", Address="Mangalore", AadhaarId="445566778899", AlternativeNumber="7788990011", Circle="Jio" },
            new Record { State="Andhra Pradesh", PhoneNumber="9988776655", OwnerName="Lakshmi Devi", FatherName="Venkatesh", Address="Vijayawada", AadhaarId="456789123456", AlternativeNumber="7777777777", Circle="Airtel" },
            new Record { State="Andhra Pradesh", PhoneNumber="9000011111", OwnerName="Ramesh Babu", FatherName="Krishna", Address="Guntur", AadhaarId="321654987321", AlternativeNumber="6666666666", Circle="VI" },
            new Record { State="Andhra Pradesh", PhoneNumber="9876501234", OwnerName="Nagarjuna Rao", FatherName="Srinivas Rao", Address="Visakhapatnam", AadhaarId="778899001122", AlternativeNumber="5566778899", Circle="Jio" },
            new Record { State="Andhra Pradesh", PhoneNumber="9440123456", OwnerName="Bhavani Reddy", FatherName="Narayana Reddy", Address="Tirupati", AadhaarId="334455667788", AlternativeNumber="4455667788", Circle="Airtel" },
            new Record { State="Andhra Pradesh", PhoneNumber="9392012345", OwnerName="Chaitanya Kumar", FatherName="Prasad", Address="Nellore", AadhaarId="223344556677", AlternativeNumber="3344556677", Circle="VI" }
        );
        var rnd = new Random(42);
        var firstNames = new[]{"Arun","Divya","Kiran","Meena","Suresh","Lakshmi","Ravi","Priya","Anil","Kavya","Ramesh","Bhavani","Chaitanya","Nagarjuna","Sunita","Vijay","Deepa","Manoj","Pooja","Rahul"};
        var lastNames = new[]{"Kumar","Reddy","Gowda","Sharma","Devi","Babu","Rao","Nair","Desai","Patel"};
        var fathers = new[]{"Shivappa","Ramaiah","Venkatesh","Krishna","Mahesh","Vasant","Rajesh","Srinivas","Narayana","Prasad"};
        var addresses = new[]{"Bangalore","Mysore","Hubli","Belgaum","Mangalore","Vijayawada","Guntur","Visakhapatnam","Tirupati","Nellore","Davangere","Shimoga","Bellary","Kurnool","Kadapa"};
        var circles = new[]{"Airtel","Jio","VI","BSNL"};
        var states = new[]{"Karnataka","Andhra Pradesh"};
        for (int i = 0; i < 90; i++)
        {
            db.Records.Add(new Record {
                State = states[rnd.Next(states.Length)],
                PhoneNumber = "9" + rnd.Next(100000000, 999999999).ToString(),
                OwnerName = firstNames[rnd.Next(firstNames.Length)] + " " + lastNames[rnd.Next(lastNames.Length)],
                FatherName = fathers[rnd.Next(fathers.Length)],
                Address = addresses[rnd.Next(addresses.Length)],
                AadhaarId = rnd.Next(100000000, 999999999).ToString() + rnd.Next(100, 999).ToString(),
                AlternativeNumber = "9" + rnd.Next(100000000, 999999999).ToString(),
                Circle = circles[rnd.Next(circles.Length)]
            });
        }
        db.SaveChanges();
    }
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