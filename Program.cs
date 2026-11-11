using AssetTrackerEnterprise.Data;
using AssetTrackerEnterprise.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configure application services and load settings from appsettings.json.
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
services.AddLogging(builder => 
{
    builder.AddConsole();
    // Suppress EF Core command logging (info level)
    builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
});
services.AddDbContext<AssetDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("AssetDatabase")));
services.AddSingleton<IConfiguration>(configuration);
services.AddScoped<AssetService>();
services.AddScoped<OfficeService>();
services.AddScoped<EmployeeService>();
services.AddScoped<ReportService>();
services.AddScoped<ExportService>();
services.AddScoped<DashboardService>();
services.AddScoped<AuthenticationService>();
services.AddScoped<MaintenanceService>();
services.AddScoped<ConsoleApplication>();

using var provider = services.BuildServiceProvider();
using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AssetDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db);
    await scope.ServiceProvider.GetRequiredService<ConsoleApplication>().RunAsync();
}
