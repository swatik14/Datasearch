using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DataSearch.Data;
using DataSearch.Models;

namespace DataSearch.Pages;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class AdminModel : PageModel
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly AppDbContext _db;
    public AdminModel(UserManager<ApplicationUser> um, AppDbContext db) { _um = um; _db = db; }

    public List<ApplicationUser> Users { get; set; } = new();
    public Dictionary<string, decimal> TotalPaid { get; set; } = new();
    [BindProperty] public NewUserInput NewUser { get; set; } = new();
    [BindProperty] public PaymentInput Payment { get; set; } = new();
    public string? Message { get; set; }

    public class PaymentInput
    {
        public string UserId { get; set; } = "";
        public decimal Amount { get; set; }
        public string Note { get; set; } = "";
    }

    public class NewUserInput
    {
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Type { get; set; } = "Customer";
        public string Plan { get; set; } = "Monthly";
    }

    private async Task LoadUsersAsync()
    {
        Users = _um.Users.OrderBy(u => u.UserName).ToList();
        var ids = Users.Select(u => u.Id).ToList();
        var payments = await _db.Payments.Where(p => ids.Contains(p.UserId)).ToListAsync();
        TotalPaid = payments.GroupBy(p => p.UserId).ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
    }

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
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
        await LoadUsersAsync();
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
        await LoadUsersAsync();
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
        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostPaymentAsync()
    {
        if (string.IsNullOrWhiteSpace(Payment.UserId) || Payment.Amount <= 0)
        {
            Message = "Enter valid amount.";
            await LoadUsersAsync();
            return Page();
        }
        _db.Payments.Add(new Payment
        {
            UserId = Payment.UserId,
            Amount = Payment.Amount,
            Note = Payment.Note,
            PaidOn = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        Message = "Payment recorded.";
        await LoadUsersAsync();
        return Page();
    }
}