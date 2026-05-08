using ITW.Lagermanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations;

internal sealed class EinsatzVerbrauchConfiguration : IEntityTypeConfiguration<EinsatzVerbrauch>
{
    public void Configure(EntityTypeBuilder<EinsatzVerbrauch> builder)
    {
        builder.ToTable("EinsatzVerbrauch", "Lager");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Datum).IsRequired();
        builder.Property(v => v.Fahrzeug).IsRequired();
        builder.Property(v => v.Patienten).IsRequired();

        builder.Property(v => v.Bemerkung).HasMaxLength(500);

        builder.Property(v => v.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        // Backing field für die private _positionen-Liste
        builder.Navigation(v => v.Positionen)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(v => v.Positionen)
            .WithOne()
            .HasForeignKey(p => p.EinsatzVerbrauchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class EinsatzVerbrauchPositionConfiguration : IEntityTypeConfiguration<EinsatzVerbrauchPosition>
{
    public void Configure(EntityTypeBuilder<EinsatzVerbrauchPosition> builder)
    {
        builder.ToTable("EinsatzVerbrauchPosition", "Lager");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Menge).IsRequired();
        builder.Property(p => p.IstProPatientBerechnet).IsRequired();

        builder.HasIndex(p => p.EinsatzVerbrauchId)
            .HasDatabaseName("IX_Lager_EinsatzVerbrauchPosition_Verbrauch");
    }
}
