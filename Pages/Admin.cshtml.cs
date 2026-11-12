using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataSearch.Models;

namespace DataSearch.Pages;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class AdminModel : PageModel
{
    private readonly UserManager<ApplicationUser> _um;
    public AdminModel(UserManager<ApplicationUser> um) { _um = um; }

    public List<ApplicationUser> Users { get; set; } = new();
    [BindProperty] public NewUserInput NewUser { get; set; } = new();
    public string? Message { get; set; }

    public class NewUserInput
    {
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Type { get; set; } = "Customer";
        public string Plan { get; set; } = "Monthly";
    }

    public async Task OnGetAsync()
    {
        Users = _um.Users.OrderBy(u => u.UserName).ToList();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var role = NewUser.Type == "Agency" ? "Agency" : "Customer";
        var user = new ApplicationUser
        {
            UserName = NewUser.UserName,
            FullName = NewUser.FullName,
            Role = role,
            Plan = NewUser.Plan,
            IsActive = true,
            SubscriptionExpiry = NewUser.Plan == "Yearly" ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMonths(1)
        };
        var res = await _um.CreateAsync(user, NewUser.Password);
        if (res.Succeeded)
        {
            await _um.AddToRoleAsync(user, role);
            Message = "User created.";
        }
        else Message = string.Join("; ", res.Errors.Select(e => e.Description));
        Users = _um.Users.OrderBy(u => u.UserName).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostToggleAsync(string userId)
    {
        var user = await _um.FindByIdAsync(userId);
        if (user != null && user.Role != "Admin")
        {
            user.IsActive = !user.IsActive;
            await _um.UpdateAsync(user);
            Message = user.IsActive ? "Reactivated." : "Deactivated.";
        }
        Users = _um.Users.OrderBy(u => u.UserName).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostExtendAsync(string userId)
    {
        var user = await _um.FindByIdAsync(userId);
        if (user != null)
        {
            var baseDate = (user.SubscriptionExpiry.HasValue && user.SubscriptionExpiry > DateTime.UtcNow) ? user.SubscriptionExpiry.Value : DateTime.UtcNow;
            user.SubscriptionExpiry = baseDate.AddMonths(1);
            user.IsActive = true;
            await _um.UpdateAsync(user);
            Message = "Extended by 1 month.";
        }
        Users = _um.Users.OrderBy(u => u.UserName).ToList();
        return Page();
    }
}