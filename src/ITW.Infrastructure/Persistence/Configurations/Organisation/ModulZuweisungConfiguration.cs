using ITW.Domain.Organisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Organisation;

public sealed class ModulZuweisungConfiguration : IEntityTypeConfiguration<ModulZuweisung>
{
    public void Configure(EntityTypeBuilder<ModulZuweisung> builder)
    {
        builder.ToTable("ModulZuweisungen");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Modul)
            .IsRequired();

        builder.Property(x => x.Bereich)
            .IsRequired();

        builder.Property(x => x.Rolle)
            .IsRequired();

        builder.Property(x => x.IstAktiv)
            .IsRequired();

        builder.Property(x => x.ZugewiesenAm)
            .IsRequired();

        builder.Property(x => x.ZugewiesenVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.DeaktiviertAm)
            .IsRequired(false);

        builder.Property(x => x.DeaktiviertVonUserId)
            .HasMaxLength(450)
            .IsRequired(false);

        builder.HasIndex(x => new { x.Modul, x.Bereich, x.Rolle })
            .IsUnique()
            .HasDatabaseName("IX_ModulZuweisungen_Modul_Bereich_Rolle");

        builder.HasIndex(x => new { x.Bereich, x.Rolle, x.IstAktiv })
            .HasDatabaseName("IX_ModulZuweisungen_Bereich_Rolle_Aktiv");
    }
}