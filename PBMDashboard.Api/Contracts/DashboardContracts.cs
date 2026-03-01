namespace PBMDashboard.Api.Contracts;

public sealed record PbmDashboardResponse(
    string DashboardTitle,
    DateTime GeneratedUtc,
    DashboardSummary Summary,
    IReadOnlyList<MonthlySpendPoint> MonthlySpend,
    IReadOnlyList<DepartmentMixPoint> DepartmentMix,
    IReadOnlyList<FacilityPerformancePoint> Facilities,
    IReadOnlyList<OperationalAlert> Alerts);

public sealed record DashboardSummary(
    int ActiveFacilities,
    int ActivePatients,
    int ClaimsToday,
    decimal PaidAmountMonthToDate,
    decimal GenericDispenseRate,
    decimal PriorAuthorizationApprovalRate,
    decimal AverageTurnaroundHours,
    int PendingUrgentAuthorizations);

public sealed record MonthlySpendPoint(
    string Month,
    decimal PaidAmount,
    int ClaimsProcessed,
    int DeniedClaims);

public sealed record DepartmentMixPoint(
    string Department,
    int ClaimsProcessed,
    decimal PaidAmount);

public sealed record FacilityPerformancePoint(
    string Facility,
    string Region,
    int ClaimsProcessed,
    decimal ApprovalRate,
    decimal AverageTurnaroundHours,
    decimal PaidAmount,
    int PendingAuthorizations);

public sealed record OperationalAlert(
    string Severity,
    string Facility,
    string Message);
