using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub;
using ITW.Dienstplan.Application.Auswertung;
using ITW.Dienstplan.Application.Perioden;
using ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Read;

public sealed record ReadMonatsauswertungViewModelQuery(Guid? PeriodeId);

public sealed class ReadMonatsauswertungViewModelService
{
    private readonly ReadDienstplanperiodenService _readDienstplanperiodenService;
    private readonly ReadDienstplanMonatsauswertungService _readDienstplanMonatsauswertungService;
    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly ReadMitarbeiterUrlaubsplanerService _readMitarbeiterUrlaubsplanerService;

    public ReadMonatsauswertungViewModelService(
        ReadDienstplanperiodenService readDienstplanperiodenService,
        ReadDienstplanMonatsauswertungService readDienstplanMonatsauswertungService,
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        ReadMitarbeiterUrlaubsplanerService readMitarbeiterUrlaubsplanerService)
    {
        _readDienstplanperiodenService = readDienstplanperiodenService
            ?? throw new ArgumentNullException(nameof(readDienstplanperiodenService));

        _readDienstplanMonatsauswertungService = readDienstplanMonatsauswertungService
            ?? throw new ArgumentNullException(nameof(readDienstplanMonatsauswertungService));

        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService
            ?? throw new ArgumentNullException(nameof(readItwMitarbeiterprofileService));

        _readMitarbeiterUrlaubsplanerService = readMitarbeiterUrlaubsplanerService
            ?? throw new ArgumentNullException(nameof(readMitarbeiterUrlaubsplanerService));
    }

    public async Task<MonatsauswertungViewModel> ExecuteAsync(
        ReadMonatsauswertungViewModelQuery query,
        CancellationToken cancellationToken = default)
    {
        var readPeriodenResult = await _readDienstplanperiodenService.ExecuteAsync(cancellationToken);
        var perioden = readPeriodenResult.Perioden;

        var ausgewaehltePeriode = ErmittleAusgewaehltePeriode(perioden, query.PeriodeId);

        if (ausgewaehltePeriode is null)
        {
            return new MonatsauswertungViewModel
            {
                Titel = "Monatsauswertung",
                Beschreibung = "Monatliche Übersicht über Dienste, Vertretungen sowie Krank-/Urlaubsstände.",
                IsSuccess = readPeriodenResult.IsSuccess,
                ErrorMessage = readPeriodenResult.ErrorMessage,
                PeriodenOptionen = Array.Empty<SelectListItem>(),
                Mitarbeiter = Array.Empty<MonatsauswertungMitarbeiterViewModel>()
            };
        }

        var auswertungResult = await _readDienstplanMonatsauswertungService.ExecuteAsync(
            ausgewaehltePeriode.Id,
            cancellationToken);

        var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);

        var isSuccess = readPeriodenResult.IsSuccess && auswertungResult.IsSuccess && mitarbeiterResult.IsSuccess;
        var errorMessage = readPeriodenResult.ErrorMessage
                           ?? auswertungResult.ErrorMessage
                           ?? mitarbeiterResult.ErrorMessage;

        var statistikLookup = auswertungResult.Eintraege.ToDictionary(
            x => x.UserId,
            StringComparer.OrdinalIgnoreCase);

        var mitarbeiter = new List<MonatsauswertungMitarbeiterViewModel>();

        if (mitarbeiterResult.IsSuccess)
        {
            var bekannteUserIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var profil in mitarbeiterResult.Profile
                         .Where(x => !x.IstGesperrt && x.HatProfil)
                         .OrderBy(x => x.Hauptqualifikation, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase))
            {
                bekannteUserIds.Add(profil.UserId);

                statistikLookup.TryGetValue(profil.UserId, out var statistik);

                var gefahreneDienste = statistik?.GefahreneDienste ?? 0;
                var vertretungen = statistik?.Vertretungen ?? 0;

                mitarbeiter.Add(new MonatsauswertungMitarbeiterViewModel
                {
                    UserId = profil.UserId,
                    AnzeigeName = profil.AnzeigeName,
                    Hauptqualifikation = profil.Hauptqualifikation,
                    GeplanteDienste = statistik?.GeplanteDienste ?? 0,
                    Krankheitstage = statistik?.Krankheitstage ?? 0,
                    Urlaubstage = statistik?.Urlaubstage ?? 0,
                    Vertretungen = vertretungen,
                    GefahreneDienste = gefahreneDienste,
                    Gesamt = gefahreneDienste + vertretungen
                });
            }

