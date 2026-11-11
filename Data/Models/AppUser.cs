namespace AssetTrackerEnterprise.Models;

// Demo login account. Passwords are stored as salted PBKDF2 hashes.
public class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Employee";
}
