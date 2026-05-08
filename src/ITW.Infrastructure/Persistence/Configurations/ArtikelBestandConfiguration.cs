using ITW.Lagermanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations;

internal sealed class ArtikelBestandConfiguration : IEntityTypeConfiguration<ArtikelBestand>
{
    public void Configure(EntityTypeBuilder<ArtikelBestand> builder)
    {
        builder.ToTable("ArtikelBestand", "Lager");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Lagerort).IsRequired();
        builder.Property(b => b.Menge).IsRequired();

        builder.HasIndex(b => new { b.ArtikelId, b.Lagerort })
            .IsUnique()
            .HasDatabaseName("UX_Lager_ArtikelBestand_Artikel_Lagerort");
    }
}
