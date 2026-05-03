using ITW.Domain.Personnel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Personnel;

public sealed class ItwMitarbeiterprofilConfiguration : IEntityTypeConfiguration<ItwMitarbeiterprofil>
{
    public void Configure(EntityTypeBuilder<ItwMitarbeiterprofil> builder)
    {
        builder.ToTable("ItwMitarbeiterprofile");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.ErstelltAm)
            .IsRequired();

        builder.Property(x => x.AktualisiertAm)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("IX_ItwMitarbeiterprofile_UserId");

        builder.Navigation(x => x.Qualifikationen)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Qualifikationen)
            .WithOne()
            .HasForeignKey(x => x.ItwMitarbeiterprofilId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}