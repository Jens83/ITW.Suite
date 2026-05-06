using ITW.Application.Aktivitaet;
using ITW.Dienstplan.Domain.Entities;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Domain.Organisation.Entities;
using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Qualifications;
using ITW.Domain.Security.Entities;
using ITW.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.DbContexts;

public sealed class PlatformDbContext : IdentityDbContext<ApplicationUser>
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<BenutzerBereichszuordnung> BenutzerBereichszuordnungen => Set<BenutzerBereichszuordnung>();

    public DbSet<ModulZuweisung> ModulZuweisungen => Set<ModulZuweisung>();

    public DbSet<ItwMitarbeiterprofil> ItwMitarbeiterprofile => Set<ItwMitarbeiterprofil>();

    public DbSet<ItwQualifikation> ItwQualifikationen => Set<ItwQualifikation>();

    public DbSet<AllgemeinesMitarbeiterprofil> AllgemeineMitarbeiterprofile => Set<AllgemeinesMitarbeiterprofil>();

    public DbSet<PasswortResetAnfrage> PasswortResetAnfragen => Set<PasswortResetAnfrage>();

    public DbSet<DienstplanPeriode> DienstplanPerioden => Set<DienstplanPeriode>();

    public DbSet<Dienstwunsch> DienstplanWuensche => Set<Dienstwunsch>();

    public DbSet<FreelancerMonatswunsch> FreelancerMonatswuensche => Set<FreelancerMonatswunsch>();

    public DbSet<GeplanterDienstTag> GeplanteDiensttage => Set<GeplanterDienstTag>();

    public DbSet<GeplanterDienstTagAusfall> GeplanteDiensttagAusfaelle => Set<GeplanterDienstTagAusfall>();

    public DbSet<AutoplanLernereignis> AutoplanLernereignisse => Set<AutoplanLernereignis>();

    public DbSet<Fahrzeug> Fahrzeuge => Set<Fahrzeug>();

    public DbSet<FahrzeugDokument> FahrzeugDokumente => Set<FahrzeugDokument>();

    public DbSet<FahrzeugVertrag> FahrzeugVertraege => Set<FahrzeugVertrag>();

    public DbSet<FahrzeugFahrerzuordnung> FahrzeugFahrerzuordnungen => Set<FahrzeugFahrerzuordnung>();

    public DbSet<FahrzeugTrackingGeraet> FahrzeugTrackingGeraete => Set<FahrzeugTrackingGeraet>();

    public DbSet<FahrtenbuchEintrag> FahrtenbuchEintraege => Set<FahrtenbuchEintrag>();

    public DbSet<TrackingGeraetStandortAktuell> TrackingGeraetStandorteAktuell => Set<TrackingGeraetStandortAktuell>();

    public DbSet<TrackingGeraetStandortHistorienpunkt> TrackingGeraetStandortHistorie => Set<TrackingGeraetStandortHistorienpunkt>();

    public DbSet<TrackingGeraetEinrichtungscode> TrackingGeraetEinrichtungscodes => Set<TrackingGeraetEinrichtungscode>();

    public DbSet<FahrzeugPruefung> FahrzeugPruefungen => Set<FahrzeugPruefung>();

    public DbSet<AktivitaetsEintrag> AktivitaetsLog => Set<AktivitaetsEintrag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
    }
}