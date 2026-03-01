using System.ComponentModel.DataAnnotations;

namespace PBMDashboard.Api.Domain;

public sealed class PbmFacility
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Region { get; set; } = string.Empty;

    public int LicensedBeds { get; set; }
    public int ActivePatients { get; set; }
}

public sealed class PharmacyClaim
{
    public int Id { get; set; }
    public int PbmFacilityId { get; set; }

    [MaxLength(120)]
    public string MedicationName { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Department { get; set; } = string.Empty;

    public DateTime FilledUtc { get; set; }
    public decimal BilledAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public bool IsGeneric { get; set; }
    public bool IsSpecialty { get; set; }
    public decimal TurnaroundHours { get; set; }
    public ClaimStatus Status { get; set; }
}

public sealed class PriorAuthorizationRequest
{
    public int Id { get; set; }
    public int PbmFacilityId { get; set; }

    [MaxLength(120)]
    public string MedicationName { get; set; } = string.Empty;

    public DateTime RequestedUtc { get; set; }
    public DateTime? ReviewedUtc { get; set; }
    public PriorAuthorizationStatus Status { get; set; }
    public bool IsUrgent { get; set; }
}

public enum ClaimStatus
{
    Approved = 1,
    Denied = 2,
    Pending = 3
}

public enum PriorAuthorizationStatus
{
    Approved = 1,
    Denied = 2,
    Pending = 3
}
