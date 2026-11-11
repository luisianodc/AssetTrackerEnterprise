using AssetTrackerEnterprise.Data;
using AssetTrackerEnterprise.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerEnterprise.Services;

// Builds business reports using database queries and LINQ.
public class ReportService(AssetDbContext db)
{
    public async Task<List<(Office Office, int Count, decimal TotalUsd)>> OfficeSummaryAsync()
    {
        var offices = await db.Offices.Include(o => o.Assets).OrderBy(o => o.OfficeName).ToListAsync();
        return offices.Select(o => (o, o.Assets.Count, o.Assets.Sum(a => a.PurchasePriceUsd))).ToList();
    }

    public Task<List<Asset>> ExpiringAsync() =>
        db.Assets.Include(a => a.Office).Where(a =>
            a.ExpectedEndOfLife >= DateTime.Today &&
            a.ExpectedEndOfLife <= DateTime.Today.AddMonths(6))
            .OrderBy(a => a.ExpectedEndOfLife).ToListAsync();

    public Task<List<Asset>> MostExpensiveAsync(int take = 10) =>
        db.Assets.Include(a => a.Office).OrderByDescending(a => a.PurchasePriceUsd).Take(take).ToListAsync();

    public async Task<(int Total, int Expired, int Assigned, string CommonType)> DashboardAsync()
    {
        var assets = await db.Assets.ToListAsync();
        var common = assets.GroupBy(a => a.AssetType).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "N/A";
        return (assets.Count, assets.Count(a => a.ExpectedEndOfLife < DateTime.Today),
            assets.Count(a => a.EmployeeId != null), common);
    }
}
