using ITW.Fahrzeugmanagement.Application.Fahrtenbuch;
using ITW.Fahrzeugmanagement.Application.FahrzeugDokumente;
using ITW.Fahrzeugmanagement.Application.Fahrzeuge;
using ITW.Fahrzeugmanagement.Application.FahrzeugPruefungen;
using ITW.Fahrzeugmanagement.Application.Tracking;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.DependencyInjection;

public static class WebFahrzeugmanagementServiceRegistrationExtensions
{
    public static IServiceCollection AddWebFahrzeugmanagementServices(this IServiceCollection services)
    {
        services.AddScoped<ReadFahrzeugUebersichtService>();
        services.AddScoped<CreateFahrzeugService>();
        services.AddScoped<ReadFahrzeugDetailService>();
        services.AddScoped<UpdateFahrzeugStammdatenService>();

        services.AddScoped<ReadFahrzeugDokumenteService>();
        services.AddScoped<UploadFahrzeugDokumentService>();
        services.AddScoped<DownloadFahrzeugDokumentService>();
        services.AddScoped<DeleteFahrzeugDokumentService>();

        services.AddScoped<ReadFahrtenbuchService>();
        services.AddScoped<ReadFahrtenbuchEintragDetailService>();
        services.AddScoped<CreateFahrtenbuchEintragService>();

        services.AddScoped<ReadFahrzeugPruefstatusService>();
        services.AddScoped<SaveFahrzeugPruefungService>();

        services.AddScoped<SaveLocationUpdateService>();
        services.AddScoped<RegisterTrackingGeraetService>();
        services.AddScoped<CreateTrackingGeraetSetupCodeService>();
        services.AddScoped<CompleteTrackingGeraetSetupService>();
        services.AddScoped<ReadTabletLiveStandortOverviewService>();

        return services;
    }
}