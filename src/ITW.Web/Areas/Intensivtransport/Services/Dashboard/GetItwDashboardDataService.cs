using ITW.Application.Abstractions.Persistence;
using ITW.Application.Aktivitaet;
using ITW.Application.Aufgaben;
using ITW.Application.Organisation.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Domain.Organisation.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels;

namespace ITW.Web.Areas.Intensivtransport.Services.Dashboard;

public sealed class GetItwDashboardDataService
{
    private readonly IAktivitaetsLogRepository _aktivitaetsLog;
    private readonly IDienstplanPeriodeRepository _perioden;
    private readonly IDienstwunschRepository _wuensche;
    private readonly IBenutzerBereichszuordnungRepository _zuordnungen;
    private readonly IAllgemeinesMitarbeiterprofilRepository _profile;
    private readonly IAufgabeRepository _aufgaben;

    public GetItwDashboardDataService(
        IAktivitaetsLogRepository aktivitaetsLog,
        IDienstplanPeriodeRepository perioden,
        IDienstwunschRepository wuensche,
        IBenutzerBereichszuordnungRepository zuordnungen,
        IAllgemeinesMitarbeiterprofilRepository profile,
        IAufgabeRepository aufgaben)
    {
        ArgumentNullException.ThrowIfNull(aktivitaetsLog);
        ArgumentNullException.ThrowIfNull(perioden);
        ArgumentNullException.ThrowIfNull(wuensche);
        ArgumentNullException.ThrowIfNull(zuordnungen);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(aufgaben);

        _aktivitaetsLog = aktivitaetsLog;
        _perioden       = perioden;
        _wuensche       = wuensche;
        _zuordnungen    = zuordnungen;
        _profile        = profile;
        _aufgaben       = aufgaben;
    }

    public async Task<ItwDashboardDataResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var aktivitaeten   = await _aktivitaetsLog.GetByBereichAsync(
            OrganisationsbereichCode.Intensivtransport, 6, cancellationToken);
        var aktivePeriode  = await _perioden.GetAktuelleOffeneAsync(cancellationToken);
        var offeneAufgaben = await _aufgaben.GetOffeneByBereichAsync(
            OrganisationsbereichCode.Intensivtransport, cancellationToken);

        ItwWunschphaseSummaryViewModel? wunschphase = null;

