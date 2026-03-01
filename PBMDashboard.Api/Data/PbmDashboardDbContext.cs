using PBMDashboard.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace PBMDashboard.Api.Data;

public sealed class PbmDashboardDbContext : DbContext
{
    public PbmDashboardDbContext(DbContextOptions<PbmDashboardDbContext> options)
        : base(options)
    {
    }

    public DbSet<PbmFacility> PbmFacilities => Set<PbmFacility>();
    public DbSet<PharmacyClaim> PharmacyClaims => Set<PharmacyClaim>();
    public DbSet<PriorAuthorizationRequest> PriorAuthorizationRequests => Set<PriorAuthorizationRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PharmacyClaim>()
            .Property(claim => claim.BilledAmount)
            .HasColumnType("TEXT");

        modelBuilder.Entity<PharmacyClaim>()
            .Property(claim => claim.PaidAmount)
            .HasColumnType("TEXT");

        modelBuilder.Entity<PharmacyClaim>()
            .Property(claim => claim.TurnaroundHours)
            .HasColumnType("TEXT");
    }
}
