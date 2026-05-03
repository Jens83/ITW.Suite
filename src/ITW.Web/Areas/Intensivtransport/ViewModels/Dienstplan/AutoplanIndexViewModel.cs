using ITW.Dienstplan.Application.Planung;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;

public sealed class AutoplanIndexViewModel
{
    public string Titel { get; set; } = "Autoplan";

    public string Beschreibung { get; set; } = string.Empty;

    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public Guid? AusgewaehltePeriodeId { get; set; }

    public string AusgewaehltePeriodeBezeichnung { get; set; } = string.Empty;

    public bool WunschphaseIstOffen { get; set; }

    public bool PlanIstFreigegeben { get; set; }

    public DateTimeOffset? PlanFreigegebenAm { get; set; }

    public bool VorschauVerfuegbar { get; set; }

    public AutomatischePlanungAusfuehrungsmodus AusgewaehlterModus { get; set; }

    public IReadOnlyList<SelectListItem> ModusOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public int AnzahlPlanbareTage { get; set; }

    public int AnzahlTageOhneHandlungsbedarf { get; set; }

    public int AnzahlTageMitNacharbeit { get; set; }

    public int AnzahlKritischeTage { get; set; }

    public int AnzahlUnveraenderteSlots { get; set; }

    public int AnzahlNeuGesetzteSlots { get; set; }

    public int AnzahlErsetzteSlots { get; set; }

    public int AnzahlWirdOffenSlots { get; set; }

    public int AnzahlBleibtOffenSlots { get; set; }

    public IReadOnlyList<SelectListItem> PeriodenOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<AutoplanTagViewModel> Tage { get; set; }
        = Array.Empty<AutoplanTagViewModel>();

    public string ArbeitsstandTitel =>
        PlanIstFreigegeben
            ? "Freigegebener Plan"
            : WunschphaseIstOffen
                ? "Vorläufiger Arbeitsstand"
                : "Arbeitsstand in Prüfung";

    public string ArbeitsstandHinweis =>
        PlanIstFreigegeben
            ? "Der Plan ist bereits freigegeben und für Mitarbeiter sichtbar. Änderungen sollten bewusst geprüft werden."
            : WunschphaseIstOffen
                ? "Wünsche können sich noch ändern. Die Vorschau und das Speichern dienen hier als Arbeitsstand für den Wachleiter."
                : "Die Wunschphase ist geschlossen. Der Arbeitsstand kann jetzt geprüft, nachbearbeitet und anschließend freigegeben werden.";

    public string ArbeitsstandBadgeCssClass =>
        PlanIstFreigegeben
            ? "bg-primary-subtle text-primary border border-primary-subtle"
            : WunschphaseIstOffen
                ? "bg-warning-subtle text-warning-emphasis border border-warning-subtle"
                : "bg-secondary-subtle text-secondary-emphasis border border-secondary-subtle";
}

public sealed class AutoplanTagViewModel
{
    public DateOnly Datum { get; set; }

    public string DatumAnzeige { get; set; } = string.Empty;

    public bool HatVollstaendigeBesatzung { get; set; }

    public bool HatKonflikt { get; set; }

    public string StatusText { get; set; } = string.Empty;

    public string StatusCssClass { get; set; } = string.Empty;

    public string HandlungsstatusText { get; set; } = string.Empty;

    public string HandlungsstatusCssClass { get; set; } = string.Empty;

    public string Kurzbeschreibung { get; set; } = string.Empty;

    public IReadOnlyList<AutoplanSlotEntscheidungViewModel> SlotEntscheidungen { get; set; }
        = Array.Empty<AutoplanSlotEntscheidungViewModel>();

    public IReadOnlyList<AutoplanKonfliktViewModel> Konflikte { get; set; }
        = Array.Empty<AutoplanKonfliktViewModel>();
}

public sealed class AutoplanSlotEntscheidungViewModel
{
    public string SlotBezeichnung { get; set; } = string.Empty;

    public string AnzeigeName { get; set; } = "Offen";

    public string Hauptqualifikation { get; set; } = string.Empty;

    public bool IstOffen { get; set; }

    public bool HatVorherigeBesetzung { get; set; }

    public string VorherigeAnzeigeName { get; set; } = string.Empty;

    public string ZuweisungsLabel { get; set; } = string.Empty;

    public string ZuweisungsCssClass { get; set; } = string.Empty;

    public string AenderungsLabel { get; set; } = string.Empty;

    public string AenderungsCssClass { get; set; } = string.Empty;

    public string Nachricht { get; set; } = string.Empty;
}

public sealed class AutoplanKonfliktViewModel
{
    public string Label { get; set; } = string.Empty;

    public string CssClass { get; set; } = string.Empty;

    public string Nachricht { get; set; } = string.Empty;
}