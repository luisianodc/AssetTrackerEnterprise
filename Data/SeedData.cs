using AssetTrackerEnterprise.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerEnterprise.Data;

// Creates starter offices, employees and demo users on first launch.
public static class SeedData
{
    public static async Task InitializeAsync(AssetDbContext db)
    {
        if (!await db.Offices.AnyAsync())
        {
            db.Offices.AddRange(
                new Office { OfficeName = "Sweden Office", Country = "Sweden", CurrencyCode = "SEK" },
                new Office { OfficeName = "USA Office", Country = "United States", CurrencyCode = "USD" },
                new Office { OfficeName = "Germany Office", Country = "Germany", CurrencyCode = "EUR" },
                new Office { OfficeName = "Turkey Office", Country = "Turkey", CurrencyCode = "TRY" });
            await db.SaveChangesAsync();
        }

        if (!await db.Employees.AnyAsync())
        {
            var offices = await db.Offices.OrderBy(o => o.Id).ToListAsync();
            db.Employees.AddRange(
                new Employee { FullName = "Alex Johnson", Department = "IT", Email = "alex@example.com", OfficeId = offices[0].Id },
                new Employee { FullName = "Sam Taylor", Department = "Finance", Email = "sam@example.com", OfficeId = offices[1].Id });
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync())
        {
            var users = new[]
            {
                new AppUser { UserName = "admin", Role = "Admin", PasswordHash = PasswordHasher.HashPassword("Admin123!") },
                new AppUser { UserName = "manager", Role = "Manager", PasswordHash = PasswordHasher.HashPassword("Manager123!") },
                new AppUser { UserName = "employee", Role = "Employee", PasswordHash = PasswordHasher.HashPassword("Employee123!") }
            };
            db.Users.AddRange(users);
            await db.SaveChangesAsync();
        }
    }
}
