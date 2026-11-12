using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DataSearch.Data;
using DataSearch.Models;

namespace DataSearch.Pages;

[Microsoft.AspNetCore.Authorization.Authorize]
public class SearchModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _um;
    public SearchModel(AppDbContext db, UserManager<ApplicationUser> um) { _db = db; _um = um; }

    [BindProperty(SupportsGet = true)] public string State { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string PhoneNumber { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string OwnerName { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string FatherName { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string Address { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string AadhaarId { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string AlternativeNumber { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string Circle { get; set; } = "";
    [BindProperty(SupportsGet = true, Name = "page")] public int CurrentPage { get; set; } = 1;

    public List<Record>? Results { get; set; }
    public int Total { get; set; }
    public bool HasMore { get; set; }
    public string? Message { get; set; }

    private const int PageSize = 50;

    private async Task<bool> CheckSubscriptionAsync()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) { Message = "Not logged in."; return false; }
        if (user.Role == "Admin") return true;
        if (!user.IsActive) { Message = "Account deactivated. Contact admin."; return false; }
        if (user.SubscriptionExpiry == null || user.SubscriptionExpiry < DateTime.UtcNow)
        {
            Message = "Subscription expired. Please renew.";
            return false;
        }
        return true;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await CheckSubscriptionAsync()) return Page();

        // Only search if at least one criterion or state selected
        bool hasCriteria = !string.IsNullOrWhiteSpace(State) || !string.IsNullOrWhiteSpace(PhoneNumber) ||
            !string.IsNullOrWhiteSpace(OwnerName) || !string.IsNullOrWhiteSpace(FatherName) ||
            !string.IsNullOrWhiteSpace(Address) || !string.IsNullOrWhiteSpace(AadhaarId) ||
            !string.IsNullOrWhiteSpace(AlternativeNumber) || !string.IsNullOrWhiteSpace(Circle);

        if (!hasCriteria)
        {
            return Page();
        }

        if (CurrentPage < 1) CurrentPage = 1;

        var q = _db.Records.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(State)) q = q.Where(r => r.State == State);
        if (!string.IsNullOrWhiteSpace(PhoneNumber)) q = q.Where(r => r.PhoneNumber.Contains(PhoneNumber));
        if (!string.IsNullOrWhiteSpace(OwnerName)) q = q.Where(r => r.OwnerName.Contains(OwnerName));
        if (!string.IsNullOrWhiteSpace(FatherName)) q = q.Where(r => r.FatherName.Contains(FatherName));
        if (!string.IsNullOrWhiteSpace(Address)) q = q.Where(r => r.Address.Contains(Address));
        if (!string.IsNullOrWhiteSpace(AadhaarId)) q = q.Where(r => r.AadhaarId.Contains(AadhaarId));
        if (!string.IsNullOrWhiteSpace(AlternativeNumber)) q = q.Where(r => r.AlternativeNumber.Contains(AlternativeNumber));
        if (!string.IsNullOrWhiteSpace(Circle)) q = q.Where(r => r.Circle.Contains(Circle));

        Total = await q.CountAsync();
        Results = await q.OrderBy(r => r.Id).Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToListAsync();
        HasMore = CurrentPage * PageSize < Total;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // redirect to GET with query params
        var qs = new Dictionary<string, string?>
        {
            ["state"] = State,
            ["phoneNumber"] = PhoneNumber,
            ["ownerName"] = OwnerName,
            ["fatherName"] = FatherName,
            ["address"] = Address,
            ["aadhaarId"] = AadhaarId,
            ["alternativeNumber"] = AlternativeNumber,
            ["circle"] = Circle,
            ["page"] = "1"
        };
        var url = "/Search?" + string.Join("&", qs.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));
        return Redirect(url);
    }
}