using AssetTrackerEnterprise.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerEnterprise.Services;

// Updates maintenance information for an asset.
public class MaintenanceService(AssetDbContext db)
{
    public async Task<bool> UpdateAsync(int assetId, DateTime? lastDate, DateTime? nextDate, string notes)
    {
        var asset = await db.Assets.FindAsync(assetId);
        if (asset is null) return false;
        if (lastDate.HasValue && nextDate.HasValue && nextDate < lastDate)
            throw new ArgumentException("Next maintenance date cannot be before last maintenance date.");
        asset.LastMaintenanceDate = lastDate;
        asset.NextMaintenanceDate = nextDate;
        asset.MaintenanceNotes = notes.Trim();
        await db.SaveChangesAsync();
        return true;
    }
}
