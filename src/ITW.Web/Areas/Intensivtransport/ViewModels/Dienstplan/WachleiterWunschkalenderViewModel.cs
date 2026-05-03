using ITW.Dienstplan.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;

public sealed class WachleiterWunschkalenderViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public Guid? AusgewaehltePeriodeId { get; set; }

    public string AusgewaehltePeriodeBezeichnung { get; set; } = string.Empty;

    public bool WunschphaseIstOffen { get; set; }

    public bool PlanIstFreigegeben { get; set; }

    public DateTimeOffset? PlanFreigegebenAm { get; set; }

    public bool PlanungsModalAutomatischOeffnen { get; set; }

    public WachleiterPlanungsModalViewModel? PlanungsModal { get; set; }

    public IReadOnlyList<SelectListItem> PeriodenOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<DienstplanKalenderWocheViewModel> KalenderWochen { get; set; }
        = Array.Empty<DienstplanKalenderWocheViewModel>();
}

public sealed class WachleiterPlanungsModalViewModel
{
    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public Guid PeriodeId { get; set; }

    public string PeriodeBezeichnung { get; set; } = string.Empty;

    public DateOnly Datum { get; set; }

    public string DatumAnzeige { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    public DienstplanKalenderTagViewModel Tag { get; set; } = new();

    public string? ArztUserId { get; set; }

    public string? Notfallsanitaeter1UserId { get; set; }

    public string? Notfallsanitaeter2UserId { get; set; }

    public IReadOnlyList<SelectListItem> ArztOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> NotfallsanitaeterOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<WunschMitarbeiterEintragViewModel> WunschMitarbeiter { get; set; }
        = Array.Empty<WunschMitarbeiterEintragViewModel>();

    public IReadOnlyList<PlanungsSlotViewModel> PlanungsSlots { get; set; }
        = Array.Empty<PlanungsSlotViewModel>();
}

public sealed class WunschMitarbeiterEintragViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string AnzeigeName { get; set; } = string.Empty;

    public string Hauptqualifikation { get; set; } = string.Empty;

    public bool IstBereitsGeplant { get; set; }

    public bool KannAlsArztGeplantWerden { get; set; }

    public bool KannAlsNotfallsanitaeterGeplantWerden { get; set; }
}

public sealed class PlanungsSlotViewModel
{
    public DienstbesetzungsSlotCode SlotCode { get; set; }

    public string SlotBezeichnung { get; set; } = string.Empty;

    public string AktuelleBesetzungAnzeigeName { get; set; } = "Noch offen";

    public string AktuelleBesetzungQualifikation { get; set; } = string.Empty;

    public string UrspruenglichGeplanteAnzeigeName { get; set; } = "Noch nicht gesetzt";

    public string UrspruenglichGeplanteQualifikation { get; set; } = string.Empty;

    public bool HatAusfall { get; set; }

    public DienstausfallGrundCode? AusfallGrundCode { get; set; }

    public string StatusText { get; set; } = "Regulär geplant";

    public string? VertretungsUserId { get; set; }

    public IReadOnlyList<SelectListItem> VertretungsOptionen { get; set; }
        = Array.Empty<SelectListItem>();
}