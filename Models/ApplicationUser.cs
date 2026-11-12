using Microsoft.AspNetCore.Identity;

namespace DataSearch.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // Customer / Agency / Admin
    public DateTime? SubscriptionExpiry { get; set; }
    public string Plan { get; set; } = "Free";
    public bool IsActive { get; set; } = true;
}