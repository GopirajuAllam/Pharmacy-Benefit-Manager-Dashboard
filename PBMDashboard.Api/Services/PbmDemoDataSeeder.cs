using Microsoft.EntityFrameworkCore;
using PBMDashboard.Api.Data;
using PBMDashboard.Api.Domain;

namespace PBMDashboard.Api.Services;

public sealed class PbmDemoDataSeeder
{
    private readonly PbmDashboardDbContext _dbContext;
    private readonly IClock _clock;

    public PbmDemoDataSeeder(PbmDashboardDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await EnsureDashboardTablesAsync(cancellationToken);

        if (_dbContext.PbmFacilities.Any())
        {
            return;
        }

        var facilities = new[]
        {
            new PbmFacility { Name = "North Ridge Medical Center", Region = "Midwest", LicensedBeds = 280, ActivePatients = 1450 },
            new PbmFacility { Name = "Starlight Community Hospital", Region = "South", LicensedBeds = 190, ActivePatients = 980 },
            new PbmFacility { Name = "Cedar Valley Hospital", Region = "West", LicensedBeds = 320, ActivePatients = 1640 }
        };

        _dbContext.PbmFacilities.AddRange(facilities);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var now = _clock.UtcNow;
        var medications = new[] { "Metformin", "Ozempic", "Humira", "Lantus", "Eliquis", "Jardiance", "Stelara", "Rosuvastatin" };
        var departments = new[] { "Oncology", "Cardiology", "Emergency", "Inpatient", "Ambulatory" };
        var claims = new List<PharmacyClaim>();
        var priorAuthorizations = new List<PriorAuthorizationRequest>();

        for (var monthOffset = -5; monthOffset <= 0; monthOffset++)
        {
            var baseDate = new DateTime(now.Year, now.Month, 15, 12, 0, 0, DateTimeKind.Utc).AddMonths(monthOffset);

            for (var facilityIndex = 0; facilityIndex < facilities.Length; facilityIndex++)
            {
                var facility = facilities[facilityIndex];

                for (var i = 0; i < 18; i++)
                {
                    var isDenied = i % 7 == 0 && monthOffset >= -2;
                    var isPending = monthOffset == 0 && i % 11 == 0;
                    var isGeneric = i % 3 != 0;
                    var isSpecialty = i % 5 == 0;
                    var billedAmount = 180m + (facilityIndex * 40m) + (i * 18m) + ((monthOffset + 5) * 22m);
                    var paidAmount = isDenied ? 0m : decimal.Round(billedAmount * (isSpecialty ? 0.84m : 0.91m), 2);

                    claims.Add(new PharmacyClaim
                    {
                        PbmFacilityId = facility.Id,
                        MedicationName = medications[(facilityIndex + i + monthOffset + 5) % medications.Length],
                        Department = departments[(facilityIndex + i) % departments.Length],
                        FilledUtc = baseDate.AddDays((i % 14) - 6).AddHours(i),
                        BilledAmount = billedAmount,
                        PaidAmount = paidAmount,
                        IsGeneric = isGeneric,
                        IsSpecialty = isSpecialty,
                        TurnaroundHours = 4m + (i % 6) + (facilityIndex * 0.8m),
                        Status = isPending ? ClaimStatus.Pending : isDenied ? ClaimStatus.Denied : ClaimStatus.Approved
                    });

                    if (i % 4 == 0)
                    {
                        var authorizationStatus = i % 9 == 0 && monthOffset >= -1
                            ? PriorAuthorizationStatus.Pending
                            : i % 6 == 0
                                ? PriorAuthorizationStatus.Denied
                                : PriorAuthorizationStatus.Approved;

                        priorAuthorizations.Add(new PriorAuthorizationRequest
                        {
                            PbmFacilityId = facility.Id,
                            MedicationName = medications[(facilityIndex + i) % medications.Length],
                            RequestedUtc = baseDate.AddDays((i % 10) - 4),
                            ReviewedUtc = authorizationStatus == PriorAuthorizationStatus.Pending
                                ? null
                                : baseDate.AddDays((i % 10) - 3).AddHours(6 + facilityIndex),
                            Status = authorizationStatus,
                            IsUrgent = i % 8 == 0
                        });
                    }
                }
            }
        }

        _dbContext.PharmacyClaims.AddRange(claims);
        _dbContext.PriorAuthorizationRequests.AddRange(priorAuthorizations);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDashboardTablesAsync(CancellationToken cancellationToken)
    {
        if (!string.Equals(_dbContext.Database.ProviderName, "Microsoft.EntityFrameworkCore.Sqlite", StringComparison.Ordinal))
        {
            return;
        }

        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS PbmFacilities (
                Id INTEGER NOT NULL CONSTRAINT PK_PbmFacilities PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Region TEXT NOT NULL,
                LicensedBeds INTEGER NOT NULL,
                ActivePatients INTEGER NOT NULL
            );
            """,
            cancellationToken);

        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS PharmacyClaims (
                Id INTEGER NOT NULL CONSTRAINT PK_PharmacyClaims PRIMARY KEY AUTOINCREMENT,
                PbmFacilityId INTEGER NOT NULL,
                MedicationName TEXT NOT NULL,
                Department TEXT NOT NULL,
                FilledUtc TEXT NOT NULL,
                BilledAmount TEXT NOT NULL,
                PaidAmount TEXT NOT NULL,
                IsGeneric INTEGER NOT NULL,
                IsSpecialty INTEGER NOT NULL,
                TurnaroundHours TEXT NOT NULL,
                Status INTEGER NOT NULL,
                CONSTRAINT FK_PharmacyClaims_PbmFacilities_PbmFacilityId FOREIGN KEY (PbmFacilityId) REFERENCES PbmFacilities (Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_PharmacyClaims_PbmFacilityId ON PharmacyClaims (PbmFacilityId);
            """,
            cancellationToken);

        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS PriorAuthorizationRequests (
                Id INTEGER NOT NULL CONSTRAINT PK_PriorAuthorizationRequests PRIMARY KEY AUTOINCREMENT,
                PbmFacilityId INTEGER NOT NULL,
                MedicationName TEXT NOT NULL,
                RequestedUtc TEXT NOT NULL,
                ReviewedUtc TEXT NULL,
                Status INTEGER NOT NULL,
                IsUrgent INTEGER NOT NULL,
                CONSTRAINT FK_PriorAuthorizationRequests_PbmFacilities_PbmFacilityId FOREIGN KEY (PbmFacilityId) REFERENCES PbmFacilities (Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_PriorAuthorizationRequests_PbmFacilityId ON PriorAuthorizationRequests (PbmFacilityId);
            """,
            cancellationToken);
    }
}