        if (aktivePeriode is not null)
        {
            var alleWuensche    = await _wuensche.GetAlleFuerPeriodeAsync(aktivePeriode.Id, cancellationToken);
            var alleZuordnungen = await _zuordnungen.GetAktivePrimaereZuordnungenByBereichAsync(
                Organisationsbereich.Intensivtransport, cancellationToken);

            var userIdsWithWunsch = alleWuensche
                .Select(w => w.UserId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var alleUserIds = alleZuordnungen.Select(z => z.UserId).ToList();

            var alleProfile = await _profile.GetByUserIdsAsync(alleUserIds, cancellationToken);

            var profilByUserId = alleProfile.ToDictionary(
                p => p.UserId,
                p => p,
                StringComparer.OrdinalIgnoreCase);

            // Honorarkräfte brauchen keine Wünsche abzugeben → aus Übersicht ausblenden
            var relevantePflichtZuordnungen = alleZuordnungen
                .Where(z =>
                {
                    profilByUserId.TryGetValue(z.UserId, out var p);
                    return p?.Beschaeftigungsart != Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Honorarkraft;
                })
                .ToList();

            var personen = relevantePflichtZuordnungen
                .Select(z =>
                {
                    profilByUserId.TryGetValue(z.UserId, out var profil);
                    var vorname  = profil?.Vorname  ?? "";
                    var nachname = profil?.Nachname ?? "";

                    var kurzname = BuildKurzname(vorname, nachname);
                    var kuerzel  = BuildKuerzel(vorname, nachname);

                    return new ItwWunschPersonViewModel
                    {
                        Kurzname           = kurzname,
                        Kuerzel            = kuerzel,
                        HatWunschAbgegeben = userIdsWithWunsch.Contains(z.UserId)
                    };
                })
                .OrderBy(p => p.HatWunschAbgegeben)
                .ThenBy(p => p.Kurzname)
                .ToList();

            const int maxAngezeigt = 6;
            var angezeigt  = personen.Take(maxAngezeigt).ToList();
            var weitere    = personen.Skip(maxAngezeigt).ToList();

            // Eingegangene Wünsche nur für Pflichtmitarbeiter zählen
            var relevanteUserIds = relevantePflichtZuordnungen
                .Select(z => z.UserId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var eingegangeneWuensche = userIdsWithWunsch.Count(id => relevanteUserIds.Contains(id));

            wunschphase = new ItwWunschphaseSummaryViewModel
            {
                Bezeichnung          = aktivePeriode.Bezeichnung,
                GesamtMitarbeiter    = relevantePflichtZuordnungen.Count,
                EingegangeneWuensche = eingegangeneWuensche,
                AngezeigtePersonen   = angezeigt,
                WeiterePersonen      = weitere,
            };
        }

        var aktivitaetVms = aktivitaeten
            .Select(a => new ItwAktivitaetViewModel
            {
                Text             = a.Text,
                Kategorie        = KategorieZuCssKlasse(a.Kategorie),
                IconCssClass     = a.IconCssClass,
                ZeitpunktAnzeige = FormatZeitpunkt(a.Zeitpunkt)
            })
            .ToArray();

        var heute         = DateOnly.FromDateTime(DateTime.Today);
        var wochenstart   = heute.AddDays(-(((int)heute.DayOfWeek + 6) % 7)); // Montag
        var dieseWocheEnd = wochenstart.AddDays(6);
        var naechsteEnd   = wochenstart.AddDays(13);

        var aufgabeVms = offeneAufgaben
            .Select(a => new ItwAufgabeViewModel
            {
                Id                 = a.Id,
                Titel              = a.Titel,
                PrioritaetKlasse   = PrioritaetZuKlasse(a.Prioritaet),
                IstSystem          = a.Quelle == AufgabeQuelle.System,
                FaelligkeitAnzeige = FormatFaelligkeit(a.Faelligkeitsdatum),
                IstUeberfaellig    = a.Faelligkeitsdatum.HasValue && a.Faelligkeitsdatum.Value < heute,
                Gruppe             = ErmittleGruppe(a.Faelligkeitsdatum, heute, dieseWocheEnd, naechsteEnd),
                SystemSchluessel   = a.SystemSchluessel,
            })
            .ToArray();

        return new ItwDashboardDataResult(aktivitaetVms, wunschphase, aufgabeVms);
    }

    private static string BuildKurzname(string vorname, string nachname)
    {
        if (string.IsNullOrWhiteSpace(vorname) && string.IsNullOrWhiteSpace(nachname))
            return "–";
        if (string.IsNullOrWhiteSpace(vorname))
            return nachname;
        return $"{vorname[0]}. {nachname}";
    }

    private static string BuildKuerzel(string vorname, string nachname)
    {
        var v = vorname.Trim();
        var n = nachname.Trim();
        if (v.Length == 0 && n.Length == 0) return "??";
        if (v.Length == 0) return n.Length >= 2 ? n[..2].ToUpperInvariant() : n.ToUpperInvariant();
        if (n.Length == 0) return v.Length >= 2 ? v[..2].ToUpperInvariant() : v.ToUpperInvariant();
        return $"{char.ToUpperInvariant(v[0])}{char.ToUpperInvariant(n[0])}";
    }

    private static string ErmittleGruppe(
        DateOnly? datum,
        DateOnly heute,
        DateOnly dieseWocheEnd,
        DateOnly naechsteEnd)
    {
        if (datum is null || datum.Value > naechsteEnd) return "Später";
        if (datum.Value <= dieseWocheEnd)               return "Diese Woche";
        return "Nächste Woche";
    }

    private static string PrioritaetZuKlasse(AufgabePrioritaet p) => p switch
    {
        AufgabePrioritaet.Hoch     => "hoch",
        AufgabePrioritaet.Dringend => "dringend",
        _                          => "normal"
    };

    private static string? FormatFaelligkeit(DateOnly? datum)
    {
        if (datum is null) return null;
        var heute = DateOnly.FromDateTime(DateTime.Today);
        var diff  = datum.Value.DayNumber - heute.DayNumber;
        return diff switch
        {
            < 0  => $"seit {Math.Abs(diff)} Tag{(Math.Abs(diff) == 1 ? "" : "en")}",
            0    => "heute",
            1    => "morgen",
            <= 7 => $"in {diff} Tagen",
            _    => datum.Value.ToString("dd.MM.yyyy")
        };
    }

    private static string KategorieZuCssKlasse(AktivitaetsKategorie k) => k switch
    {
        AktivitaetsKategorie.Erfolg  => "ok",
        AktivitaetsKategorie.Warnung => "warn",
        AktivitaetsKategorie.Fehler  => "err",
        _                            => "info"
    };

    private static string FormatZeitpunkt(DateTimeOffset zeitpunkt)
    {
        var jetzt = DateTimeOffset.UtcNow;
        var diff  = jetzt - zeitpunkt;
        var local = zeitpunkt.LocalDateTime;

        if (diff.TotalMinutes < 60)
            return $"vor {Math.Max(1, (int)diff.TotalMinutes)} Min.";
        if (diff.TotalHours < 24)
            return $"Heute, {local:HH:mm} Uhr";
        if (diff.TotalDays < 2)
            return $"Gestern, {local:HH:mm} Uhr";

        return $"{local:dd.MM.}, {local:HH:mm} Uhr";
    }
}

public sealed record ItwDashboardDataResult(
    IReadOnlyList<ItwAktivitaetViewModel> LetzteAktivitaeten,
    ItwWunschphaseSummaryViewModel? AktuelleWunschphase,
    IReadOnlyList<ItwAufgabeViewModel> OffeneAufgaben);
