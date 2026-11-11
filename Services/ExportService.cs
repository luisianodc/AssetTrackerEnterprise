using System.Text;
using System.Text.Json;
using AssetTrackerEnterprise.Models;

namespace AssetTrackerEnterprise.Services;

// Exports asset reports as text, CSV or JSON files.
public class ExportService
{
    public async Task ExportAsync(IEnumerable<Asset> assets, string format)
    {
        var list = assets.ToList();
        var folder = Path.Combine(AppContext.BaseDirectory, "Exports");
        Directory.CreateDirectory(folder);
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var path = Path.Combine(folder, $"assets_{stamp}.{format}");
        switch (format.ToLowerInvariant())
        {
            case "json":
                await File.WriteAllTextAsync(path, JsonSerializer.Serialize(list.Select(ToRow()), new JsonSerializerOptions { WriteIndented = true }));
                break;
            case "csv":
                var csv = new StringBuilder("Id,Type,Brand,Model,PurchaseDate,PriceUSD,Office,SerialNumber,Status\n");
                foreach (var a in list) csv.AppendLine(string.Join(",", new[] { a.Id.ToString(), a.AssetType, a.Brand, a.ModelName,
                    a.PurchaseDate.ToString("yyyy-MM-dd"), a.PurchasePriceUsd.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    a.Office?.OfficeName ?? "", a.SerialNumber, a.LifetimeStatus }.Select(Escape)));
                await File.WriteAllTextAsync(path, csv.ToString(), Encoding.UTF8);
                break;
            case "txt":
                await File.WriteAllLinesAsync(path, list.Select(a => $"{a.Id} | {a.AssetType} | {a.Brand} {a.ModelName} | {a.Office?.OfficeName} | {a.PurchasePriceUsd:C} USD | {a.LifetimeStatus}"));
                break;
            default: throw new ArgumentException("Format must be TXT, CSV or JSON.");
        }
        Console.WriteLine($"Export saved: {path}");
    }

    private static Func<Asset, object> ToRow() => a => new
    {
        a.Id, a.AssetType, a.Brand, a.ModelName, a.PurchaseDate, a.PurchasePriceUsd,
        Office = a.Office?.OfficeName, a.SerialNumber, a.WarrantyExpirationDate, a.LifetimeStatus
    };

    private static string Escape(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
}
