using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DataSearch.Data;
using DataSearch.Models;

namespace DataSearch.Pages;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class ReportModel : PageModel
{
    private readonly AppDbContext _db;
    public ReportModel(AppDbContext db) { _db = db; }

    public List<Payment> Payments { get; set; } = new();
    public Dictionary<string, decimal> MonthlyTotals { get; set; } = new();
    public decimal GrandTotal { get; set; }

    public async Task OnGetAsync()
    {
        Payments = await _db.Payments.Include(p => p.User).OrderByDescending(p => p.PaidOn).ToListAsync();
        MonthlyTotals = Payments
            .GroupBy(p => p.PaidOn.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
        GrandTotal = Payments.Sum(p => p.Amount);
    }
}