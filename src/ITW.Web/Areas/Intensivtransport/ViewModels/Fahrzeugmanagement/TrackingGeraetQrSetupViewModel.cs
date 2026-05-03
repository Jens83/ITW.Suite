namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class TrackingGeraetQrSetupViewModel
{
    public string Titel { get; init; } = "Tablet hinzufügen";

    public string Beschreibung { get; init; } =
        "QR-Code mit dem Einsatz-Tablet scannen.";

    public string SetupLink { get; init; } = string.Empty;

    public string QrCodeSvg { get; init; } = string.Empty;

    public string GueltigBisText { get; init; } = "-";
}