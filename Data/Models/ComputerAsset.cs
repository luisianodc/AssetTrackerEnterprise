namespace AssetTrackerEnterprise.Models;

// Shared properties for laptop and desktop computer assets.
public abstract class ComputerAsset : Asset
{
    public string OperatingSystem { get; set; } = "";
    public int RamGb { get; set; }
}
