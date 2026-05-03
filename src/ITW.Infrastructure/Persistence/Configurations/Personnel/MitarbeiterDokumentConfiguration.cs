// Datei: src/ITW.Infrastructure/Persistence/Configurations/Personnel/MitarbeiterDokumentConfiguration.cs
using ITW.Domain.Personnel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Personnel;

public sealed class MitarbeiterDokumentConfiguration : IEntityTypeConfiguration<MitarbeiterDokument>
{
    public void Configure(EntityTypeBuilder<MitarbeiterDokument> builder)
    {
        builder.ToTable("MitarbeiterDokumente");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Kategorie)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DateinameOriginal)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.Speicherpfad)
            .IsRequired()
            .HasMaxLength(600);

        builder.Property(x => x.Inhaltstyp)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.DateigroesseBytes)
            .IsRequired();

        builder.Property(x => x.HochgeladenAm)
            .IsRequired();

        builder.Property(x => x.HochgeladenVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_MitarbeiterDokumente_UserId");

        builder.HasIndex(x => new { x.UserId, x.HochgeladenAm })
            .HasDatabaseName("IX_MitarbeiterDokumente_UserId_HochgeladenAm");
    }
}