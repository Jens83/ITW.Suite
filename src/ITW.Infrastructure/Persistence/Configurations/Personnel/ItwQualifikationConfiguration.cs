using ITW.Domain.Personnel.Qualifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Personnel;

public sealed class ItwQualifikationConfiguration : IEntityTypeConfiguration<ItwQualifikation>
{
    public void Configure(EntityTypeBuilder<ItwQualifikation> builder)
    {
        builder.ToTable("ItwQualifikationen");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Bezeichnung)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Sortierung)
            .IsRequired();

        builder.Property(x => x.IsAktiv)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_ItwQualifikationen_Code");
    }
}