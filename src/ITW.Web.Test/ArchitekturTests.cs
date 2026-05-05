using System.Reflection;
using NetArchTest.Rules;

namespace ITW.Web.Test;

/// <summary>
/// Stellt sicher, dass die Web-Orchestrierungsschicht keine verbotenen Abhängigkeiten einführt.
/// Regeln lt. docs/05_Konventionen/02_Web_Orchestration.md
/// </summary>
public sealed class ArchitekturTests
{
    // FlashKeys ist ein öffentlicher Typ aus ITW.Web — dient als Assembly-Anker
    private static readonly Assembly WebAssembly =
        typeof(ITW.Web.UI.Feedback.FlashKeys).Assembly;

    // --- Regel 1: Kein DbContext in Web.Areas ---

    [Fact]
    public void WebAreas_SollenKeinenDbContextReferenzieren()
    {
        var result = Types.InAssembly(WebAssembly)
            .That()
            .ResideInNamespaceStartingWith("ITW.Web.Areas")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore.DbContext")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result, "DbContext"));
    }

    // --- Regel 2: Keine konkreten Infrastructure-Typen in Web.Areas ---

    [Fact]
    public void WebAreas_SollenKeineInfrastrukturAbhaengigkeitenHaben()
    {
        var result = Types.InAssembly(WebAssembly)
            .That()
            .ResideInNamespaceStartingWith("ITW.Web.Areas")
            .ShouldNot()
            .HaveDependencyOn("ITW.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result, "ITW.Infrastructure"));
    }

    // --- Regel 3: Controller dürfen keine Domain-Entitäten verwenden ---
    // Bekannte Ausnahmen (Technikschulden, zu migrieren):
    //   UrlaubsplanerController   → nutzt IDienstplanPeriodeRepository / IDienstwunschRepository direkt
    //   DienstplanWachleiterController → nutzt AusgewaehltePeriode (DienstplanPeriode-Entität)
    // Beide sind in docs/05_Konventionen/02_Web_Orchestration.md dokumentiert.

    [Fact]
    public void WebAreas_Controller_SollenKeineDienstplanEntitaetenVerwenden_OhneBekannteTechnikschulden()
    {
        var result = Types.InAssembly(WebAssembly)
            .That()
            .ResideInNamespaceStartingWith("ITW.Web.Areas")
            .And()
            .HaveNameEndingWith("Controller")
            .And()
            .DoNotHaveName("UrlaubsplanerController")
            .And()
            .DoNotHaveName("DienstplanWachleiterController")
            .ShouldNot()
            .HaveDependencyOn("ITW.Dienstplan.Domain.Entities")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result, "ITW.Dienstplan.Domain.Entities"));
    }

    [Fact]
    public void WebAreas_Controller_SollenKeineDomainEntitaetenVerwenden()
    {
        var result = Types.InAssembly(WebAssembly)
            .That()
            .ResideInNamespaceStartingWith("ITW.Web.Areas")
            .And()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn("ITW.Domain.Organisation.Entities")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result, "ITW.Domain.Organisation.Entities"));
    }

    [Fact]
    public void WebAreas_Controller_SollenKeinePersonalEntitaetenVerwenden()
    {
        var result = Types.InAssembly(WebAssembly)
            .That()
            .ResideInNamespaceStartingWith("ITW.Web.Areas")
            .And()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn("ITW.Domain.Personnel.Entities")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result, "ITW.Domain.Personnel.Entities"));
    }

    private static string FormatViolations(TestResult result, string forbidden)
    {
        if (result.IsSuccessful)
        {
            return string.Empty;
        }

        var types = result.FailingTypes is { Count: > 0 }
            ? string.Join(", ", result.FailingTypes.Select(t => t.FullName))
            : "(keine Details)";

        return $"Verstöße gegen Abhängigkeitsverbot auf '{forbidden}': {types}";
    }
}
