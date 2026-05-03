// Datei: src/ITW.Infrastructure/Persistence/Configurations/Security/PasswortResetAnfrageConfiguration.cs
using ITW.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Security;

public sealed class PasswortResetAnfrageConfiguration : IEntityTypeConfiguration<PasswortResetAnfrage>
{
    public void Configure(EntityTypeBuilder<PasswortResetAnfrage> builder)
    {
        builder.ToTable("PasswortResetAnfragen");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Benutzername)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Vorname)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Nachname)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Bereich)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.AngefordertAm)
            .IsRequired();

        builder.Property(x => x.BearbeitetVonUserId)
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("IX_PasswortResetAnfragen_User_Status");

        builder.HasIndex(x => new { x.Bereich, x.Status })
            .HasDatabaseName("IX_PasswortResetAnfragen_Bereich_Status");
    }
}