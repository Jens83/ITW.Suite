using ITW.Lagermanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations;

internal sealed class ArtikelChargeConfiguration : IEntityTypeConfiguration<ArtikelCharge>
{
    public void Configure(EntityTypeBuilder<ArtikelCharge> builder)
    {
        builder.ToTable("ArtikelCharge", "Lager");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Lagerort).IsRequired();
        builder.Property(c => c.Menge).IsRequired();
        builder.Property(c => c.Ablaufdatum).IsRequired();

        builder.Property(c => c.ChargeNummer)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty);

        builder.Property(c => c.EingebuchtVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(c => c.AusgebuchtVonUserId)
            .HasMaxLength(450);

        builder.HasIndex(c => new { c.ArtikelId, c.Ablaufdatum })
            .HasDatabaseName("IX_Lager_ArtikelCharge_Artikel_Ablauf")
            .HasFilter("[IstAusgebucht] = 0");
    }
}
