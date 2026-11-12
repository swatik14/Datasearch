using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataSearch.Models;

namespace DataSearch.Pages;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _um;
    public RegisterModel(UserManager<ApplicationUser> um) { _um = um; }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Plan { get; set; } = "Monthly";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = new ApplicationUser
        {
            UserName = Input.UserName,
            Email = Input.Email,
            FullName = Input.FullName,
            Plan = Input.Plan,
            Role = Input.Plan == "Agency" ? "Agency" : "Customer",
            SubscriptionExpiry = Input.Plan == "Yearly" ? DateTime.UtcNow.AddYears(1) :
                                Input.Plan == "Agency" ? DateTime.UtcNow.AddYears(1) :
                                DateTime.UtcNow.AddMonths(1)
        };
        var res = await _um.CreateAsync(user, Input.Password);
        if (res.Succeeded) return Redirect("/Login");
        ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Description));
        return Page();
    }
}