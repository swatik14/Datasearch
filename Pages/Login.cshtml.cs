using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataSearch.Models;

namespace DataSearch.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    public LoginModel(SignInManager<ApplicationUser> signIn) { _signIn = signIn; }

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
        if (res.Succeeded) return Redirect("/Search");
        ErrorMessage = "Invalid username or password.";
        return Page();
    }
}