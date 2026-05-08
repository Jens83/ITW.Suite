using ITW.Application.Aktivitaet;
using ITW.Application.Aufgaben;
using ITW.Dienstplan.Domain.Entities;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Entities;
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

    // Lagermanagement
    public DbSet<LagerArtikel>             LagerArtikel              => Set<LagerArtikel>();
    public DbSet<ArtikelBestand>           ArtikelBestaende          => Set<ArtikelBestand>();
    public DbSet<ArtikelCharge>            ArtikelChargen            => Set<ArtikelCharge>();
    public DbSet<EinsatzVerbrauch>         EinsatzVerbräuche         => Set<EinsatzVerbrauch>();
    public DbSet<EinsatzVerbrauchPosition> EinsatzVerbrauchPositionen => Set<EinsatzVerbrauchPosition>();
    public DbSet<SauerstoffLieferung>      SauerstoffLieferungen     => Set<SauerstoffLieferung>();
    public DbSet<SauerstoffFlasche>        SauerstoffFlaschen        => Set<SauerstoffFlasche>();
    public DbSet<SauerstoffBewegung>       SauerstoffBewegungen      => Set<SauerstoffBewegung>();

    public DbSet<AktivitaetsEintrag> AktivitaetsLog => Set<AktivitaetsEintrag>();

    public DbSet<Aufgabe> Aufgaben => Set<Aufgabe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
    }
}