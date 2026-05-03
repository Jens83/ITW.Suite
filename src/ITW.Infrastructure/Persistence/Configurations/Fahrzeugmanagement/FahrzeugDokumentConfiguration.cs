using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class FahrzeugDokumentConfiguration : IEntityTypeConfiguration<FahrzeugDokument>
{
    public void Configure(EntityTypeBuilder<FahrzeugDokument> builder)
    {
        builder.ToTable("FahrzeugDokumente");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FahrzeugId)
            .IsRequired();

        builder.Property(x => x.Dateiname)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Speicherpfad)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.GueltigBis)
            .HasColumnType("date");

        builder.Property(x => x.HochgeladenVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Bezeichnung)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => new { x.FahrzeugId, x.Kategorie })
            .HasDatabaseName("IX_FahrzeugDokumente_Fahrzeug_Kategorie");
    }
}