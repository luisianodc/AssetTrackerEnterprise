using Microsoft.Extensions.Configuration;

namespace AssetTrackerEnterprise.Services;

// Converts USD amounts using configurable educational fixed exchange rates.
public class CurrencyService(IConfiguration configuration)
{
    public decimal ConvertFromUsd(decimal amount, string currencyCode)
    {
        var rate = configuration[$"CurrencyRates:{currencyCode}"];
        if (!decimal.TryParse(rate, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var exchangeRate))
            throw new InvalidOperationException($"No exchange rate configured for {currencyCode}.");
        return Math.Round(amount * exchangeRate, 2);
    }

    public static string Format(decimal amount, string currencyCode)
    {
        var culture = currencyCode switch
        {
            "SEK" => new System.Globalization.CultureInfo("sv-SE"),
            "EUR" => new System.Globalization.CultureInfo("de-DE"),
            "TRY" => new System.Globalization.CultureInfo("tr-TR"),
            _ => new System.Globalization.CultureInfo("en-US")
        };
        return amount.ToString("C", culture);
    }
}
