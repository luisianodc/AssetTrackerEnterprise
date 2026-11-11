using AssetTrackerEnterprise.Data;
using AssetTrackerEnterprise.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerEnterprise.Services;

// Handles asset CRUD, search, filtering, sorting and assignment.
public class AssetService(AssetDbContext db)
{
    public async Task<List<Asset>> GetAllAsync() =>
        await db.Assets.Include(a => a.Office).Include(a => a.Employee)
            .OrderBy(a => a.AssetType).ThenBy(a => a.PurchaseDate).ToListAsync();

    public async Task<Asset?> GetByIdAsync(int id) =>
        await db.Assets.Include(a => a.Office).Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task AddAsync(Asset asset)
    {
        Validate(asset);
        db.Assets.Add(asset);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(int id, Action<Asset> update)
    {
        var asset = await db.Assets.FindAsync(id);
        if (asset is null) return false;
        update(asset);
        Validate(asset);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var asset = await db.Assets.FindAsync(id);
        if (asset is null) return false;
        db.Assets.Remove(asset);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Asset>> SearchAsync(string? brand, string? model, string? office, int? year)
    {
        var query = db.Assets.Include(a => a.Office).Include(a => a.Employee).AsQueryable();
        if (!string.IsNullOrWhiteSpace(brand)) query = query.Where(a => a.Brand.Contains(brand));
        if (!string.IsNullOrWhiteSpace(model)) query = query.Where(a => a.ModelName.Contains(model));
        if (!string.IsNullOrWhiteSpace(office)) query = query.Where(a => a.Office!.OfficeName.Contains(office));
        if (year.HasValue) query = query.Where(a => a.PurchaseDate.Year == year.Value);
        return await query.OrderBy(a => a.AssetType).ThenBy(a => a.PurchaseDate).ToListAsync();
    }

    public async Task<List<Asset>> FilterAsync(string filter, int? officeId = null)
    {
        var query = db.Assets.Include(a => a.Office).Include(a => a.Employee).AsQueryable();
        query = filter.ToLowerInvariant() switch
        {
            "expired" => query.Where(a => a.ExpectedEndOfLife < DateTime.Today),
            "computers" => query.Where(a => a is Laptop || a is DesktopComputer),
            "mobile" => query.Where(a => a is IPhone || a is Samsung || a is Nokia || a is Tablet),
            _ => query
        };
        if (officeId.HasValue) query = query.Where(a => a.OfficeId == officeId.Value);
        return await query.OrderBy(a => a.AssetType).ThenBy(a => a.PurchaseDate).ToListAsync();
    }

    public async Task<List<Asset>> GetPageAsync(int page, int pageSize)
    {
        return await db.Assets.Include(a => a.Office).Include(a => a.Employee)
            .OrderBy(a => a.AssetType).ThenBy(a => a.PurchaseDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<bool> AssignAsync(int assetId, int? employeeId)
    {
        var asset = await db.Assets.FindAsync(assetId);
        if (asset is null) return false;
        if (employeeId.HasValue && !await db.Employees.AnyAsync(e => e.Id == employeeId.Value)) return false;
        asset.EmployeeId = employeeId;
        await db.SaveChangesAsync();
        return true;
    }

    private static void Validate(Asset asset)
    {
        if (string.IsNullOrWhiteSpace(asset.Brand) || string.IsNullOrWhiteSpace(asset.ModelName))
            throw new ArgumentException("Brand and model are required.");
        if (string.IsNullOrWhiteSpace(asset.SerialNumber))
            throw new ArgumentException("Serial number is required.");
        if (asset.PurchaseDate.Date > DateTime.Today)
            throw new ArgumentException("Purchase date cannot be in the future.");
        if (asset.WarrantyExpirationDate.Date < asset.PurchaseDate.Date)
            throw new ArgumentException("Warranty expiration cannot be earlier than purchase date.");
        if (asset.PurchasePriceUsd < 0) throw new ArgumentException("Price cannot be negative.");
    }
}
