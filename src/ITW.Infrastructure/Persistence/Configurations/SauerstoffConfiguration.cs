using ITW.Lagermanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations;

internal sealed class SauerstoffLieferungConfiguration : IEntityTypeConfiguration<SauerstoffLieferung>
{
    public void Configure(EntityTypeBuilder<SauerstoffLieferung> builder)
    {
        builder.ToTable("SauerstoffLieferung", "Lager");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.LieferscheinNummer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Lieferdatum).IsRequired();

        builder.Property(l => l.Bemerkung).HasMaxLength(500);

        builder.Property(l => l.ErfasstVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(l => l.LieferscheinNummer)
            .HasDatabaseName("UX_Lager_SauerstoffLieferung_Schein")
            .IsUnique();
    }
}

internal sealed class SauerstoffFlascheConfiguration : IEntityTypeConfiguration<SauerstoffFlasche>
{
    public void Configure(EntityTypeBuilder<SauerstoffFlasche> builder)
    {
        builder.ToTable("SauerstoffFlasche", "Lager");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Groesse).IsRequired();
        builder.Property(f => f.Status).IsRequired();

        builder.Property(f => f.FlaschenNummer).HasMaxLength(100);

        builder.Property(f => f.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne<SauerstoffLieferung>()
            .WithMany()
            .HasForeignKey(f => f.LieferungId)
            .HasConstraintName("FK_Lager_SauerstoffFlasche_Lieferung")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.FlaschenNummer)
            .HasDatabaseName("UX_Lager_SauerstoffFlasche_FlaschenNummer")
            .IsUnique()
            .HasFilter("[FlaschenNummer] IS NOT NULL");

        builder.HasIndex(f => new { f.Status, f.FahrzeugId })
            .HasDatabaseName("IX_Lager_SauerstoffFlasche_Status_Fahrzeug")
            .HasFilter("[IstAktiv] = 1");
    }
}

internal sealed class SauerstoffBewegungConfiguration : IEntityTypeConfiguration<SauerstoffBewegung>
{
    public void Configure(EntityTypeBuilder<SauerstoffBewegung> builder)
    {
        builder.ToTable("SauerstoffBewegung", "Lager");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Typ).IsRequired();
        builder.Property(b => b.Datum).IsRequired();

        builder.Property(b => b.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(b => b.Bemerkung).HasMaxLength(500);

        builder.HasOne<SauerstoffFlasche>()
            .WithMany()
            .HasForeignKey(b => b.FlascheId)
            .HasConstraintName("FK_Lager_SauerstoffBewegung_Flasche")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.FlascheId, b.Datum })
            .HasDatabaseName("IX_Lager_SauerstoffBewegung_Flasche_Datum");
    }
}
