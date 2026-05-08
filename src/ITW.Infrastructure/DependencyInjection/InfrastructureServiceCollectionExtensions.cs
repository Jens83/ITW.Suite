using ITW.Application.Abstractions.DateTime;
using ITW.Application.Aktivitaet;
using ITW.Application.Aufgaben;
using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Lagermanagement.Application.Contracts;
using ITW.Infrastructure.Identity.UserManagement;
using ITW.Infrastructure.Persistence.DbContexts;
using ITW.Infrastructure.Persistence.Repositories;
using ITW.Infrastructure.Persistence.Schema;
using ITW.Infrastructure.Services.DateTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<PlatformDbContext>(configureDbContext);

        services.AddScoped<IBenutzerBereichszuordnungRepository, BenutzerBereichszuordnungRepository>();
        services.AddScoped<IModulZuweisungRepository, ModulZuweisungRepository>();
        services.AddScoped<IBenutzerkontoRepository, BenutzerkontoRepository>();

        services.AddScoped<IPasswortResetBenutzerLookupRepository, PasswortResetBenutzerLookupRepository>();
        services.AddScoped<IPasswortResetAnfrageRepository, PasswortResetAnfrageRepository>();

        services.AddScoped<IItwMitarbeiterprofilRepository, ItwMitarbeiterprofilRepository>();
        services.AddScoped<IAllgemeinesMitarbeiterprofilRepository, AllgemeinesMitarbeiterprofilRepository>();
        services.AddScoped<IMitarbeiterDokumentRepository, MitarbeiterDokumentRepository>();

        services.AddScoped<IMitarbeiterUrlaubsanspruchRepository, MitarbeiterUrlaubsanspruchRepository>();
        services.AddScoped<IMitarbeiterUrlaubszeitraumRepository, MitarbeiterUrlaubszeitraumRepository>();

        services.AddScoped<IDienstplanPeriodeRepository, DienstplanPeriodeRepository>();
        services.AddScoped<IDienstwunschRepository, DienstwunschRepository>();
        services.AddScoped<IFreelancerMonatswunschRepository, FreelancerMonatswunschRepository>();
        services.AddScoped<IDienstwunschAuswertungRepository, DienstwunschAuswertungRepository>();
        services.AddScoped<IGeplanterDienstTagRepository, GeplanterDienstTagRepository>();
        services.AddScoped<IDienstbesetzungsAusfallRepository, DienstbesetzungsAusfallRepository>();
        services.AddScoped<IDienstplanUrlaubszeitraumRepository, DienstplanUrlaubszeitraumRepository>();
        services.AddScoped<IDienstplanMitarbeiterPlanungsRepository, DienstplanMitarbeiterPlanungsRepository>();
        services.AddScoped<IAutoplanLernereignisRepository, AutoplanLernereignisRepository>();
        services.AddScoped<IFahrzeugTrackingRepository, FahrzeugTrackingRepository>();
        services.AddScoped<IFahrzeugRepository, FahrzeugRepository>();
        services.AddScoped<IFahrzeugDokumentRepository, FahrzeugDokumentRepository>();
        services.AddSingleton<IFahrzeugDokumentDateiSpeicher, FahrzeugDokumentDateiSpeicher>();
        services.AddScoped<IFahrzeugVertragRepository, FahrzeugVertragRepository>();
        services.AddScoped<IFahrtenbuchRepository, FahrtenbuchRepository>();
        services.AddScoped<IFahrzeugPruefungRepository, FahrzeugPruefungRepository>();

        // Lagermanagement
        services.AddScoped<ILagerArtikelRepository, LagerArtikelRepository>();
        services.AddScoped<IArtikelBestandRepository, ArtikelBestandRepository>();
        services.AddScoped<IArtikelChargeRepository, ArtikelChargeRepository>();
        services.AddScoped<ISauerstoffLieferungRepository, SauerstoffLieferungRepository>();
        services.AddScoped<ISauerstoffFlascheRepository, SauerstoffFlascheRepository>();
        services.AddScoped<IEinsatzVerbrauchRepository, EinsatzVerbrauchRepository>();

        services.AddScoped<IAktivitaetsLogRepository, AktivitaetsLogRepository>();
        services.AddScoped<IAufgabeRepository, AufgabeRepository>();

        services.AddScoped<OrganisationSchemaBootstrapper>();
        services.AddScoped<DienstplanSchemaBootstrapper>();
        services.AddScoped<PersonnelSchemaBootstrapper>();
        services.AddScoped<PersonnelUrlaubSchemaBootstrapper>();
        services.AddScoped<FahrzeugmanagementSchemaBootstrapper>();
        services.AddScoped<LagermanagementSchemaBootstrapper>();
        services.AddScoped<AktivitaetsLogSchemaBootstrapper>();
        services.AddScoped<AufgabenSchemaBootstrapper>();

        services.AddSingleton<IMitarbeiterDokumentDateiSpeicher, MitarbeiterDokumentDateiSpeicher>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}