using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class TrackingGeraetStandortHistorienpunktConfiguration : IEntityTypeConfiguration<TrackingGeraetStandortHistorienpunkt>
{
    public void Configure(EntityTypeBuilder<TrackingGeraetStandortHistorienpunkt> builder)
    {
        builder.ToTable("TrackingGeraetStandortHistorie");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TrackingGeraetId)
            .IsRequired();

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

        builder.HasIndex(x => new { x.TrackingGeraetId, x.ErfasstAmUtc })
            .HasDatabaseName("IX_TrackingGeraetStandortHistorie_Geraet_ErfasstAmUtc");

        builder.HasIndex(x => new { x.RouteSessionId, x.ErfasstAmUtc })
            .HasDatabaseName("IX_TrackingGeraetStandortHistorie_RouteSession_ErfasstAmUtc");
    }
}