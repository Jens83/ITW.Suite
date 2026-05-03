using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class FahrzeugVertragConfiguration : IEntityTypeConfiguration<FahrzeugVertrag>
{
    public void Configure(EntityTypeBuilder<FahrzeugVertrag> builder)
    {
        builder.ToTable("FahrzeugVertraege");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FahrzeugId)
            .IsRequired();

        builder.Property(x => x.Anbieter)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Vertragsnummer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.GueltigVon)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.GueltigBis)
            .HasColumnType("date");

        builder.Property(x => x.BetragProPeriode)
            .HasColumnType("decimal(12,2)");

        builder.Property(x => x.Notiz)
            .HasMaxLength(1000);

        builder.Property(x => x.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.FahrzeugId, x.VertragTyp, x.GueltigBis })
            .HasDatabaseName("IX_FahrzeugVertraege_Fahrzeug_VertragTyp_GueltigBis");
    }
}