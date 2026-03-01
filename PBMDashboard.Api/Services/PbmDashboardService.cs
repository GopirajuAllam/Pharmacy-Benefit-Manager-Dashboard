using Microsoft.EntityFrameworkCore;
using PBMDashboard.Api.Contracts;
using PBMDashboard.Api.Data;
using PBMDashboard.Api.Domain;

namespace PBMDashboard.Api.Services;

public sealed class PbmDashboardService
{
    private readonly PbmDashboardDbContext _dbContext;
    private readonly IClock _clock;

    public PbmDashboardService(PbmDashboardDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<PbmDashboardResponse> BuildAsync(CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var sixMonthsStart = monthStart.AddMonths(-5);
        var todayStart = now.Date;

        var facilities = await _dbContext.PbmFacilities
            .AsNoTracking()
            .OrderBy(facility => facility.Name)
            .ToListAsync(cancellationToken);

        var claims = await _dbContext.PharmacyClaims
            .AsNoTracking()
            .Where(claim => claim.FilledUtc >= sixMonthsStart)
            .ToListAsync(cancellationToken);

        var priorAuthorizations = await _dbContext.PriorAuthorizationRequests
            .AsNoTracking()
            .Where(request => request.RequestedUtc >= sixMonthsStart)
            .ToListAsync(cancellationToken);

        var monthClaims = claims.Where(claim => claim.FilledUtc >= monthStart).ToList();
        var monthPriorAuthorizations = priorAuthorizations.Where(request => request.RequestedUtc >= monthStart).ToList();

        var summary = new DashboardSummary(
            facilities.Count,
            facilities.Sum(facility => facility.ActivePatients),
            claims.Count(claim => claim.FilledUtc >= todayStart),
            decimal.Round(monthClaims.Sum(claim => claim.PaidAmount), 2),
            CalculateRate(monthClaims.Count(claim => claim.IsGeneric), monthClaims.Count),
            CalculateRate(
                monthPriorAuthorizations.Count(request => request.Status == PriorAuthorizationStatus.Approved),
                monthPriorAuthorizations.Count),
            monthClaims.Count == 0 ? 0 : decimal.Round(monthClaims.Average(claim => claim.TurnaroundHours), 1),
            monthPriorAuthorizations.Count(request => request.IsUrgent && request.Status == PriorAuthorizationStatus.Pending));

        var monthlySpend = Enumerable.Range(0, 6)
            .Select(offset => monthStart.AddMonths(offset - 5))
            .Select(month =>
            {
                var nextMonth = month.AddMonths(1);
                var slice = claims.Where(claim => claim.FilledUtc >= month && claim.FilledUtc < nextMonth).ToList();

                return new MonthlySpendPoint(
                    month.ToString("MMM yyyy"),
                    decimal.Round(slice.Sum(claim => claim.PaidAmount), 2),
                    slice.Count,
                    slice.Count(claim => claim.Status == ClaimStatus.Denied));
            })
            .ToList();

        var departmentMix = monthClaims
            .GroupBy(claim => claim.Department)
            .OrderByDescending(group => group.Sum(claim => claim.PaidAmount))
            .Select(group => new DepartmentMixPoint(
                group.Key,
                group.Count(),
                decimal.Round(group.Sum(claim => claim.PaidAmount), 2)))
            .ToList();

        var facilityPerformance = facilities
            .Select(facility =>
            {
                var facilityClaims = monthClaims.Where(claim => claim.PbmFacilityId == facility.Id).ToList();
                var facilityAuthorizations = monthPriorAuthorizations.Where(request => request.PbmFacilityId == facility.Id).ToList();

                return new FacilityPerformancePoint(
                    facility.Name,
                    facility.Region,
                    facilityClaims.Count,
                    CalculateRate(
                        facilityClaims.Count(claim => claim.Status == ClaimStatus.Approved),
                        facilityClaims.Count),
                    facilityClaims.Count == 0 ? 0 : decimal.Round(facilityClaims.Average(claim => claim.TurnaroundHours), 1),
                    decimal.Round(facilityClaims.Sum(claim => claim.PaidAmount), 2),
                    facilityAuthorizations.Count(request => request.Status == PriorAuthorizationStatus.Pending));
            })
            .OrderByDescending(point => point.PaidAmount)
            .ToList();

        var alerts = BuildAlerts(facilities, facilityPerformance, priorAuthorizations, claims, now);

        return new PbmDashboardResponse(
            "Pharmacy Benefit Manager Dashboard",
            now,
            summary,
            monthlySpend,
            departmentMix,
            facilityPerformance,
            alerts);
    }

    private static List<OperationalAlert> BuildAlerts(
        IReadOnlyList<PbmFacility> sourceFacilities,
        IReadOnlyList<FacilityPerformancePoint> facilities,
        IReadOnlyList<PriorAuthorizationRequest> priorAuthorizations,
        IReadOnlyList<PharmacyClaim> claims,
        DateTime now)
    {
        var alerts = new List<OperationalAlert>();

        foreach (var facility in facilities.Where(facility => facility.ClaimsProcessed >= 10 && facility.ApprovalRate < 85))
        {
            alerts.Add(new OperationalAlert(
                "High",
                facility.Facility,
                $"Approval rate is {facility.ApprovalRate:N1}% for the current month."));
        }

        var urgentPendingByFacility = priorAuthorizations
            .Where(request => request.IsUrgent && request.Status == PriorAuthorizationStatus.Pending)
            .GroupBy(request => request.PbmFacilityId)
            .Select(group => new { FacilityId = group.Key, Count = group.Count() })
            .Where(group => group.Count > 0);

        foreach (var pending in urgentPendingByFacility)
        {
            var sourceFacility = sourceFacilities.FirstOrDefault(candidate => candidate.Id == pending.FacilityId);
            if (sourceFacility is null)
            {
                continue;
            }

            alerts.Add(new OperationalAlert(
                "Medium",
                sourceFacility.Name,
                $"{pending.Count} urgent prior authorizations still need review."));
        }

        var recentSpecialtySpend = claims
            .Where(claim => claim.IsSpecialty && claim.FilledUtc >= now.AddDays(-7))
            .GroupBy(claim => claim.PbmFacilityId)
            .Select(group => new { FacilityId = group.Key, Total = group.Sum(claim => claim.PaidAmount) })
            .OrderByDescending(group => group.Total)
            .FirstOrDefault();

        if (recentSpecialtySpend is not null)
        {
            var sourceFacility = sourceFacilities.FirstOrDefault(candidate => candidate.Id == recentSpecialtySpend.FacilityId);
            if (sourceFacility is not null)
            {
                alerts.Add(new OperationalAlert(
                    "Low",
                    sourceFacility.Name,
                    $"Specialty medication spend reached ${recentSpecialtySpend.Total:N0} in the last 7 days."));
            }
        }

        return alerts
            .DistinctBy(alert => $"{alert.Severity}:{alert.Facility}:{alert.Message}")
            .Take(6)
            .ToList();
    }

    private static decimal CalculateRate(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            return 0;
        }

        return decimal.Round((decimal)numerator / denominator * 100m, 1);
    }
}
