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
    public string? SystemSchluessel   { get; init; }

    // Einzige Stelle für Aufgaben-Navigation: Key-Muster → URL
    public string? NavigationsUrl
    {
        get
        {
            if (string.IsNullOrEmpty(SystemSchluessel)) return null;

            if (SystemSchluessel.StartsWith("itw:dienstplan-fertigstellen:"))
            {
                var id = SystemSchluessel["itw:dienstplan-fertigstellen:".Length..];
                return $"/Intensivtransport/DienstplanWachleiter/Wachleiterkalender?periodeId={id}";
            }
            if (SystemSchluessel.StartsWith("itw:plan-freigeben:"))
            {
                var id = SystemSchluessel["itw:plan-freigeben:".Length..];
                return $"/Intensivtransport/DienstplanWachleiter/Wachleiterkalender?periodeId={id}";
            }
            if (SystemSchluessel.StartsWith("itw:wunschphase-oeffnen:"))
            {
                var id = SystemSchluessel["itw:wunschphase-oeffnen:".Length..];
                return $"/Intensivtransport/DienstplanWachleiter/Index?periodeId={id}";
            }
            if (SystemSchluessel.StartsWith("itw:fahrzeug-pruefung:"))
            {
                // Key: itw:fahrzeug-pruefung:{fahrzeugId}:{pruefungId}
                var rest = SystemSchluessel["itw:fahrzeug-pruefung:".Length..];
                var sep = rest.IndexOf(':');
                var fahrzeugId = sep > 0 ? rest[..sep] : rest;
                return $"/Intensivtransport/FahrzeugPruefungen/Pruefstatus/{fahrzeugId}";
            }

            return null;
        }
    }
}

public sealed class ItwWunschPersonViewModel
{
    public string Kurzname { get; init; } = "";
    public string Kuerzel { get; init; } = "";
    public bool HatWunschAbgegeben { get; init; }
}
