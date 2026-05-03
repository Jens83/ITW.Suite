namespace ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;

public sealed class SichtbarerDienstplanViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public bool HatFreigegebenePlaene { get; set; }

    public IReadOnlyList<SichtbarerPlanAbschnittViewModel> Abschnitte { get; set; }
        = Array.Empty<SichtbarerPlanAbschnittViewModel>();
}

public sealed class SichtbarerPlanAbschnittViewModel
{
    public string Ueberschrift { get; set; } = string.Empty;

    public Guid PeriodeId { get; set; }

    public string PeriodeBezeichnung { get; set; } = string.Empty;

    public int Jahr { get; set; }

    public int Monat { get; set; }

    public IReadOnlyList<SichtbarerPlanTagViewModel> Tage { get; set; }
        = Array.Empty<SichtbarerPlanTagViewModel>();
}

public sealed class SichtbarerPlanTagViewModel
{
    public DateOnly Datum { get; set; }

    public string DatumAnzeige { get; set; } = string.Empty;

    public string WochentagKurz { get; set; } = string.Empty;

    public bool IstHeute { get; set; }

    public bool IstWochenende { get; set; }

    public bool IstFeiertag { get; set; }

    public string FeiertagsName { get; set; } = string.Empty;

    public bool IstTagDesBenutzers { get; set; }

    public bool EigenerDienstIstVertretung { get; set; }

    public bool HatSichtbareSlots => Slots.Count > 0;

    public string? EigenerStatusText => !IstTagDesBenutzers
        ? null
        : EigenerDienstIstVertretung
            ? "Vertretung"
            : "Dienst";

    public IReadOnlyList<SichtbarerPlanSlotViewModel> Slots { get; set; }
        = Array.Empty<SichtbarerPlanSlotViewModel>();
}

public sealed class SichtbarerPlanSlotViewModel
{
    public string SlotBezeichnung { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public string AnzeigeName { get; set; } = string.Empty;

    public string Hauptqualifikation { get; set; } = string.Empty;

    public bool IstOffen { get; set; }

    public bool IstVertretung { get; set; }

    public string BadgeText => IstOffen
        ? "Offen"
        : IstVertretung
            ? "Vertretung"
            : "Dienst";

    public string? VertretungFuerAnzeigeName { get; set; }
}