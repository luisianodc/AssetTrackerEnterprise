using AssetTrackerEnterprise.Data;
using AssetTrackerEnterprise.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerEnterprise.Services;

// Reads and maintains company offices.
public class OfficeService(AssetDbContext db)
{
    public Task<List<Office>> GetAllAsync() => db.Offices.OrderBy(o => o.OfficeName).ToListAsync();
    public Task<Office?> GetAsync(int id) => db.Offices.FindAsync(id).AsTask();

    public async Task AddAsync(string name, string country, string currency)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(country) ||
            string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("All office fields are required.");
        db.Offices.Add(new Office { OfficeName = name.Trim(), Country = country.Trim(), CurrencyCode = currency.Trim().ToUpperInvariant() });
        await db.SaveChangesAsync();
    }
}
