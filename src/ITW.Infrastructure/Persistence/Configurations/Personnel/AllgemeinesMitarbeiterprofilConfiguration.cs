using ITW.Domain.Personnel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Personnel;

public sealed class AllgemeinesMitarbeiterprofilConfiguration : IEntityTypeConfiguration<AllgemeinesMitarbeiterprofil>
{
    public void Configure(EntityTypeBuilder<AllgemeinesMitarbeiterprofil> builder)
    {
        builder.ToTable("AllgemeineMitarbeiterprofile");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Vorname)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Nachname)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200);

        builder.Property(x => x.Beschaeftigungsart)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Telefonnummer)
            .HasMaxLength(50);

        builder.Property(x => x.Strasse)
            .HasMaxLength(200);

        builder.Property(x => x.Hausnummer)
            .HasMaxLength(20);

        builder.Property(x => x.Postleitzahl)
            .HasMaxLength(20);

        builder.Property(x => x.Ort)
            .HasMaxLength(100);

        builder.Property(x => x.ErstelltAm)
            .IsRequired();

        builder.Property(x => x.AktualisiertAm)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("IX_AllgemeineMitarbeiterprofile_UserId");
    }
}