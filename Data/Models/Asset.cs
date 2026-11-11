namespace AssetTrackerEnterprise.Models;

// Base class for every company asset.
public abstract class Asset
{
    public int Id { get; set; }
    public string AssetType { get; set; } = "";
    public string Brand { get; set; } = "";
    public string ModelName { get; set; } = "";
    public DateTime PurchaseDate { get; set; }
    public decimal PurchasePriceUsd { get; set; }
    public string SerialNumber { get; set; } = "";
    public DateTime WarrantyExpirationDate { get; set; }
    public int OfficeId { get; set; }
    public Office? Office { get; set; }
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string MaintenanceNotes { get; set; } = "";

    // The assignment brief defines the normal asset lifecycle as three years.
    public DateTime ExpectedEndOfLife => PurchaseDate.AddYears(3);

    public int RemainingDays => (ExpectedEndOfLife - DateTime.Today).Days;

    public string LifetimeStatus
    {
        get
        {
            var remaining = ExpectedEndOfLife - DateTime.Today;
            if (remaining <= TimeSpan.Zero) return "EXPIRED";
            if (remaining.TotalDays < 90) return "YELLOW";
            if (remaining.TotalDays < 180) return "RED";
            return "NORMAL";
        }
    }
}
