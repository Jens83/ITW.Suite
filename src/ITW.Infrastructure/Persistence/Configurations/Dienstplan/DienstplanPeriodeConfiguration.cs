using ITW.Dienstplan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Dienstplan;

public sealed class DienstplanPeriodeConfiguration : IEntityTypeConfiguration<DienstplanPeriode>
{
    public void Configure(EntityTypeBuilder<DienstplanPeriode> builder)
    {
        builder.ToTable("DienstplanPerioden");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Jahr)
            .IsRequired();

        builder.Property(x => x.Monat)
            .IsRequired();

        builder.Property(x => x.Bezeichnung)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.WunschphaseIstOffen)
            .IsRequired();

        builder.Property(x => x.PlanIstFreigegeben)
            .IsRequired();

        builder.Property(x => x.PlanFreigegebenAm);

        builder.Property(x => x.PlanFreigegebenVonUserId)
            .HasMaxLength(450);

        builder.Property(x => x.ErstelltAm)
            .IsRequired();

        builder.Property(x => x.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.Jahr, x.Monat })
            .IsUnique()
            .HasDatabaseName("IX_DienstplanPerioden_Jahr_Monat");

        builder.HasIndex(x => x.WunschphaseIstOffen)
            .HasDatabaseName("IX_DienstplanPerioden_WunschphaseIstOffen");

        builder.HasIndex(x => x.PlanIstFreigegeben)
            .HasDatabaseName("IX_DienstplanPerioden_PlanIstFreigegeben");
    }
}