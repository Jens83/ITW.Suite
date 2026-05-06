using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;

public sealed class DienstplanIndexViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public bool DarfPeriodeAnlegen { get; set; }

    public bool ModalAutomatischOeffnen { get; set; }

    [Range(1, 12, ErrorMessage = "Bitte einen gültigen Monat auswählen.")]
    public int Monat { get; set; }

    [Range(2000, 2100, ErrorMessage = "Bitte ein gültiges Jahr auswählen.")]
    public int Jahr { get; set; }

    [Display(Name = "Wunschphase direkt öffnen")]
    public bool WunschphaseDirektOeffnen { get; set; }

    public bool HatOffeneWunschphase { get; set; }

    public Guid? AktivePeriodeId { get; set; }

    public string AktivePeriodeBezeichnung { get; set; } = string.Empty;

    public int AktivePeriodeMonat { get; set; }

    public int AktivePeriodeJahr { get; set; }

    public string BeschaeftigungsartText { get; set; } = string.Empty;

    public bool IstFreelancer { get; set; }

    public bool IstFestangestellt { get; set; }

    public bool DarfWunschTageWaehlen =>
        !IstFreelancer || FreelancerGewuenschteDienste.HasValue;

    public int AnzahlWunschTage { get; set; }

    public int AnzahlNichtVerfuegbareTage { get; set; }

    public int AnzahlGewaehlteTage => AnzahlWunschTage;

    public int? FreelancerGewuenschteDienste { get; set; }

    public IReadOnlyList<SelectListItem> MonatOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> JahrOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> FreelancerDienstanzahlOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<DienstplanPeriodeEintragViewModel> Perioden { get; set; }
        = Array.Empty<DienstplanPeriodeEintragViewModel>();

    public IReadOnlyList<DienstplanKalenderWocheViewModel> KalenderWochen { get; set; }
        = Array.Empty<DienstplanKalenderWocheViewModel>();
}

public sealed class DienstplanPeriodeEintragViewModel
{
    public Guid Id { get; set; }

    public string Bezeichnung { get; set; } = string.Empty;

    public int Jahr { get; set; }

    public int Monat { get; set; }

    public bool WunschphaseIstOffen { get; set; }

    public bool PlanIstFreigegeben { get; set; }

    public DateTimeOffset? PlanFreigegebenAm { get; set; }

    public DateTimeOffset ErstelltAm { get; set; }
}

public sealed class DienstplanKalenderWocheViewModel
{
    public IReadOnlyList<DienstplanKalenderTagViewModel> Tage { get; set; }
        = Array.Empty<DienstplanKalenderTagViewModel>();
}

public sealed class DienstplanKalenderTagViewModel
{
    public DateOnly Datum { get; set; }

    public bool IstImAktivenMonat { get; set; }

    public bool IstHeute { get; set; }

    public bool IstAlsWunschMarkiert { get; set; }

    public bool IstAlsNichtVerfuegbarMarkiert { get; set; }

    public bool IstGewaehlt => IstAlsWunschMarkiert;

    public bool IstFeiertag { get; set; }

    public bool IstWochenende { get; set; }

    public bool IstUrlaub { get; set; }

    public bool IstAuswaehlbar { get; set; }

    public string FeiertagsName { get; set; } = string.Empty;

    public string SperrgrundText =>
     IstFeiertag
         ? FeiertagsName
         : IstUrlaub
             ? "Urlaub"
             : IstWochenende
                 ? "Wochenende"
                 : string.Empty;

    public int AnzahlWuenscheGesamt { get; set; }

    public int AnzahlAerzte { get; set; }

    public int AnzahlNotfallsanitaeter { get; set; }

    public int AnzahlGeplanteAerzte { get; set; }

    public int AnzahlGeplanteNotfallsanitaeter { get; set; }

    public bool HatAutomatikKonflikt { get; set; }

    public string AutomatikKonfliktText { get; set; } = string.Empty;

    public bool HatSondereinsatz =>
        (IstFeiertag || IstWochenende) &&
        IstImAktivenMonat &&
        (AnzahlGeplanteAerzte > 0 || AnzahlGeplanteNotfallsanitaeter > 0);

    public bool HatVollstaendigeBesatzung =>
        !IstFeiertag &&
        !IstWochenende &&
        AnzahlGeplanteAerzte >= 1 &&
        AnzahlGeplanteNotfallsanitaeter >= 2;

    public bool HatTeilweiseBesatzung =>
        !IstFeiertag &&
        !IstWochenende &&
        !HatVollstaendigeBesatzung &&
        (AnzahlWuenscheGesamt > 0 || AnzahlGeplanteAerzte > 0 || AnzahlGeplanteNotfallsanitaeter > 0);

    public bool HatKeineWuensche =>
        !IstFeiertag &&
        !IstWochenende &&
        AnzahlWuenscheGesamt <= 0 &&
        AnzahlGeplanteAerzte <= 0 &&
        AnzahlGeplanteNotfallsanitaeter <= 0;

    public string WachleiterStatusText =>
        IstFeiertag || IstWochenende
            ? string.Empty
            : HatAutomatikKonflikt
                ? "Konflikt"
                : HatVollstaendigeBesatzung
                    ? "Vollständig"
                    : HatTeilweiseBesatzung
                        ? "Offen"
                        : "Kein Wunsch";

    public string WachleiterStatusCssClass =>
        IstFeiertag || IstWochenende
            ? string.Empty
            : HatAutomatikKonflikt
                ? "status-offen"
                : HatVollstaendigeBesatzung
                    ? "status-vollstaendig"
                    : HatTeilweiseBesatzung
                        ? "status-teilweise"
                        : "status-offen";
}