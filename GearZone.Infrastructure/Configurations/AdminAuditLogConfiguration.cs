using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations;

public sealed class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.ToTable("AdminAuditLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ActorUserId).HasMaxLength(450);
        builder.Property(x => x.ActorDisplayName).HasMaxLength(200);
        builder.Property(x => x.ActorEmail).HasMaxLength(256);
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.EntityId).HasMaxLength(100);
        builder.Property(x => x.EntityDisplayName).HasMaxLength(300);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.HttpMethod).HasMaxLength(10);
        builder.Property(x => x.RequestPath).HasMaxLength(1000);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
        // Leave unbounded strings provider-neutral: SQL Server maps them to
        // nvarchar(max), while SQLite uses TEXT for the in-memory test suite.
        builder.Property(x => x.ChangesJson);
        builder.Property(x => x.MetadataJson);

        builder.HasIndex(x => x.OccurredAtUtc);
        builder.HasIndex(x => new { x.ActorUserId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.Module, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.Outcome, x.RiskLevel, x.OccurredAtUtc });
        builder.HasIndex(x => x.CorrelationId);
    }
}
