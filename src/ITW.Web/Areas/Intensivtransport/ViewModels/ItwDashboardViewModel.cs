namespace ITW.Web.Areas.Intensivtransport.ViewModels;

public sealed class ItwDashboardViewModel
{
    // Rolle
    public bool IstWachleiter { get; init; }
    public bool IstMitarbeiter { get; init; }

    // Modulzugriff
    public bool HatDienstplan { get; init; }
    public bool HatEinsatzverwaltung { get; init; }
    public bool DarfPersonaldatenSehen { get; init; }
    public bool DarfFahrzeugmanagementSehen { get; init; }
    public bool HatMindestensEinModul { get; init; }

    // Wachleiter-Dashboard
    public IReadOnlyList<ItwAktivitaetViewModel> LetzteAktivitaeten { get; init; }
        = Array.Empty<ItwAktivitaetViewModel>();

    public ItwWunschphaseSummaryViewModel? AktuelleWunschphase { get; init; }

    public IReadOnlyList<ItwAufgabeViewModel> OffeneAufgaben { get; init; }
        = Array.Empty<ItwAufgabeViewModel>();
}

public sealed class ItwAktivitaetViewModel
{
    public string Text { get; init; } = "";
    public string Kategorie { get; init; } = "info"; // "ok" | "warn" | "err" | "info"
    public string IconCssClass { get; init; } = "bi bi-info-circle";
    public string ZeitpunktAnzeige { get; init; } = "";
}

public sealed class ItwWunschphaseSummaryViewModel
{
    public string Bezeichnung { get; init; } = "";
    public int GesamtMitarbeiter { get; init; }
    public int EingegangeneWuensche { get; init; }

    public int WeitereAusstehend => WeiterePersonen.Count(p => !p.HatWunschAbgegeben);

    public int ProzentEingegangen => GesamtMitarbeiter > 0
        ? (int)Math.Round((double)EingegangeneWuensche / GesamtMitarbeiter * 100)
        : 0;

    public IReadOnlyList<ItwWunschPersonViewModel> AngezeigtePersonen { get; init; }
        = Array.Empty<ItwWunschPersonViewModel>();

    public IReadOnlyList<ItwWunschPersonViewModel> WeiterePersonen { get; init; }
        = Array.Empty<ItwWunschPersonViewModel>();
}

public sealed class ItwAufgabeViewModel
{
    public Guid    Id                 { get; init; }
    public string  Titel              { get; init; } = "";
    public string  PrioritaetKlasse   { get; init; } = "";   // "normal" | "hoch" | "dringend"
    public bool    IstSystem          { get; init; }
    public string? FaelligkeitAnzeige { get; init; }
    public bool    IstUeberfaellig    { get; init; }
    public string  Gruppe             { get; init; } = "Später"; // "Diese Woche" | "Nächste Woche" | "Später"
}

public sealed class ItwWunschPersonViewModel
{
    public string Kurzname { get; init; } = "";
    public string Kuerzel { get; init; } = "";
    public bool HatWunschAbgegeben { get; init; }
}
