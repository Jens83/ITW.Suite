using ITW.Dienstplan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Dienstplan;

public sealed class GeplanterDienstTagAusfallConfiguration : IEntityTypeConfiguration<GeplanterDienstTagAusfall>
{
    public void Configure(EntityTypeBuilder<GeplanterDienstTagAusfall> builder)
    {
        builder.ToTable("GeplanteDiensttagAusfaelle");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DienstplanPeriodeId)
            .IsRequired();

        builder.Property(x => x.DienstDatum)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.BesetzungsSlotCode)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.UrspruenglichGeplanterUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AusfallGrundCode)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.VertretungsUserId)
            .HasMaxLength(450);

        builder.Property(x => x.ErfasstVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.ErfasstAm)
            .IsRequired();

        builder.HasIndex(x => new { x.DienstplanPeriodeId, x.DienstDatum, x.BesetzungsSlotCode })
            .IsUnique()
            .HasDatabaseName("IX_GeplanteDiensttagAusfaelle_Periode_Datum_Slot");
    }
}