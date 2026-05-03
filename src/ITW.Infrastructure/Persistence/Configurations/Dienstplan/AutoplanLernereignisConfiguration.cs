using ITW.Dienstplan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Dienstplan;

public sealed class AutoplanLernereignisConfiguration : IEntityTypeConfiguration<AutoplanLernereignis>
{
    public void Configure(EntityTypeBuilder<AutoplanLernereignis> builder)
    {
        builder.ToTable("AutoplanLernereignisse");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DienstplanPeriodeId)
            .IsRequired();

        builder.Property(x => x.DienstDatum)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.BesetzungsSlotCode)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.EreignisTypCode)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.VorherigeUserId)
            .HasMaxLength(450);

        builder.Property(x => x.NeueUserId)
            .HasMaxLength(450);

        builder.Property(x => x.UrspruenglichGeplanterUserId)
            .HasMaxLength(450);

        builder.Property(x => x.KontextArztUserId)
            .HasMaxLength(450);

        builder.Property(x => x.KontextNotfallsanitaeter1UserId)
            .HasMaxLength(450);

        builder.Property(x => x.KontextNotfallsanitaeter2UserId)
            .HasMaxLength(450);

        builder.Property(x => x.BearbeitetVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.ErfasstAm)
            .IsRequired();

        builder.HasIndex(x => new { x.DienstplanPeriodeId, x.DienstDatum })
            .HasDatabaseName("IX_AutoplanLernereignisse_Periode_Datum");

        builder.HasIndex(x => x.ErfasstAm)
            .HasDatabaseName("IX_AutoplanLernereignisse_ErfasstAm");
    }
}