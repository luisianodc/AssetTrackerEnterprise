using System.Security.Cryptography;
using System.Text;
using AssetTrackerEnterprise.Data;
using AssetTrackerEnterprise.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerEnterprise.Services;


// Provides a lightweight demonstration login and role checks.
public class AuthenticationService(AssetDbContext db)
{
    public AppUser? CurrentUser { get; private set; }

    public async Task<bool> LoginAsync(string userName, string password)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.UserName == userName);
        if (user is null || !VerifyPassword(password, user.PasswordHash)) return false;
        CurrentUser = user;
        return true;
    }

    public bool HasRole(params string[] roles) =>
        CurrentUser is not null && roles.Contains(CurrentUser.Role, StringComparer.OrdinalIgnoreCase);

    public static string HashPassword(string password) => PasswordHasher.HashPassword(password);

    private static bool VerifyPassword(string password, string stored) => PasswordHasher.VerifyPassword(password, stored);
}
