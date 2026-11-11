namespace AssetTrackerEnterprise.Models;

// Employee record used for asset assignment.
public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Department { get; set; } = "";
    public string Email { get; set; } = "";
    public int OfficeId { get; set; }
    public Office? Office { get; set; }
    public ICollection<Asset> AssignedAssets { get; set; } = new List<Asset>();
}
