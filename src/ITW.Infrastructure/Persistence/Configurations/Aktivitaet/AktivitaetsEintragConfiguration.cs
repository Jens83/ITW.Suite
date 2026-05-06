using ITW.Application.Aktivitaet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Aktivitaet;

public sealed class AktivitaetsEintragConfiguration : IEntityTypeConfiguration<AktivitaetsEintrag>
{
    public void Configure(EntityTypeBuilder<AktivitaetsEintrag> builder)
    {
        builder.ToTable("AktivitaetsLog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Bereich)
            .IsRequired();

        builder.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Kategorie)
            .IsRequired();

        builder.Property(x => x.IconCssClass)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Zeitpunkt)
            .IsRequired();

        builder.HasIndex(x => new { x.Bereich, x.Zeitpunkt })
            .HasDatabaseName("IX_AktivitaetsLog_Bereich_Zeitpunkt");
    }
}