            foreach (var rest in auswertungResult.Eintraege
                         .Where(x => !bekannteUserIds.Contains(x.UserId))
                         .OrderBy(x => x.UserId, StringComparer.OrdinalIgnoreCase))
            {
                mitarbeiter.Add(new MonatsauswertungMitarbeiterViewModel
                {
                    UserId = rest.UserId,
                    AnzeigeName = rest.UserId,
                    Hauptqualifikation = "Unbekannt",
                    GeplanteDienste = rest.GeplanteDienste,
                    Krankheitstage = rest.Krankheitstage,
                    Urlaubstage = rest.Urlaubstage,
                    Vertretungen = rest.Vertretungen,
                    GefahreneDienste = rest.GefahreneDienste,
                    Gesamt = rest.GefahreneDienste + rest.Vertretungen
                });
            }
        }

        var sichtbareMitarbeiter = mitarbeiter
            .Where(x =>
                x.GeplanteDienste > 0 ||
                x.Krankheitstage > 0 ||
                x.Urlaubstage > 0 ||
                x.Vertretungen > 0 ||
                x.GefahreneDienste > 0)
            .OrderBy(x => x.Hauptqualifikation, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await ErgaenzeJahresurlaubsstandAsync(
            sichtbareMitarbeiter,
            mitarbeiterResult,
            ausgewaehltePeriode.Jahr,
            cancellationToken);

        return new MonatsauswertungViewModel
        {
            Titel = "Monatsauswertung",
            Beschreibung = "Monatliche Übersicht über Dienste, Vertretungen sowie Krank-/Urlaubsstände.",
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage,
            AusgewaehltePeriodeId = ausgewaehltePeriode.Id,
            AusgewaehltePeriodeBezeichnung = ausgewaehltePeriode.Bezeichnung,
            PeriodenOptionen = perioden
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Bezeichnung,
                    Selected = x.Id == ausgewaehltePeriode.Id
                })
                .ToArray(),
            Mitarbeiter = sichtbareMitarbeiter,
            SummeGeplanteDienste = sichtbareMitarbeiter.Sum(x => x.GeplanteDienste),
            SummeKrankheitstage = sichtbareMitarbeiter.Sum(x => x.Krankheitstage),
            SummeUrlaubstage = sichtbareMitarbeiter.Sum(x => x.Urlaubstage),
            SummeVertretungen = sichtbareMitarbeiter.Sum(x => x.Vertretungen),
            SummeGefahreneDienste = sichtbareMitarbeiter.Sum(x => x.GefahreneDienste),
            SummeGesamt = sichtbareMitarbeiter.Sum(x => x.Gesamt)
        };
    }

    private static DienstplanPeriodeListenEintrag? ErmittleAusgewaehltePeriode(
        IReadOnlyList<DienstplanPeriodeListenEintrag> perioden,
        Guid? periodeId)
    {
        DienstplanPeriodeListenEintrag? ausgewaehltePeriode = null;

        if (periodeId.HasValue && periodeId.Value != Guid.Empty)
        {
            ausgewaehltePeriode = perioden.FirstOrDefault(x => x.Id == periodeId.Value);
        }

        ausgewaehltePeriode ??= perioden.FirstOrDefault(x => x.WunschphaseIstOffen);
        ausgewaehltePeriode ??= perioden.FirstOrDefault();

        return ausgewaehltePeriode;
    }

    private async Task ErgaenzeJahresurlaubsstandAsync(
        IReadOnlyList<MonatsauswertungMitarbeiterViewModel> mitarbeiter,
        ReadItwMitarbeiterprofileResult mitarbeiterResult,
        int jahr,
        CancellationToken cancellationToken)
    {
        if (mitarbeiter.Count == 0 || !mitarbeiterResult.IsSuccess)
        {
            return;
        }

        var profilLookup = mitarbeiterResult.Profile.ToDictionary(
            x => x.UserId,
            StringComparer.OrdinalIgnoreCase);

        foreach (var eintrag in mitarbeiter)
        {
            if (!profilLookup.TryGetValue(eintrag.UserId, out var profil))
            {
                continue;
            }

            if (profil.Beschaeftigungsart is not Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt
                and not Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer)
            {
                continue;
            }

            var urlaubsstand = await _readMitarbeiterUrlaubsplanerService.ExecuteAsync(
                new ReadMitarbeiterUrlaubsplanerQuery
                {
                    UserId = profil.UserId,
                    Jahr = jahr,
                    Beschaeftigungsart = profil.Beschaeftigungsart
                },
                cancellationToken);

            if (!urlaubsstand.IsSuccess)
            {
                continue;
            }

            eintrag.Jahresurlaubsanspruch = urlaubsstand.Anspruchstage + urlaubsstand.Uebertragstage;
            eintrag.GenommeneUrlaubstageImJahr = urlaubsstand.GenommeneUrlaubstage;
            eintrag.Resturlaubstage = urlaubsstand.Resturlaubstage;
        }
    }
}