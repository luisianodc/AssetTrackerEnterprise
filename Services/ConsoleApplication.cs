using AssetTrackerEnterprise.Data;
using AssetTrackerEnterprise.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AssetTrackerEnterprise.Services;

// Menu-driven console user interface.
public class ConsoleApplication(
    AssetDbContext _,
    AssetService assets,
    OfficeService offices,
    EmployeeService employees,
    ReportService reports,
    ExportService exports,
    DashboardService dashboard,
    AuthenticationService auth,
    MaintenanceService maintenance,
    IConfiguration configuration,
    ILogger<ConsoleApplication> logger)
{
    public async Task RunAsync()
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("       ASSET TRACKER ENTERPRISE");
        Console.WriteLine("==========================================");
        if (!await LoginAsync()) return;

        var running = true;
        while (running)
        {
            Console.WriteLine($"\nLogged in: {auth.CurrentUser!.UserName} ({auth.CurrentUser.Role})");
            Console.WriteLine("1. Dashboard");
            Console.WriteLine("2. List assets");
            Console.WriteLine("3. Create asset");
            Console.WriteLine("4. Update asset");
            Console.WriteLine("5. Delete asset");
            Console.WriteLine("6. Search assets");
            Console.WriteLine("7. Filter assets");
            Console.WriteLine("8. Offices/report");
            Console.WriteLine("9. Employees/assign");
            Console.WriteLine("10. Maintenance");
            Console.WriteLine("11. Export");
            Console.WriteLine("0. Exit");

            var choice = Read("Select: ");
            if (string.IsNullOrWhiteSpace(choice))
            {
                Console.WriteLine("Invalid input: Empty selection.");
                OfferRetryOrExit(ref running);
                continue;
            }

            try
            {
                switch (choice)
                {
                    case "1": await ShowDashboardAsync(); break;
                    case "2": await ListAssetsAsync(); break;
                    case "3": await CreateAssetAsync(); break;
                    case "4": await UpdateAssetAsync(); break;
                    case "5": await DeleteAssetAsync(); break;
                    case "6": await SearchAsync(); break;
                    case "7": await FilterAsync(); break;
                    case "8": await OfficeReportAsync(); break;
                    case "9": await EmployeeMenuAsync(); break;
                    case "10": await MaintenanceAsync(); break;
                    case "11": await ExportAsync(); break;
                    case "0": running = false; break;
                    default: 
                        Console.WriteLine("Invalid menu choice. Please select an option from 0-11.");
                        OfferRetryOrExit(ref running);
                        break;
                }
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation("Operation cancelled by user: {Message}", ex.Message);
                Console.WriteLine("Operation cancelled. Returning to menu.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Operation failed");
                Console.WriteLine($"Error: {ex.Message}");
                OfferRetryOrExit(ref running);
            }
        }
        Console.WriteLine("Goodbye.");
    }

    private async Task<bool> LoginAsync()
    {
        Console.WriteLine("Demo accounts: admin/Admin123!, manager/Manager123!, employee/Employee123!");
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var user = Read("Username: ");
            var password = Read("Password: ");
            if (await auth.LoginAsync(user, password))
            {
                Console.WriteLine($"Welcome, {auth.CurrentUser!.UserName}.");
                return true;
            }
            Console.WriteLine("Invalid login.");
        }
        return false;
    }

    private async Task ShowDashboardAsync()
    {
        var stats = await dashboard.GetStatisticsAsync();
        var officeRows = await reports.OfficeSummaryAsync();
        Console.WriteLine($"\nTotal assets: {stats.Total} | Expired: {stats.Expired} | Assigned: {stats.Assigned}");
        Console.WriteLine($"Most used asset type: {stats.CommonType}");
        foreach (var row in officeRows)
        {
            var local = new CurrencyService(configuration).ConvertFromUsd(row.TotalUsd, row.Office.CurrencyCode);
            Console.WriteLine($"{row.Office.OfficeName}: {row.Count} assets | {CurrencyService.Format(local, row.Office.CurrencyCode)}");
        }
    }

    private async Task ListAssetsAsync()
    {
        var all = await assets.GetAllAsync();
        var pageSize = int.TryParse(configuration["Application:PageSize"], out var configuredPageSize) && configuredPageSize > 0 ? configuredPageSize : 10;
        var pages = Math.Max(1, (int)Math.Ceiling(all.Count / (double)pageSize));
        var page = 1;
        do
        {
            var rows = await assets.GetPageAsync(page, pageSize);
            PrintAssets(rows);
            Console.WriteLine($"Page {page}/{pages} - N next, P previous, Enter return");
            var action = Read("> ").ToLowerInvariant();
            if (action == "n" && page < pages) page++;
            else if (action == "p" && page > 1) page--;
            else break;
        } while (true);
    }

    private async Task CreateAssetAsync()
    {
        if (!auth.HasRole("Admin", "Manager")) { Denied(); return; }
        Console.WriteLine("Asset type: 1 Laptop, 2 Desktop, 3 iPhone, 4 Samsung, 5 Nokia, 6 Tablet");
        Asset asset = Read("Type: ") switch
        {
            "1" => new Laptop(),
            "2" => new DesktopComputer(),
            "3" => new IPhone(),
            "4" => new Samsung(),
            "5" => new Nokia(),
            "6" => new Tablet(),
            _ => throw new ArgumentException("Unknown asset type.")
        };
        asset.Brand = Read("Brand: ");
        asset.ModelName = Read("Model: ");
        asset.SerialNumber = Read("Serial number: ");
        asset.PurchaseDate = ReadDate("Purchase date (yyyy-mm-dd): ");
        asset.WarrantyExpirationDate = ReadDate("Warranty expiration (yyyy-mm-dd): ");
        asset.PurchasePriceUsd = ReadDecimal("Purchase price USD: ");
        asset.OfficeId = await SelectOfficeAsync();
        if (asset is ComputerAsset computer)
        {
            computer.OperatingSystem = Read("Operating system: ");
            computer.RamGb = ReadInt("RAM (GB): ");
        }
        else if (asset is MobileAsset mobile)
        {
            mobile.OperatingSystem = Read("Operating system: ");
        }
        await assets.AddAsync(asset);
        Console.WriteLine($"Asset created with ID {asset.Id}.");
    }

    private async Task UpdateAssetAsync()
    {
        if (!auth.HasRole("Admin", "Manager")) { Denied(); return; }
        var id = ReadInt("Asset ID: ");
        var asset = await assets.GetByIdAsync(id);
        if (asset is null) { Console.WriteLine("Asset not found."); return; }
        Console.WriteLine($"Current: {asset.Brand} {asset.ModelName}. Leave fields blank to keep current values.");
        var brand = Read("New brand: ");
        var model = Read("New model: ");
        var price = Read("New USD price: ");
        await assets.UpdateAsync(id, a =>
        {
            if (!string.IsNullOrWhiteSpace(brand)) a.Brand = brand;
            if (!string.IsNullOrWhiteSpace(model)) a.ModelName = model;
            if (!string.IsNullOrWhiteSpace(price) && decimal.TryParse(price, out var p)) a.PurchasePriceUsd = p;
        });
        Console.WriteLine("Asset updated.");
    }

    private async Task DeleteAssetAsync()
    {
        if (!auth.HasRole("Admin")) { Denied(); return; }
        var id = ReadInt("Asset ID: ");
        if (Read("Confirm delete (y/n): ").Equals("y", StringComparison.OrdinalIgnoreCase))
            Console.WriteLine(await assets.DeleteAsync(id) ? "Asset deleted." : "Asset not found.");
    }

    private async Task SearchAsync()
    {
        var brand = Read("Brand (blank = any): ");
        var model = Read("Model (blank = any): ");
        var office = Read("Office (blank = any): ");
        var yearText = Read("Purchase year (blank = any): ");
        int? year = int.TryParse(yearText, out var y) ? y : null;
        PrintAssets(await assets.SearchAsync(brand, model, office, year));
    }

    private async Task FilterAsync()
    {
        Console.WriteLine("1 Expired  2 Computers  3 Mobile  4 By office");
        var choice = Read("Filter: ");
        var filter = choice switch { "1" => "expired", "2" => "computers", "3" => "mobile", _ => "all" };
        int? officeId = choice == "4" ? await SelectOfficeAsync() : null;
        PrintAssets(await assets.FilterAsync(filter, officeId));
    }

    private async Task OfficeReportAsync()
    {
        foreach (var office in await offices.GetAllAsync())
            Console.WriteLine($"{office.Id}. {office.OfficeName} | {office.Country} | {office.CurrencyCode}");
        await ShowDashboardAsync();
        Console.WriteLine("\nExpiring within six months:");
        PrintAssets(await reports.ExpiringAsync());
        Console.WriteLine("\nMost expensive assets:");
        PrintAssets(await reports.MostExpensiveAsync());
    }

    private async Task EmployeeMenuAsync()
    {
        if (!auth.HasRole("Admin", "Manager")) { Denied(); return; }
        Console.WriteLine("1 List employees  2 Add employee  3 Assign/unassign asset");
        switch (Read("Choice: "))
        {
            case "1":
                foreach (var e in await employees.GetAllAsync())
                    Console.WriteLine($"{e.Id}. {e.FullName} | {e.Department} | {e.Email} | {e.AssignedAssets.Count} assets");
                break;
            case "2":
                var officeId = await SelectOfficeAsync();
                await employees.AddAsync(Read("Full name: "), Read("Department: "), Read("Email: "), officeId);
                Console.WriteLine("Employee added.");
                break;
            case "3":
                var assetId = ReadInt("Asset ID: ");
                var employeeText = Read("Employee ID (0 to unassign): ");
                var employeeId = int.TryParse(employeeText, out var eid) && eid > 0 ? eid : (int?)null;
                Console.WriteLine(await assets.AssignAsync(assetId, employeeId) ? "Assignment saved." : "Asset or employee not found.");
                break;
        }
    }

    private async Task MaintenanceAsync()
    {
        var id = ReadInt("Asset ID: ");
        var last = ReadOptionalDate("Last maintenance (yyyy-mm-dd, blank = none): ");
        var next = ReadOptionalDate("Next maintenance (yyyy-mm-dd, blank = none): ");
        var notes = Read("Maintenance notes: ");
        Console.WriteLine(await maintenance.UpdateAsync(id, last, next, notes) ? "Maintenance saved." : "Asset not found.");
    }

    private async Task ExportAsync()
    {
        var format = Read("Export format (txt/csv/json): ").ToLowerInvariant();
        await exports.ExportAsync(await assets.GetAllAsync(), format);
    }

    private async Task<int> SelectOfficeAsync()
    {
        var list = await offices.GetAllAsync();
        foreach (var office in list) Console.WriteLine($"{office.Id}. {office.OfficeName} ({office.CurrencyCode})");
        var id = ReadInt("Office ID: ");
        if (!list.Any(o => o.Id == id)) throw new ArgumentException("Select a valid office.");
        return id;
    }

    private static void PrintAssets(IEnumerable<Asset> list)
    {
        var rows = list.ToList();
        if (rows.Count == 0) { Console.WriteLine("No assets found."); return; }

        Console.WriteLine("\n");
        Console.WriteLine("┌──────┬──────────┬─────────────────────┬──────────────────┬─────────────┬────────────┬──────────┬──────────────────┬──────────────┐");
        Console.WriteLine("│  ID  │ CATEGORY │   BRAND / MODEL     │     OFFICE       │  PURCHASE   │    USD     │  STATUS  │  ASSIGNED TO     │  LOCAL PRICE │");
        Console.WriteLine("├──────┼──────────┼─────────────────────┼──────────────────┼─────────────┼────────────┼──────────┼──────────────────┼──────────────┤");

        foreach (var a in rows)
        {
            var rate = a.Office?.CurrencyCode switch { "SEK" => 10.5m, "EUR" => 0.92m, "TRY" => 34.0m, _ => 1.0m };
            var local = Math.Round(a.PurchasePriceUsd * rate, 2);
            var localText = a.Office is null ? "" : CurrencyService.Format(local, a.Office.CurrencyCode);
            var status = a.LifetimeStatus;
            var color = status switch { "YELLOW" => ConsoleColor.Yellow, "RED" => ConsoleColor.Red, "EXPIRED" => ConsoleColor.DarkRed, _ => ConsoleColor.Gray };

            var idStr = a.Id.ToString().PadRight(4);
            var typeStr = a.AssetType.PadRight(8);
            var brandModelStr = $"{a.Brand} {a.ModelName}".Length > 19 ? $"{a.Brand} {a.ModelName}".Substring(0, 16) + "..." : $"{a.Brand} {a.ModelName}".PadRight(19);
            var officeStr = (a.Office?.OfficeName ?? "-").Length > 16 ? (a.Office?.OfficeName ?? "-").Substring(0, 13) + "..." : (a.Office?.OfficeName ?? "-").PadRight(16);
            var purchaseStr = a.PurchaseDate.ToString("yyyy-MM-dd").PadRight(11);
            var usdStr = a.PurchasePriceUsd.ToString("C").PadRight(10);
            var assignedStr = (a.Employee?.FullName ?? "-").Length > 16 ? (a.Employee?.FullName ?? "-").Substring(0, 13) + "..." : (a.Employee?.FullName ?? "-").PadRight(16);
            var localStr = localText.PadRight(12);

            Console.Write($"│ {idStr} │ {typeStr} │ {brandModelStr} │ {officeStr} │ {purchaseStr} │ {usdStr} │ ");

            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(status.PadRight(6));
            Console.ForegroundColor = old;

            Console.WriteLine($" │ {assignedStr} │ {localStr} │");
        }

        Console.WriteLine("└──────┴──────────┴─────────────────────┴──────────────────┴─────────────┴────────────┴──────────┴──────────────────┴──────────────┘");
    }

    private static string Read(string prompt) { Console.Write(prompt); return Console.ReadLine()?.Trim() ?? ""; }

    private static void OfferRetryOrExit(ref bool running)
    {
        Console.WriteLine("\nOptions: R = Retry, E = Exit to menu, 0 = Exit application");
        var response = Read("Choose: ").ToLowerInvariant().Trim();

        if (response == "e" || response == "exit")
        {
            return;
        }
        else if (response == "0")
        {
            running = false;
        }
        else if (response == "r" || response == "retry")
        {
            return;
        }
        else
        {
            Console.WriteLine("Invalid response. Returning to menu.");
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            var input = Read(prompt);
            if (int.TryParse(input, out var value) && value > 0)
            {
                return value;
            }

            Console.WriteLine("Error: Enter a valid positive number.");
            Console.WriteLine("Options: R = Retry, E = Exit to menu");
            var response = Read("Choose: ").ToLowerInvariant().Trim();

            if (response == "e" || response == "exit")
            {
                throw new OperationCanceledException("User cancelled the operation.");
            }
            else if (response != "r" && response != "retry")
            {
                Console.WriteLine("Invalid response. Returning to menu.");
                throw new OperationCanceledException("User cancelled the operation.");
            }
        }
    }

    private static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            var input = Read(prompt);
            if (decimal.TryParse(input, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var value) && value >= 0)
            {
                return value;
            }

            Console.WriteLine("Error: Enter a valid non-negative decimal using dot as decimal separator.");
            Console.WriteLine("Options: R = Retry, E = Exit to menu");
            var response = Read("Choose: ").ToLowerInvariant().Trim();

            if (response == "e" || response == "exit")
            {
                throw new OperationCanceledException("User cancelled the operation.");
            }
            else if (response != "r" && response != "retry")
            {
                Console.WriteLine("Invalid response. Returning to menu.");
                throw new OperationCanceledException("User cancelled the operation.");
            }
        }
    }

    private static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            var input = Read(prompt);
            if (DateTime.TryParse(input, out var value))
            {
                return value.Date;
            }

            Console.WriteLine("Error: Enter a valid date in format yyyy-mm-dd.");
            Console.WriteLine("Options: R = Retry, E = Exit to menu");
            var response = Read("Choose: ").ToLowerInvariant().Trim();

            if (response == "e" || response == "exit")
            {
                throw new OperationCanceledException("User cancelled the operation.");
            }
            else if (response != "r" && response != "retry")
            {
                Console.WriteLine("Invalid response. Returning to menu.");
                throw new OperationCanceledException("User cancelled the operation.");
            }
        }
    }

    private static DateTime? ReadOptionalDate(string prompt)
    {
        while (true)
        {
            var text = Read(prompt);
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (DateTime.TryParse(text, out var date))
            {
                return date.Date;
            }

            Console.WriteLine("Error: Enter a valid date in format yyyy-mm-dd, or leave blank to skip.");
            Console.WriteLine("Options: R = Retry, E = Exit to menu");
            var response = Read("Choose: ").ToLowerInvariant().Trim();

            if (response == "e" || response == "exit")
            {
                throw new OperationCanceledException("User cancelled the operation.");
            }
            else if (response != "r" && response != "retry")
            {
                Console.WriteLine("Invalid response. Returning to menu.");
                throw new OperationCanceledException("User cancelled the operation.");
            }
        }
    }

    private static void Denied() => Console.WriteLine("Access denied for your role.");
}
