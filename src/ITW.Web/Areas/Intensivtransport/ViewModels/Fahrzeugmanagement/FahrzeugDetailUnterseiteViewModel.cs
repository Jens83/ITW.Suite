namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class FahrzeugDetailUnterseiteViewModel
{
    public Guid FahrzeugId { get; init; }

    public string Titel { get; init; } = string.Empty;

    public string Beschreibung { get; init; } = string.Empty;

    public string IconCssClass { get; init; } = "bi bi-info-circle";

    public string Hinweis { get; init; } = string.Empty;

    public FahrzeugDetailNavigationViewModel Navigation { get; init; } = new();
}