using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class TrackingGeraetStandortAktuellConfiguration : IEntityTypeConfiguration<TrackingGeraetStandortAktuell>
{
    public void Configure(EntityTypeBuilder<TrackingGeraetStandortAktuell> builder)
    {
        builder.ToTable("TrackingGeraetStandorteAktuell");

        builder.HasKey(x => x.TrackingGeraetId);

        builder.Property(x => x.RouteSessionId)
            .IsRequired();

        builder.Property(x => x.Latitude)
            .IsRequired()
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.Longitude)
            .IsRequired()
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.SpeedKmh)
            .IsRequired()
            .HasColumnType("decimal(6,2)");

        builder.Property(x => x.DeviceIdentifier)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(x => x.ErfasstAmUtc)
            .HasDatabaseName("IX_TrackingGeraetStandorteAktuell_ErfasstAmUtc");
    }
}