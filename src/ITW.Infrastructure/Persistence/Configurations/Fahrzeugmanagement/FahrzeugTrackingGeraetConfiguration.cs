using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class FahrzeugTrackingGeraetConfiguration : IEntityTypeConfiguration<FahrzeugTrackingGeraet>
{
    public void Configure(EntityTypeBuilder<FahrzeugTrackingGeraet> builder)
    {
        builder.ToTable("FahrzeugTrackingGeraete");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeviceIdentifier)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.ApiKeyHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(x => x.DeviceIdentifier)
            .IsUnique()
            .HasDatabaseName("UX_FahrzeugTrackingGeraete_DeviceIdentifier");
    }
}