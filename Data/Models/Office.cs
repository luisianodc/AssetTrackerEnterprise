namespace AssetTrackerEnterprise.Models;

// A company location with its local reporting currency.
public class Office
{
    public int Id { get; set; }
    public string OfficeName { get; set; } = "";
    public string Country { get; set; } = "";
    public string CurrencyCode { get; set; } = "USD";
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
