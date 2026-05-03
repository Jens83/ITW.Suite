using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Qualifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Personnel;

public sealed class ItwMitarbeiterQualifikationConfiguration : IEntityTypeConfiguration<ItwMitarbeiterQualifikation>
{
    public void Configure(EntityTypeBuilder<ItwMitarbeiterQualifikation> builder)
    {
        builder.ToTable("ItwMitarbeiterQualifikationen");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QualifikationId)
            .IsRequired();

        builder.Property(x => x.IstHauptqualifikation)
            .IsRequired();

        builder.Property(x => x.ZugewiesenAm)
            .IsRequired();

        builder.HasIndex(x => new { x.ItwMitarbeiterprofilId, x.QualifikationId })
            .IsUnique()
            .HasDatabaseName("IX_ItwMitarbeiterQualifikationen_Profil_Qualifikation");

        builder.HasOne<ItwQualifikation>()
            .WithMany()
            .HasForeignKey(x => x.QualifikationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}