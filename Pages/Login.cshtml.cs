using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataSearch.Models;

namespace DataSearch.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _um;
    public LoginModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> um) { _signIn = signIn; _um = um; }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var res = await _signIn.PasswordSignInAsync(Input.UserName, Input.Password, true, false);
        if (res.Succeeded)
        {
            var user = await _um.FindByNameAsync(Input.UserName);
            if (user != null && user.Role == "Admin") return Redirect("/Admin");
            return Redirect("/Search");
        }
        ErrorMessage = "Invalid username or password.";
        return Page();
    }
}