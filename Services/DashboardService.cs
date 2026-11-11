namespace AssetTrackerEnterprise.Services;

// Dashboard facade for summary reporting.
public class DashboardService(ReportService reports)
{
    public Task<(int Total, int Expired, int Assigned, string CommonType)> GetStatisticsAsync() =>
        reports.DashboardAsync();
}
