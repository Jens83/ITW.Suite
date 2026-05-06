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
    public int WeitereAusstehend { get; init; }

    public int ProzentEingegangen => GesamtMitarbeiter > 0
        ? (int)Math.Round((double)EingegangeneWuensche / GesamtMitarbeiter * 100)
        : 0;

    public IReadOnlyList<ItwWunschPersonViewModel> AngezeigtePersonen { get; init; }
        = Array.Empty<ItwWunschPersonViewModel>();
}

public sealed class ItwWunschPersonViewModel
{
    public string Kurzname { get; init; } = "";
    public string Kuerzel { get; init; } = "";
    public bool HatWunschAbgegeben { get; init; }
}
