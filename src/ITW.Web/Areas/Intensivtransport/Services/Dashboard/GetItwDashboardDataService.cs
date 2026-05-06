using ITW.Application.Abstractions.Persistence;
using ITW.Application.Aktivitaet;
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

    public GetItwDashboardDataService(
        IAktivitaetsLogRepository aktivitaetsLog,
        IDienstplanPeriodeRepository perioden,
        IDienstwunschRepository wuensche,
        IBenutzerBereichszuordnungRepository zuordnungen,
        IAllgemeinesMitarbeiterprofilRepository profile)
    {
        ArgumentNullException.ThrowIfNull(aktivitaetsLog);
        ArgumentNullException.ThrowIfNull(perioden);
        ArgumentNullException.ThrowIfNull(wuensche);
        ArgumentNullException.ThrowIfNull(zuordnungen);
        ArgumentNullException.ThrowIfNull(profile);

        _aktivitaetsLog = aktivitaetsLog;
        _perioden       = perioden;
        _wuensche       = wuensche;
        _zuordnungen    = zuordnungen;
        _profile        = profile;
    }

    public async Task<ItwDashboardDataResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var aktivitaetTask = _aktivitaetsLog.GetByBereichAsync(
            OrganisationsbereichCode.Intensivtransport, 6, cancellationToken);
        var periodeTask = _perioden.GetAktuelleOffeneAsync(cancellationToken);

        await Task.WhenAll(aktivitaetTask, periodeTask);

        var aktivitaeten  = await aktivitaetTask;
        var aktivePeriode = await periodeTask;

        ItwWunschphaseSummaryViewModel? wunschphase = null;

        if (aktivePeriode is not null)
        {
            var wuenscheTask    = _wuensche.GetAlleFuerPeriodeAsync(aktivePeriode.Id, cancellationToken);
            var zuordnungenTask = _zuordnungen.GetAktivePrimaereZuordnungenByBereichAsync(
                Organisationsbereich.Intensivtransport, cancellationToken);

            await Task.WhenAll(wuenscheTask, zuordnungenTask);

            var alleWuensche    = await wuenscheTask;
            var alleZuordnungen = await zuordnungenTask;

            var userIdsWithWunsch = alleWuensche
                .Select(w => w.UserId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var alleUserIds = alleZuordnungen.Select(z => z.UserId).ToList();

            var alleProfile = await _profile.GetByUserIdsAsync(alleUserIds, cancellationToken);

            var profilByUserId = alleProfile.ToDictionary(
                p => p.UserId,
                p => p,
                StringComparer.OrdinalIgnoreCase);

            var personen = alleZuordnungen
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
                .OrderBy(p => p.HatWunschAbgegeben)   // Ausstehende zuerst
                .ThenBy(p => p.Kurzname)
                .ToList();

            const int maxAngezeigt = 6;
            var angezeigt        = personen.Take(maxAngezeigt).ToList();
            var gezeigtNichtAbg  = angezeigt.Count(p => !p.HatWunschAbgegeben);
            var gesamtNichtAbg   = personen.Count(p => !p.HatWunschAbgegeben);
            var weitereAusstehend = Math.Max(0, gesamtNichtAbg - gezeigtNichtAbg);

            wunschphase = new ItwWunschphaseSummaryViewModel
            {
                Bezeichnung          = aktivePeriode.Bezeichnung,
                GesamtMitarbeiter    = alleZuordnungen.Count,
                EingegangeneWuensche = userIdsWithWunsch.Count,
                AngezeigtePersonen   = angezeigt,
                WeitereAusstehend    = weitereAusstehend
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

        return new ItwDashboardDataResult(aktivitaetVms, wunschphase);
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
    ItwWunschphaseSummaryViewModel? AktuelleWunschphase);
