using ITW.Application.Organisation.ReadAktiveModule;
using ITW.Application.Organisation.ReadModulZuweisungenUebersicht;
using ITW.Application.Organisation.SetModulZuweisungStatus;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.DependencyInjection;

public static class WebOrganisationModuleServiceRegistrationExtensions
{
    public static IServiceCollection AddWebOrganisationModuleServices(this IServiceCollection services)
    {
        services.AddScoped<ReadAktiveModuleService>();
        services.AddScoped<ReadModulZuweisungenUebersichtService>();
        services.AddScoped<SetModulZuweisungStatusService>();

        return services;
    }
}