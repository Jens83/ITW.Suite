using ITW.Fahrzeugmanagement.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class FahrzeugDokumenteViewModel
{
    public Guid FahrzeugId { get; set; }

    public string Titel { get; set; } = "Dokumente";

    public string Beschreibung { get; set; } =
        "Fahrzeugdokumente hochladen und verwalten.";

    public FahrzeugDokumentKategorie Kategorie { get; set; }

    public string Bezeichnung { get; set; } = string.Empty;

    public DateOnly? GueltigBis { get; set; }

    public IFormFile? Datei { get; set; }

    public FahrzeugDetailNavigationViewModel Navigation { get; set; } = new();

    public IReadOnlyList<SelectListItem> KategorieOptionen { get; set; } = [];

    public IReadOnlyList<FahrzeugDokumentItemViewModel> Dokumente { get; set; } = [];
}

public sealed class FahrzeugDokumentItemViewModel
{
    public Guid DokumentId { get; init; }

    public FahrzeugDokumentKategorie Kategorie { get; init; }

    public string Bezeichnung { get; init; } = string.Empty;

    public string KategorieText => Kategorie switch
    {
        FahrzeugDokumentKategorie.Zulassung => "Zulassung",
        FahrzeugDokumentKategorie.Versicherung => "Versicherung",
        FahrzeugDokumentKategorie.Leasingvertrag => "Leasingvertrag",
        FahrzeugDokumentKategorie.Wartungsnachweis => "Wartungsnachweis",
        FahrzeugDokumentKategorie.Werkstattrechnung => "Werkstattrechnung",
        FahrzeugDokumentKategorie.FahrtenbuchBeleg => "Fahrtenbuch-Beleg",
        FahrzeugDokumentKategorie.Tankbeleg => "Tankbeleg",
        FahrzeugDokumentKategorie.Sonstiges => "Sonstiges",
        _ => "Unbekannt"
    };

    public string Dateiname { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public DateOnly? GueltigBis { get; init; }

    public DateTimeOffset HochgeladenAm { get; init; }

    public DateOnly Heute { get; init; }

    public string HochgeladenAmText => HochgeladenAm.ToLocalTime().ToString("dd.MM.yyyy");

    public string GueltigBisText => GueltigBis.HasValue
        ? GueltigBis.Value.ToString("dd.MM.yyyy")
        : "-";

    public string IconCssClass
    {
        get
        {
            if (ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-file-earmark-pdf";
            }

            if (ContentType.Contains("image", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-file-earmark-image";
            }

            if (ContentType.Contains("word", StringComparison.OrdinalIgnoreCase) ||
                ContentType.Contains("officedocument", StringComparison.OrdinalIgnoreCase))
            {
                return "bi-file-earmark-word";
            }

            return "bi-file-earmark-text";
        }
    }

    public bool IstAbgelaufen
    {
        get
        {
            if (!GueltigBis.HasValue)
            {
                return false;
            }

            return GueltigBis.Value < Heute;
        }
    }

    public bool LaeuftBaldAb
    {
        get
        {
            if (!GueltigBis.HasValue || IstAbgelaufen)
            {
                return false;
            }

            var warnGrenze = Heute.AddDays(30);

            return GueltigBis.Value <= warnGrenze;
        }
    }

    public string FristHinweisText
    {
        get
        {
            if (!GueltigBis.HasValue)
            {
                return "Keine Frist";
            }

            if (IstAbgelaufen)
            {
                return "Abgelaufen";
            }

            if (LaeuftBaldAb)
            {
                return "Läuft bald ab";
            }

            return "Gültig";
        }
    }

    public string FristBadgeCssClass
    {
        get
        {
            if (!GueltigBis.HasValue)
            {
                return "bg-secondary";
            }

            if (IstAbgelaufen)
            {
                return "bg-danger";
            }

            if (LaeuftBaldAb)
            {
                return "bg-warning text-dark";
            }

            return "bg-success";
        }
    }

    public string AnzeigeTitel
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Bezeichnung))
            {
                return Bezeichnung;
            }

            return $"{KategorieText} {HochgeladenAmText}".Trim();
        }
    }
}