using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class FahrzeugConfiguration : IEntityTypeConfiguration<Fahrzeug>
{
    public void Configure(EntityTypeBuilder<Fahrzeug> builder)
    {
        builder.ToTable("Fahrzeuge");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InterneNummer)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Kennzeichen)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Vin)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Hersteller)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Modell)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Fahrzeugtyp)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Erstzulassung)
            .HasColumnType("date");

        builder.Property(x => x.StandardStandort)
            .HasMaxLength(200);

        builder.Property(x => x.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AktualisiertVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => x.Kennzeichen)
            .IsUnique()
            .HasDatabaseName("UX_Fahrzeuge_Kennzeichen");

        builder.HasIndex(x => x.Vin)
            .IsUnique()
            .HasDatabaseName("UX_Fahrzeuge_Vin");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_Fahrzeuge_Status");
    }
}