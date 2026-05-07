using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Urlaubsplaner;

public sealed class UrlaubsplanerIndexViewModel
{
    public string Titel { get; set; } = "Urlaubsplaner";

    public string Beschreibung { get; set; } = "Jahresanspruch, Übertrag und Urlaubszeiträume für Mitarbeiter im Intensivtransport.";

    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public string? AusgewaehlterUserId { get; set; }

    public int AusgewaehltesJahr { get; set; }

    public string MitarbeiterAnzeigeName { get; set; } = string.Empty;

    public string BeschaeftigungsartText { get; set; } = string.Empty;

    public bool IstFestangestellt { get; set; }

    public bool IstFreelancer { get; set; }

    public bool KannUrlaubszeitraeumePflegen => IstFestangestellt;

    public int Anspruchstage { get; set; }

    public int Uebertragstage { get; set; }

    public int GenommeneUrlaubstage { get; set; }

    public int Resturlaubstage { get; set; }

    public bool AnspruchIstStandardwert { get; set; }

    public int AnzahlAusstehend { get; set; }

    public bool HatMitarbeiterAuswahl => !string.IsNullOrWhiteSpace(AusgewaehlterUserId);

    public IReadOnlyList<SelectListItem> MitarbeiterOptionen { get; set; } = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> JahrOptionen { get; set; } = Array.Empty<SelectListItem>();

    public IReadOnlyList<UrlaubszeitraumEintragViewModel> Zeitraeume { get; set; } = Array.Empty<UrlaubszeitraumEintragViewModel>();
}

public sealed class UrlaubszeitraumEintragViewModel
{
    public Guid Id { get; set; }

    public string VonAnzeige { get; set; } = string.Empty;

    public string BisAnzeige { get; set; } = string.Empty;

    public int Urlaubstage { get; set; }

    public string? Notiz { get; set; }
}