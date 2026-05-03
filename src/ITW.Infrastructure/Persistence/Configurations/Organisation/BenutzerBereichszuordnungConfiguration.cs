using ITW.Domain.Organisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Organisation;

public sealed class BenutzerBereichszuordnungConfiguration : IEntityTypeConfiguration<BenutzerBereichszuordnung>
{
    public void Configure(EntityTypeBuilder<BenutzerBereichszuordnung> builder)
    {
        builder.ToTable("BenutzerBereichszuordnungen");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Bereich)
            .IsRequired();

        builder.Property(x => x.Rolle)
            .IsRequired();

        builder.Property(x => x.Fuehrungsverantwortung)
            .IsRequired();

        builder.Property(x => x.IsPrimary)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.ZugewiesenAm)
            .IsRequired();

        builder.Property(x => x.DeaktiviertAm)
            .IsRequired(false);

        builder.HasIndex(x => new { x.UserId, x.IsActive, x.IsPrimary })
            .HasDatabaseName("IX_BenutzerBereichszuordnungen_User_Aktiv_Primaer");

        builder.HasIndex(x => new { x.Bereich, x.IsActive, x.IsPrimary })
            .HasDatabaseName("IX_BenutzerBereichszuordnungen_Bereich_Aktiv_Primaer");
    }
}