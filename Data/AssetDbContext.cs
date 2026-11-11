using AssetTrackerEnterprise.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerEnterprise.Data;

// EF Core database context.
public class AssetDbContext(DbContextOptions<AssetDbContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Office> Offices => Set<Office>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>()
            .HasDiscriminator<string>("AssetType")
            .HasValue<Laptop>("Laptop")
            .HasValue<DesktopComputer>("Desktop Computer")
            .HasValue<IPhone>("iPhone")
            .HasValue<Samsung>("Samsung")
            .HasValue<Nokia>("Nokia")
            .HasValue<Tablet>("Tablet");

        modelBuilder.Entity<Asset>().Property(a => a.PurchasePriceUsd).HasPrecision(18, 2);
        modelBuilder.Entity<Asset>()
            .HasOne(a => a.Office).WithMany(o => o.Assets)
            .HasForeignKey(a => a.OfficeId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Asset>()
            .HasOne(a => a.Employee).WithMany(e => e.AssignedAssets)
            .HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Office).WithMany()
            .HasForeignKey(e => e.OfficeId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AppUser>().HasIndex(u => u.UserName).IsUnique();
    }
}
