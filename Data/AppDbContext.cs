using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DataSearch.Models;

namespace DataSearch.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Record> Records => Set<Record>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Record>(e =>
        {
            e.HasIndex(r => r.State);
            e.HasIndex(r => r.PhoneNumber);
            e.HasIndex(r => r.OwnerName);
            e.HasIndex(r => r.FatherName);
            e.HasIndex(r => r.AadhaarId);
            e.HasIndex(r => r.AlternativeNumber);
            e.HasIndex(r => r.Circle);
            e.Property(r => r.PhoneNumber).HasMaxLength(20);
            e.Property(r => r.OwnerName).HasMaxLength(120);
            e.Property(r => r.FatherName).HasMaxLength(120);
            e.Property(r => r.Address).HasMaxLength(500);
            e.Property(r => r.AadhaarId).HasMaxLength(20);
            e.Property(r => r.AlternativeNumber).HasMaxLength(20);
            e.Property(r => r.Circle).HasMaxLength(40);
            e.Property(r => r.State).HasMaxLength(40);
        });
    }
}