using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class FahrzeugFahrerzuordnungConfiguration : IEntityTypeConfiguration<FahrzeugFahrerzuordnung>
{
    public void Configure(EntityTypeBuilder<FahrzeugFahrerzuordnung> builder)
    {
        builder.ToTable("FahrzeugFahrerzuordnungen");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FahrzeugId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.ZuordnungTyp)
            .IsRequired();

        builder.Property(x => x.IstPrimaer)
            .IsRequired();

        builder.Property(x => x.GueltigVon)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.GueltigBis)
            .HasColumnType("date");

        builder.Property(x => x.Bemerkung)
            .HasMaxLength(500);

        builder.Property(x => x.ErstelltAm)
            .IsRequired();

        builder.Property(x => x.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.FahrzeugId, x.UserId, x.GueltigVon })
            .HasDatabaseName("IX_FahrzeugFahrerzuordnungen_Fahrzeug_User_GueltigVon");
    }
}
