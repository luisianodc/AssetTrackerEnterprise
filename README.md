# Asset Tracker Enterprise (.NET 10)

A menu-driven C# console application for company asset management. It uses Entity Framework Core with SQLite, LINQ, inheritance, async database operations, role-based demo login, office currencies, reporting and export.
bla bla
## Requirements
- .NET  10  SDK
- Visual Studio 2022 (with .NET 10 support) or JetBrains Rider
- Internet access on first run to restore NuGet packages

## Run


Or use a terminal in the extracted folder:

```bash
git clone https://github.com/luisianodc/AssetTrackerEnterprise.git

dotnet build

dotnet run
```

The SQLite database `assettracker.db` is created automatically in the application's working directory on first launch. Starter offices, two employees and demo users are inserted automatically.

## Demo accounts
| Role | Username | Password |
|---|---|---|
| Admin | `admin` | `Admin123!` |
| Manager | `manager` | `Manager123!` |
| Employee | `employee` | `Employee123!` |

This is a learning/demo authentication system, not production identity management. Change the demo passwords before using any real data.

## Features
- Asset inheritance: `Asset`, `ComputerAsset`, `MobileAsset`, `Laptop`, `DesktopComputer`, `IPhone`, `Samsung`, `Nokia`, `Tablet`
- CRUD with validation and role checks
- SQLite persistence through EF Core
- Sorting by asset category, then purchase date
- Lifecycle status: YELLOW under 3 months, RED under 6 months, NORMAL above 6 months; expired assets are marked EXPIRED
- Offices in Sweden, USA, Germany and Turkey
- USD purchase prices converted to local currency using fixed editable educational rates
- Search by brand, model, office and purchase year
- Filters for expired, computers, mobile devices and office
- Pagination, employees and asset assignment
- Maintenance dates and notes
- Dashboard and office reports
- TXT, CSV and JSON export under the `Exports` folder
- Configuration in `appsettings.json` and console logging

## Currency rates
Edit `CurrencyRates` in `appsettings.json`. The sample rates are illustrative fixed rates, not live market rates:
- 1 USD = 10.5 SEK
- 1 USD = 0.92 EUR
- 1 USD = 34 TRY

## Database / migrations
The application uses `EnsureCreated` for a frictionless first run. If you want to use EF Core migrations during development, install the EF tool and replace `EnsureCreatedAsync` with `MigrateAsync` in `Program.cs`, then run:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/AssetTrackerEnterprise
dotnet ef database update --project src/AssetTrackerEnterprise
```

Do not use `EnsureCreated` and migrations together on the same database.

## Notes
- Asset lifecycle is calculated as three years from purchase date, following the supplied mini-project brief.
- The role simulation: Admin can create/update/delete; Manager can create/update; Employee can view and search. Maintenance and export are currently available to all logged-in roles.
- This is a complete educational console project. Before production use, add proper authorization, secret management, backups, audit trails, tests, and a real exchange-rate provider.
