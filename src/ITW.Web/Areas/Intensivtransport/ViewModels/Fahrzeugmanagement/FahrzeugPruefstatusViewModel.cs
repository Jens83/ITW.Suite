using ITW.Fahrzeugmanagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class FahrzeugPruefstatusViewModel
{
    public Guid FahrzeugId { get; set; }

    public string Titel { get; set; } = "Prüfstatus";

    public string Beschreibung { get; set; } =
        "HU/AU, Sicherheitsprüfungen und Service im Blick behalten.";

    public FahrzeugPruefungTyp Typ { get; set; }

    public DateOnly? FaelligAm { get; set; }

    public DateOnly? LetzteErledigungAm { get; set; }

    public string? Bemerkung { get; set; }

    public FahrzeugDetailNavigationViewModel Navigation { get; set; } = new();

    public IReadOnlyList<SelectListItem> PruefungTypOptionen { get; set; } = [];

    public IReadOnlyList<FahrzeugPruefstatusItemViewModel> Pruefungen { get; set; } = [];
}

public sealed class FahrzeugPruefstatusItemViewModel
{
    public Guid? PruefungId { get; init; }

    public FahrzeugPruefungTyp Typ { get; init; }

    public DateOnly? FaelligAm { get; init; }

    public DateOnly? LetzteErledigungAm { get; init; }

    public string? Bemerkung { get; init; }

    public string TypText => Typ switch
    {
        FahrzeugPruefungTyp.HuAu => "HU/AU",
        FahrzeugPruefungTyp.SicherheitspruefungElektrischeAnlage => "Sicherheitsprüfung elektrische Anlage",
        FahrzeugPruefungTyp.SicherheitspruefungSauerstoffanlage => "Sicherheitsprüfung Sauerstoffanlage",
        FahrzeugPruefungTyp.SicherheitspruefungAufbau => "Sicherheitsprüfung Aufbau",
        FahrzeugPruefungTyp.Service => "Service allgemein",
        _ => "Unbekannt"
    };

    public string FaelligAmText => FaelligAm.HasValue
        ? FaelligAm.Value.ToString("dd.MM.yyyy")
        : "Nicht hinterlegt";

    public string LetzteErledigungAmText => LetzteErledigungAm.HasValue
        ? LetzteErledigungAm.Value.ToString("dd.MM.yyyy")
        : "-";

    public DateOnly Heute { get; init; }

    public bool IstHinterlegt => FaelligAm.HasValue;

    public bool IstUeberfaellig
    {
        get
        {
            if (!FaelligAm.HasValue)
            {
                return false;
            }

            return FaelligAm.Value < Heute;
        }
    }

    public bool WirdBaldFaellig
    {
        get
        {
            if (!FaelligAm.HasValue || IstUeberfaellig)
            {
                return false;
            }

            var warnGrenze = Heute.AddDays(30);

            return FaelligAm.Value <= warnGrenze;
        }
    }

    public string StatusText
    {
        get
        {
            if (!FaelligAm.HasValue)
            {
                return "Nicht hinterlegt";
            }

            if (IstUeberfaellig)
            {
                var tage = Heute.DayNumber - FaelligAm.Value.DayNumber;
                return $"Überfällig seit {tage} Tagen";
            }

            if (WirdBaldFaellig)
            {
                var tage = FaelligAm.Value.DayNumber - Heute.DayNumber;
                return $"Fällig in {tage} Tagen";
            }

            return "In Ordnung";
        }
    }

    public string AmpelCssClass
    {
        get
        {
            if (!FaelligAm.HasValue)
            {
                return "app-pill--neutral";
            }

            if (IstUeberfaellig)
            {
                return "app-pill--danger";
            }

            if (WirdBaldFaellig)
            {
                return "app-pill--warning";
            }

            return "app-pill--success";
        }
    }

    public string BemerkungText => string.IsNullOrWhiteSpace(Bemerkung)
        ? "-"
        : Bemerkung;
}