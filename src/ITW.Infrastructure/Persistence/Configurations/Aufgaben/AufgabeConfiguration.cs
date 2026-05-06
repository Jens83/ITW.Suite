using ITW.Application.Aufgaben;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Aufgaben;

public sealed class AufgabeConfiguration : IEntityTypeConfiguration<Aufgabe>
{
    public void Configure(EntityTypeBuilder<Aufgabe> builder)
    {
        builder.ToTable("Aufgaben");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Bereich)
            .IsRequired();

        builder.Property(x => x.Titel)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Prioritaet)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Quelle)
            .IsRequired();

        builder.Property(x => x.SystemSchluessel)
            .HasMaxLength(200);

        builder.Property(x => x.Faelligkeitsdatum);

        builder.Property(x => x.ErstelltAm)
            .IsRequired();

        builder.Property(x => x.ErledigtAm);

        builder.HasIndex(x => new { x.Bereich, x.Status })
            .HasDatabaseName("IX_Aufgaben_Bereich_Status");

        builder.HasIndex(x => x.SystemSchluessel)
            .HasDatabaseName("IX_Aufgaben_SystemSchluessel");
    }
}
