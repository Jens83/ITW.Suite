using ITW.Dienstplan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Dienstplan;

public sealed class DienstwunschConfiguration : IEntityTypeConfiguration<Dienstwunsch>
{
    public void Configure(EntityTypeBuilder<Dienstwunsch> builder)
    {
        builder.ToTable("DienstplanWuensche");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DienstplanPeriodeId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.WunschDatum)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.WunschTyp)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ErstelltAm)
            .IsRequired();

        builder.HasIndex(x => new { x.DienstplanPeriodeId, x.UserId, x.WunschDatum })
            .IsUnique()
            .HasDatabaseName("IX_DienstplanWuensche_Periode_User_Datum");
    }
}