using ITW.Application.Personnel.Documents;
using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Profiles;
using ITW.Application.Personnel.Urlaub;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.DependencyInjection;

public static class WebPersonnelServiceRegistrationExtensions
{
    public static IServiceCollection AddWebPersonnelServices(this IServiceCollection services)
    {
        services.AddScoped<ReadItwMitarbeiterprofileService>();
        services.AddScoped<ReadItwMitarbeiterprofilDetailService>();
        services.AddScoped<ReadItwMitarbeiterDetailUebersichtService>();
        services.AddScoped<ReadAllgemeinesMitarbeiterprofilDetailService>();
        services.AddScoped<SaveItwMitarbeiterprofilService>();
        services.AddScoped<SaveAllgemeinesMitarbeiterprofilService>();

        services.AddScoped<SaveMitarbeiterUrlaubsanspruchService>();
        services.AddScoped<SaveMitarbeiterUrlaubszeitraumService>();
        services.AddScoped<DeleteMitarbeiterUrlaubszeitraumService>();
        services.AddScoped<ReadMitarbeiterUrlaubsplanerService>();

        services.AddScoped<ReadMitarbeiterDokumenteService>();
        services.AddScoped<UploadMitarbeiterDokumentService>();
        services.AddScoped<DownloadMitarbeiterDokumentService>();

        return services;
    }
}