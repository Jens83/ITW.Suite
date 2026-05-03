using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class TrackingGeraetEinrichtungscodeConfiguration : IEntityTypeConfiguration<TrackingGeraetEinrichtungscode>
{
    public void Configure(EntityTypeBuilder<TrackingGeraetEinrichtungscode> builder)
    {
        builder.ToTable("TrackingGeraetEinrichtungscodes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TabletName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.DeviceIdentifier)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ErstelltVonUserId)
            .HasMaxLength(450);

        builder.HasIndex(x => x.CodeHash)
            .HasDatabaseName("IX_TrackingGeraetEinrichtungscodes_CodeHash");

        builder.HasIndex(x => new { x.EingeloestAmUtc, x.GueltigBisUtc })
            .HasDatabaseName("IX_TrackingGeraetEinrichtungscodes_Status_GueltigBis");
    }
}