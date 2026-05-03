namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class FahrzeugDetailNavigationViewModel
{
    public Guid FahrzeugId { get; init; }

    public string Kennzeichen { get; init; } = string.Empty;

    public string InterneNummer { get; init; } = string.Empty;

    public string Fahrzeugname { get; init; } = string.Empty;

    public string StatusText { get; init; } = string.Empty;

    public string AktiveSeite { get; init; } = "Details";
}