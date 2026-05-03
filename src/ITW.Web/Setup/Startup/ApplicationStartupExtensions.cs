using ITW.Infrastructure.Persistence.DbContexts;
using ITW.Infrastructure.Persistence.Schema;
using ITW.Web.Setup.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ITW.Web.Setup.Startup;

public static class ApplicationStartupExtensions
{
    public static async Task InitializeApplicationAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        dbContext.Database.EnsureCreated();

        var organisationSchemaBootstrapper = scope.ServiceProvider.GetRequiredService<OrganisationSchemaBootstrapper>();
        await organisationSchemaBootstrapper.EnsureOrganisationSchemaAsync();

        var personnelSchemaBootstrapper = scope.ServiceProvider.GetRequiredService<PersonnelSchemaBootstrapper>();
        await personnelSchemaBootstrapper.EnsurePersonnelSchemaAsync();

        var personnelUrlaubSchemaBootstrapper = scope.ServiceProvider.GetRequiredService<PersonnelUrlaubSchemaBootstrapper>();
        await personnelUrlaubSchemaBootstrapper.EnsurePersonnelUrlaubSchemaAsync();

        var dienstplanSchemaBootstrapper = scope.ServiceProvider.GetRequiredService<DienstplanSchemaBootstrapper>();
        await dienstplanSchemaBootstrapper.EnsureDienstplanSchemaAsync();

        var fahrzeugmanagementSchemaBootstrapper = scope.ServiceProvider.GetRequiredService<FahrzeugmanagementSchemaBootstrapper>();
        await fahrzeugmanagementSchemaBootstrapper.EnsureFahrzeugmanagementSchemaAsync();

        var bootstrapper = scope.ServiceProvider.GetRequiredService<InitialIdentityBootstrapper>();
        await bootstrapper.ExecuteAsync();
    }
}