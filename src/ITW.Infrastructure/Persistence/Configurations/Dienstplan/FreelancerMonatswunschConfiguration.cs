using ITW.Dienstplan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITW.Infrastructure.Persistence.Configurations.Dienstplan;

public sealed class FreelancerMonatswunschConfiguration : IEntityTypeConfiguration<FreelancerMonatswunsch>
{
    public void Configure(EntityTypeBuilder<FreelancerMonatswunsch> builder)
    {
        builder.ToTable("FreelancerMonatswuensche");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DienstplanPeriodeId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.GewuenschteDienste)
            .IsRequired();

        builder.Property(x => x.ErstelltAm)
            .IsRequired();

        builder.Property(x => x.AktualisiertAm)
            .IsRequired();

        builder.HasIndex(x => new { x.DienstplanPeriodeId, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_FreelancerMonatswuensche_Periode_User");
    }
}