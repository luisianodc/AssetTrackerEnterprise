using AssetTrackerEnterprise.Data;
using AssetTrackerEnterprise.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerEnterprise.Services;

// Employee directory and asset assignment support.
public class EmployeeService(AssetDbContext db)
{
    public Task<List<Employee>> GetAllAsync() =>
        db.Employees.Include(e => e.Office).Include(e => e.AssignedAssets).OrderBy(e => e.FullName).ToListAsync();

    public async Task AddAsync(string name, string department, string email, int officeId)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Name and email are required.");
        db.Employees.Add(new Employee { FullName = name.Trim(), Department = department.Trim(), Email = email.Trim(), OfficeId = officeId });
        await db.SaveChangesAsync();
    }
}
