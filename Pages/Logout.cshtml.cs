using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataSearch.Models;

namespace DataSearch.Pages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    public LogoutModel(SignInManager<ApplicationUser> signIn) { _signIn = signIn; }
    public async Task<IActionResult> OnGetAsync()
    {
        await _signIn.SignOutAsync();
        return Redirect("/Login");
    }
}