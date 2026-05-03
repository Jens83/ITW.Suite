using ITW.Application.Organisation.Contracts;
using ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace ITW.Web.Test;

public sealed class DienstplanControllerSplitTests
{
    [Fact]
    public async Task Index_LeitetMitarbeiterAnDienstplanMitarbeiterControllerWeiter()
    {
        // Arrange
        var controller = ErzeugeController(
            CurrentUserContextLookupResult.Erfolg(
                ErzeugeCurrentUserContext(
                    userId: "mitarbeiter-1",
                    bereich: OrganisationsbereichCode.Intensivtransport,
                    rolle: BereichsrolleCode.Mitarbeiter,
                    fuehrungsverantwortung: FuehrungsverantwortungCode.Keine,
                    aktiveModule: [ModulCode.Dienstplan])));

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("DienstplanMitarbeiter", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Index_LeitetWachleiterAnDienstplanWachleiterControllerWeiter()
    {
        // Arrange
        var controller = ErzeugeController(
            CurrentUserContextLookupResult.Erfolg(
                ErzeugeCurrentUserContext(
                    userId: "wachleiter-1",
                    bereich: OrganisationsbereichCode.Intensivtransport,
                    rolle: BereichsrolleCode.Wachleiter,
                    fuehrungsverantwortung: FuehrungsverantwortungCode.OperativeLeitung,
                    aktiveModule: [ModulCode.Dienstplan])));

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("DienstplanWachleiter", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Index_GibtKeinZugriffWennRolleNichtErlaubtIst()
    {
        // Arrange
        var controller = ErzeugeController(
            CurrentUserContextLookupResult.Erfolg(
                ErzeugeCurrentUserContext(
                    userId: "verwaltung-1",
                    bereich: OrganisationsbereichCode.Intensivtransport,
                    rolle: BereichsrolleCode.Verwaltungsmitarbeiter,
                    fuehrungsverantwortung: FuehrungsverantwortungCode.Keine,
                    aktiveModule: [ModulCode.Dienstplan])));

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("KeinZugriff", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
        Assert.Equal(string.Empty, redirectResult.RouteValues?["area"]);
    }

    [Fact]
    public async Task Index_GibtKeinZugriffWennKeinBenutzerkontextVorliegt()
    {
        // Arrange
        var controller = ErzeugeController(
            CurrentUserContextLookupResult.Fehler("Kein Benutzerkontext vorhanden."));

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("KeinZugriff", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
        Assert.Equal(string.Empty, redirectResult.RouteValues?["area"]);
    }

    private static DienstplanController ErzeugeController(
        CurrentUserContextLookupResult lookupResult)
    {
        var controller = new DienstplanController(
            new FakeCurrentUserContextAccessor(lookupResult));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    private static CurrentUserContext ErzeugeCurrentUserContext(
        string userId,
        OrganisationsbereichCode bereich,
        BereichsrolleCode rolle,
        FuehrungsverantwortungCode fuehrungsverantwortung,
        IReadOnlyCollection<ModulCode> aktiveModule)
    {
        return new CurrentUserContext(
            userId,
            bereich,
            rolle,
            fuehrungsverantwortung,
            aktiveModule);
    }
}

public sealed class ModuleAccessFilterTests
{
    [Fact]
    public async Task OnAuthorizationAsync_LaesstZugriffZu_WennModulAktivIst()
    {
        // Arrange
        var filter = new ModuleAccessFilter(
            new FakeCurrentUserContextAccessor(
                CurrentUserContextLookupResult.Erfolg(
                    new CurrentUserContext(
                        "mitarbeiter-1",
                        OrganisationsbereichCode.Intensivtransport,
                        BereichsrolleCode.Mitarbeiter,
                        FuehrungsverantwortungCode.Keine,
                        [ModulCode.Dienstplan]))),
            ModulCode.Dienstplan);

        var context = ErzeugeAuthorizationContext();

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnAuthorizationAsync_LeitetZumBereichsDashboardWeiter_WennModulFehlt()
    {
        // Arrange
        var filter = new ModuleAccessFilter(
            new FakeCurrentUserContextAccessor(
                CurrentUserContextLookupResult.Erfolg(
                    new CurrentUserContext(
                        "mitarbeiter-1",
                        OrganisationsbereichCode.Intensivtransport,
                        BereichsrolleCode.Mitarbeiter,
                        FuehrungsverantwortungCode.Keine,
                        [ModulCode.Personal]))),
            ModulCode.Dienstplan);

        var context = ErzeugeAuthorizationContext();

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Dashboard", redirectResult.ControllerName);
        Assert.Equal("Intensivtransport", redirectResult.RouteValues?["area"]);
    }

    [Fact]
    public async Task OnAuthorizationAsync_LeitetZuKeinZugriffWeiter_WennBenutzerkontextFehlt()
    {
        // Arrange
        var filter = new ModuleAccessFilter(
            new FakeCurrentUserContextAccessor(
                CurrentUserContextLookupResult.Fehler("Kein Benutzerkontext vorhanden.")),
            ModulCode.Dienstplan);

        var context = ErzeugeAuthorizationContext();

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("KeinZugriff", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
        Assert.Equal(string.Empty, redirectResult.RouteValues?["area"]);
    }

    [Fact]
    public async Task OnAuthorizationAsync_LeitetZuKeinZugriffWeiter_WennModulcodeUnbekanntIst()
    {
        // Arrange
        var filter = new ModuleAccessFilter(
            new FakeCurrentUserContextAccessor(
                CurrentUserContextLookupResult.Erfolg(
                    new CurrentUserContext(
                        "mitarbeiter-1",
                        OrganisationsbereichCode.Intensivtransport,
                        BereichsrolleCode.Mitarbeiter,
                        FuehrungsverantwortungCode.Keine,
                        [ModulCode.Dienstplan]))),
            ModulCode.Unbekannt);

        var context = ErzeugeAuthorizationContext();

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("KeinZugriff", redirectResult.ActionName);
        Assert.Equal("Account", redirectResult.ControllerName);
        Assert.Equal(string.Empty, redirectResult.RouteValues?["area"]);
    }

    private static AuthorizationFilterContext ErzeugeAuthorizationContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new AuthorizationFilterContext(
            actionContext,
            new List<IFilterMetadata>());
    }
}

internal sealed class FakeCurrentUserContextAccessor : ICurrentUserContextAccessor
{
    private readonly CurrentUserContextLookupResult _result;

    public FakeCurrentUserContextAccessor(CurrentUserContextLookupResult result)
    {
        _result = result;
    }

    public Task<CurrentUserContextLookupResult> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_result);
    }
}