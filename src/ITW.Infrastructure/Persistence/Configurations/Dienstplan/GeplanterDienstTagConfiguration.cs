using ITW.Dienstplan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Dienstplan;

public sealed class GeplanterDienstTagConfiguration : IEntityTypeConfiguration<GeplanterDienstTag>
{
    public void Configure(EntityTypeBuilder<GeplanterDienstTag> builder)
    {
        builder.ToTable("GeplanteDiensttage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DienstplanPeriodeId)
            .IsRequired();

        builder.Property(x => x.DienstDatum)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.ArztUserId)
            .HasMaxLength(450);

        builder.Property(x => x.Notfallsanitaeter1UserId)
            .HasMaxLength(450);

        builder.Property(x => x.Notfallsanitaeter2UserId)
            .HasMaxLength(450);

        builder.Property(x => x.AktualisiertVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AktualisiertAm)
            .IsRequired();

        builder.HasIndex(x => new { x.DienstplanPeriodeId, x.DienstDatum })
            .IsUnique()
            .HasDatabaseName("IX_GeplanteDiensttage_Periode_Datum");
    }
}