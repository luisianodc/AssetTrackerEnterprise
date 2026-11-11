namespace AssetTrackerEnterprise.Models;

// Shared properties for mobile devices.
public abstract class MobileAsset : Asset
{
    public string OperatingSystem { get; set; } = "";
    public bool HasCellularConnection { get; set; } = true;
}
