using ITW.Lagermanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations;

internal sealed class LagerArtikelConfiguration : IEntityTypeConfiguration<LagerArtikel>
{
    public void Configure(EntityTypeBuilder<LagerArtikel> builder)
    {
        builder.ToTable("Artikel", "Lager");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Kategorie)
            .IsRequired();

        builder.Property(a => a.BasisEinheit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.PackungsEinheit)
            .HasMaxLength(50);

        builder.Property(a => a.VerbrauchProPatient)
            .HasPrecision(10, 4);

        builder.Property(a => a.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.AktualisiertVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(a => a.Name)
            .IsUnique()
            .HasDatabaseName("UX_Lager_Artikel_Name");
    }
}
