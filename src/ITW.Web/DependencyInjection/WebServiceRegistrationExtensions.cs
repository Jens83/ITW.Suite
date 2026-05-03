using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.DependencyInjection;

public static class WebServiceRegistrationExtensions
{
    public static IServiceCollection AddWebApplicationServices(this IServiceCollection services)
    {
        services
            .AddWebUserAndOrganisationServices()
            .AddWebPersonnelServices()
            .AddWebDienstplanServices()
            .AddWebOrganisationModuleServices()
            .AddWebCoreServices()
            .AddWebFahrzeugmanagementServices();

        return services;
    }
}