using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class FahrzeugPruefungConfiguration : IEntityTypeConfiguration<FahrzeugPruefung>
{
    public void Configure(EntityTypeBuilder<FahrzeugPruefung> builder)
    {
        builder.ToTable("FahrzeugPruefungen");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FaelligAm)
            .HasColumnType("date");

        builder.Property(x => x.LetzteErledigungAm)
            .HasColumnType("date");

        builder.Property(x => x.Bemerkung)
            .HasMaxLength(1000);

        builder.Property(x => x.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AktualisiertVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.FahrzeugId, x.Typ })
            .IsUnique()
            .HasDatabaseName("UX_FahrzeugPruefungen_Fahrzeug_Typ");

        builder.HasIndex(x => new { x.FahrzeugId, x.FaelligAm })
            .HasDatabaseName("IX_FahrzeugPruefungen_Fahrzeug_FaelligAm");
    }
}