// Datei: src/ITW.Web/Areas/Intensivtransport/ViewModels/Mitarbeiter/MitarbeiterDokumentEintragViewModel.cs
namespace ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter;

public sealed class MitarbeiterDokumentEintragViewModel
{
    public Guid DokumentId { get; set; }

    public string Kategorie { get; set; } = string.Empty;

    public string DateinameOriginal { get; set; } = string.Empty;

    public string Inhaltstyp { get; set; } = string.Empty;

    public string DateigroesseText { get; set; } = string.Empty;

    public DateTimeOffset HochgeladenAm { get; set; }
}