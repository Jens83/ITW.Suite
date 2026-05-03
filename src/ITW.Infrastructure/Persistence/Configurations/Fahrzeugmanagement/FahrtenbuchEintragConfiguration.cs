using ITW.Fahrzeugmanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Fahrzeugmanagement;

public sealed class FahrtenbuchEintragConfiguration : IEntityTypeConfiguration<FahrtenbuchEintrag>
{
    public void Configure(EntityTypeBuilder<FahrtenbuchEintrag> builder)
    {
        builder.ToTable("FahrtenbuchEintraege");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FahrzeugId)
            .IsRequired();

        builder.Property(x => x.FahrerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.BeifahrerName)
            .HasMaxLength(200);

        builder.Property(x => x.FahrerUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Fahrtzweck)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Startort)
            .HasMaxLength(300);

        builder.Property(x => x.Zielort)
            .HasMaxLength(300);

        builder.Property(x => x.Bemerkung)
            .HasMaxLength(1000);

        builder.Property(x => x.TankmengeLiter)
            .HasPrecision(8, 2);

        builder.Property(x => x.ErstelltVonUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AktualisiertVonUserId)
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.FahrzeugId, x.StartzeitUtc })
            .HasDatabaseName("IX_FahrtenbuchEintraege_Fahrzeug_StartzeitUtc");

        builder.HasIndex(x => new { x.FahrerUserId, x.StartzeitUtc })
            .HasDatabaseName("IX_FahrtenbuchEintraege_Fahrer_StartzeitUtc");

        builder.HasIndex(x => x.RouteSessionId)
            .HasDatabaseName("IX_FahrtenbuchEintraege_RouteSessionId");

        builder.HasIndex(x => x.EinsatzId)
            .HasDatabaseName("IX_FahrtenbuchEintraege_EinsatzId");
    }
}