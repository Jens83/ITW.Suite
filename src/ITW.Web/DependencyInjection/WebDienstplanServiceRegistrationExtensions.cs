using ITW.Dienstplan.Application.Auswertung;
using ITW.Dienstplan.Application.Kalender;
using ITW.Dienstplan.Application.Perioden;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Application.Wunschphase;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Read;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Write;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.DependencyInjection;

public static class WebDienstplanServiceRegistrationExtensions
{
    public static IServiceCollection AddWebDienstplanServices(this IServiceCollection services)
    {
        services.AddScoped<CreateDienstplanPeriodeService>();
        services.AddScoped<ReadDienstplanperiodenService>();

        services.AddScoped<ReadWachleiterKalenderService>();
        services.AddScoped<ReadWachleiterPlanungsModalService>();
        services.AddScoped<ReadAutomatischePlanungsvorschauService>();
        services.AddScoped<ReadAutoplanVertreterPraeferenzenService>();
        services.AddScoped<ReadAutoplanVertreterPraeferenzScoreService>();
        services.AddScoped<ReadAutoplanAllgemeinerVertreterPraeferenzScoreService>();

        services.AddScoped<ReadAktiveWunschphaseService>();
        services.AddScoped<SetWunschphaseStatusService>();
        services.AddScoped<SetPlanfreigabeStatusService>();
        services.AddScoped<ToggleDienstwunschService>();
        services.AddScoped<SaveFreelancerMonatswunschService>();

        services.AddScoped<SaveGeplanterDienstTagService>();
        services.AddScoped<SaveDienstbesetzungsAusfallService>();

        services.AddScoped<ReadDienstplanMonatsauswertungService>();

        services.AddScoped<ReadMonatsauswertungViewModelService>();
        services.AddScoped<ReadSichtbarerDienstplanViewModelService>();
        services.AddScoped<ReadDienstplanIndexViewModelService>();

        services.AddScoped<SaveWachleiterTagesplanungService>();
        services.AddScoped<SaveWachleiterAusfallService>();
        services.AddScoped<ToggleMitarbeiterWunschService>();
        services.AddScoped<SaveFreelancerMonatswunschMitarbeiterService>();

        return services;
    }
}