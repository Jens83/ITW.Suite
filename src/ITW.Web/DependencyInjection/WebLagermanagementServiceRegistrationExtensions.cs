using ITW.Lagermanagement.Application.Artikel;
using ITW.Lagermanagement.Application.Bestand;
using ITW.Lagermanagement.Application.Einsatz;
using ITW.Lagermanagement.Application.Sauerstoff;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.DependencyInjection;

public static class WebLagermanagementServiceRegistrationExtensions
{
    public static IServiceCollection AddWebLagermanagementServices(this IServiceCollection services)
    {
        services.AddScoped<CreateLagerArtikelService>();
        services.AddScoped<UpdateLagerArtikelService>();
        services.AddScoped<EinbuchenArtikelService>();
        services.AddScoped<AusbuchenArtikelService>();
        services.AddScoped<BewegeSauerstoffFlascheService>();
        services.AddScoped<ErfasseEinsatzVerbrauchService>();

        return services;
    }
}
