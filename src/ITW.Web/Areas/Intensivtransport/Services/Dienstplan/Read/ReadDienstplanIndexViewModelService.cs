using ITW.Application.Organisation.Contracts;
using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Application.Perioden;
using ITW.Dienstplan.Application.Wunschphase;
using ITW.Domain.Kalender;
using ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Read;

public sealed record ReadDienstplanIndexViewModelQuery(
    CurrentUserContext CurrentUser,
    int Monat,
    int Jahr,
    bool WunschphaseDirektOeffnen,
    bool ModalAutomatischOeffnen);

public sealed class ReadDienstplanIndexViewModelService
{
    private static readonly CultureInfo DeutscheKultur = CultureInfo.GetCultureInfo("de-DE");

    private readonly ReadDienstplanperiodenService _readDienstplanperiodenService;
    private readonly ReadAktiveWunschphaseService _readAktiveWunschphaseService;
    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly IMitarbeiterUrlaubszeitraumRepository _mitarbeiterUrlaubszeitraumRepository;

    public ReadDienstplanIndexViewModelService(
        ReadDienstplanperiodenService readDienstplanperiodenService,
        ReadAktiveWunschphaseService readAktiveWunschphaseService,
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        IMitarbeiterUrlaubszeitraumRepository mitarbeiterUrlaubszeitraumRepository)
    {
        _readDienstplanperiodenService = readDienstplanperiodenService
            ?? throw new ArgumentNullException(nameof(readDienstplanperiodenService));

        _readAktiveWunschphaseService = readAktiveWunschphaseService
            ?? throw new ArgumentNullException(nameof(readAktiveWunschphaseService));

        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService
            ?? throw new ArgumentNullException(nameof(readItwMitarbeiterprofileService));

        _mitarbeiterUrlaubszeitraumRepository = mitarbeiterUrlaubszeitraumRepository
            ?? throw new ArgumentNullException(nameof(mitarbeiterUrlaubszeitraumRepository));
    }

    public async Task<DienstplanIndexViewModel> ExecuteAsync(
        ReadDienstplanIndexViewModelQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.CurrentUser);

        var currentUser = query.CurrentUser;
        var istWachleiter = currentUser.Rolle == BereichsrolleCode.Wachleiter;

        var readPeriodenResult = await _readDienstplanperiodenService.ExecuteAsync(cancellationToken);

        ReadAktiveWunschphaseResult readWunschphaseResult;
        Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart beschaeftigungsart;

        if (istWachleiter)
        {
            readWunschphaseResult = ReadAktiveWunschphaseResult.Erfolg(null);
            beschaeftigungsart = Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Unbekannt;
        }
        else
        {
            readWunschphaseResult = await _readAktiveWunschphaseService.ExecuteAsync(
                currentUser.UserId,
                cancellationToken);

            beschaeftigungsart = await ErmittleBeschaeftigungsartAsync(
                currentUser.UserId,
                cancellationToken);
        }

        var viewModel = BaueDienstplanIndexViewModel(
            readPeriodenResult,
            readWunschphaseResult,
            currentUser,
            query.Monat,
            query.Jahr,
            query.WunschphaseDirektOeffnen,
            query.ModalAutomatischOeffnen,
            beschaeftigungsart);

        if (!istWachleiter)
        {
            await SperreUrlaubstageImMitarbeiterkalenderAsync(
                viewModel,
                currentUser.UserId,
                cancellationToken);
        }

        return viewModel;
    }

    private async Task<Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart> ErmittleBeschaeftigungsartAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var result = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Unbekannt;
        }

        var profil = result.Profile.FirstOrDefault(x =>
            string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

        return profil?.Beschaeftigungsart
            ?? Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Unbekannt;
    }

    private static DienstplanIndexViewModel BaueDienstplanIndexViewModel(
        ReadDienstplanperiodenResult readPeriodenResult,
        ReadAktiveWunschphaseResult readWunschphaseResult,
        CurrentUserContext currentUser,
        int monat,
        int jahr,
        bool wunschphaseDirektOeffnen,
        bool modalAutomatischOeffnen,
        Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart beschaeftigungsart)
    {
        var aktiveWunschphase = readWunschphaseResult.AktiveWunschphase;
        var wunschTage = aktiveWunschphase?.WunschTage ?? Array.Empty<int>();
        var nichtVerfuegbareTage = aktiveWunschphase?.NichtVerfuegbareTage ?? Array.Empty<int>();
        var istWachleiter = currentUser.Rolle == BereichsrolleCode.Wachleiter;
        var istFreelancer = !istWachleiter && beschaeftigungsart == Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer;
        var istFestangestellt = !istWachleiter && beschaeftigungsart == Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt;
        var istHonorarkraft = !istWachleiter && beschaeftigungsart == Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Honorarkraft;

        return new DienstplanIndexViewModel
        {
            Titel = istWachleiter ? "Dienstplan" : "Dienstwünsche",
            Beschreibung = istWachleiter
                ? "Übersicht über offene und vorhandene Dienstplanperioden. Öffnen Sie die Details, um in den Wachleiterkalender zu wechseln."
                : istHonorarkraft
                    ? "Honorarkräfte können Wunschtermine angeben und werden bei vorhandenen Wünschen in der Planung berücksichtigt."
                    : "Persönliche Wunschabgabe für den Bereich Intensivtransport.",
            IsSuccess = readPeriodenResult.IsSuccess && readWunschphaseResult.IsSuccess,
            ErrorMessage = readPeriodenResult.ErrorMessage ?? readWunschphaseResult.ErrorMessage,
            DarfPeriodeAnlegen = istWachleiter,
            Monat = monat is >= 1 and <= 12 ? monat : DateTime.Today.Month,
            Jahr = jahr is >= 2000 and <= 2100 ? jahr : DateTime.Today.Year,
            WunschphaseDirektOeffnen = wunschphaseDirektOeffnen,
            ModalAutomatischOeffnen = modalAutomatischOeffnen,
            MonatOptionen = BaueMonatOptionen(monat),
            JahrOptionen = BaueJahrOptionen(jahr),
            FreelancerDienstanzahlOptionen = new[]
            {
                new SelectListItem { Value = string.Empty, Text = "Bitte auswählen", Selected = !aktiveWunschphase?.FreelancerGewuenschteDienste.HasValue ?? true },
                new SelectListItem { Value = "1", Text = "1 Dienst", Selected = aktiveWunschphase?.FreelancerGewuenschteDienste == 1 },
                new SelectListItem { Value = "2", Text = "2 Dienste", Selected = aktiveWunschphase?.FreelancerGewuenschteDienste == 2 },
                new SelectListItem { Value = "3", Text = "3 Dienste", Selected = aktiveWunschphase?.FreelancerGewuenschteDienste == 3 }
            },
            Perioden = readPeriodenResult.Perioden
                .Select(x => new DienstplanPeriodeEintragViewModel
                {
                    Id = x.Id,
                    Bezeichnung = x.Bezeichnung,
                    Jahr = x.Jahr,
                    Monat = x.Monat,
                    WunschphaseIstOffen = x.WunschphaseIstOffen,
                    PlanIstFreigegeben = x.PlanIstFreigegeben,
                    PlanFreigegebenAm = x.PlanFreigegebenAm,
                    ErstelltAm = x.ErstelltAm
                })
                .ToArray(),
            HatOffeneWunschphase = !istWachleiter && aktiveWunschphase is not null,
            AktivePeriodeId = !istWachleiter ? aktiveWunschphase?.PeriodeId : null,
            AktivePeriodeBezeichnung = !istWachleiter ? aktiveWunschphase?.Bezeichnung ?? string.Empty : string.Empty,
            AktivePeriodeMonat = !istWachleiter ? aktiveWunschphase?.Monat ?? 0 : 0,
            AktivePeriodeJahr = !istWachleiter ? aktiveWunschphase?.Jahr ?? 0 : 0,
            BeschaeftigungsartText = istWachleiter ? string.Empty : FormatiereBeschaeftigungsart(beschaeftigungsart),
            IstFreelancer = istFreelancer,
            IstFestangestellt = istFestangestellt,
            AnzahlWunschTage = !istWachleiter ? wunschTage.Count : 0,
            AnzahlNichtVerfuegbareTage = !istWachleiter ? nichtVerfuegbareTage.Count : 0,
            FreelancerGewuenschteDienste = !istWachleiter ? aktiveWunschphase?.FreelancerGewuenschteDienste : null,
            KalenderWochen = !istWachleiter && aktiveWunschphase is not null
                ? BaueKalenderWochen(
                    aktiveWunschphase.Jahr,
                    aktiveWunschphase.Monat,
                    wunschTage,
                    nichtVerfuegbareTage)
                : Array.Empty<DienstplanKalenderWocheViewModel>()
        };
    }

    private static IReadOnlyList<DienstplanKalenderWocheViewModel> BaueKalenderWochen(
        int jahr,
        int monat,
        IReadOnlyList<int> wunschTage,
        IReadOnlyList<int> nichtVerfuegbareTage)
    {
        var ersteDatum = new DateOnly(jahr, monat, 1);
        var letzteDatum = ersteDatum.AddMonths(1).AddDays(-1);
        var startOffset = ((int)ersteDatum.DayOfWeek + 6) % 7;
        var kalenderStart = ersteDatum.AddDays(-startOffset);
        var endOffset = 6 - ((int)letzteDatum.DayOfWeek + 6) % 7;
        var kalenderEnde = letzteDatum.AddDays(endOffset);

        var heute = DateOnly.FromDateTime(DateTime.Today);
        var feiertage = MecklenburgVorpommernFeiertage.GetFeiertage(jahr);

        var wunschTageSet = wunschTage.ToHashSet();
        var nichtVerfuegbareTageSet = nichtVerfuegbareTage.ToHashSet();

        var wochen = new List<DienstplanKalenderWocheViewModel>();
        var aktuelleWoche = new List<DienstplanKalenderTagViewModel>();

        for (var datum = kalenderStart; datum <= kalenderEnde; datum = datum.AddDays(1))
        {
            var istImAktivenMonat = datum.Month == monat && datum.Year == jahr;
            var istFeiertag = feiertage.TryGetValue(datum, out var feiertagsname);
            var istWochenende = datum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

            aktuelleWoche.Add(new DienstplanKalenderTagViewModel
            {
                Datum = datum,
                IstImAktivenMonat = istImAktivenMonat,
                IstHeute = datum == heute,
                IstAlsWunschMarkiert = istImAktivenMonat && !istFeiertag && !istWochenende && wunschTageSet.Contains(datum.Day),
                IstAlsNichtVerfuegbarMarkiert = istImAktivenMonat && !istFeiertag && !istWochenende && nichtVerfuegbareTageSet.Contains(datum.Day),
                IstFeiertag = istImAktivenMonat && istFeiertag,
                IstWochenende = istImAktivenMonat && istWochenende,
                FeiertagsName = istImAktivenMonat && istFeiertag ? feiertagsname! : string.Empty,
                IstAuswaehlbar = istImAktivenMonat && !istFeiertag && !istWochenende
            });

            if (aktuelleWoche.Count == 7)
            {
                wochen.Add(new DienstplanKalenderWocheViewModel
                {
                    Tage = aktuelleWoche.ToArray()
                });

                aktuelleWoche = new List<DienstplanKalenderTagViewModel>();
            }
        }

        return wochen;
    }

    private async Task SperreUrlaubstageImMitarbeiterkalenderAsync(
        DienstplanIndexViewModel viewModel,
        string userId,
        CancellationToken cancellationToken)
    {
        if (!viewModel.HatOffeneWunschphase ||
            !viewModel.AktivePeriodeId.HasValue ||
            viewModel.AktivePeriodeJahr <= 0 ||
            viewModel.AktivePeriodeMonat <= 0 ||
            viewModel.KalenderWochen.Count == 0)
        {
            return;
        }

        var monatsStart = new DateOnly(viewModel.AktivePeriodeJahr, viewModel.AktivePeriodeMonat, 1);
        var monatsEnde = monatsStart.AddMonths(1).AddDays(-1);

        var urlaubszeitraeume = await _mitarbeiterUrlaubszeitraumRepository.GetAlleFuerBenutzerUndJahrAsync(
            userId,
            monatsStart.Year,
            cancellationToken);

        if (urlaubszeitraeume.Count == 0)
        {
            return;
        }

        var urlaubstage = new HashSet<DateOnly>();

        foreach (var zeitraum in urlaubszeitraeume.Where(x => x.IstAktiv))
        {
            var start = zeitraum.Von < monatsStart ? monatsStart : zeitraum.Von;
            var ende = zeitraum.Bis > monatsEnde ? monatsEnde : zeitraum.Bis;

            if (ende < start)
            {
                continue;
            }

            for (var datum = start; datum <= ende; datum = datum.AddDays(1))
            {
                urlaubstage.Add(datum);
            }
        }

        if (urlaubstage.Count == 0)
        {
            return;
        }

        foreach (var woche in viewModel.KalenderWochen)
        {
            foreach (var tag in woche.Tage.Where(x => x.IstImAktivenMonat && x.IstAuswaehlbar))
            {
                if (urlaubstage.Contains(tag.Datum))
                {
                    tag.IstAuswaehlbar = false;
                    tag.IstAlsWunschMarkiert = false;
                    tag.IstAlsNichtVerfuegbarMarkiert = false;
                    tag.IstUrlaub = true;
                }
            }
        }
    }

    private static IReadOnlyList<SelectListItem> BaueMonatOptionen(int ausgewaehlterMonat)
    {
        return Enumerable.Range(1, 12)
            .Select(monat => new SelectListItem
            {
                Value = monat.ToString(),
                Text = DeutscheKultur.DateTimeFormat.GetMonthName(monat),
                Selected = monat == ausgewaehlterMonat
            })
            .ToArray();
    }

    private static IReadOnlyList<SelectListItem> BaueJahrOptionen(int ausgewaehltesJahr)
    {
        var basisJahr = DateTime.Today.Year;

        return Enumerable.Range(basisJahr - 1, 5)
            .Select(jahr => new SelectListItem
            {
                Value = jahr.ToString(),
                Text = jahr.ToString(),
                Selected = jahr == ausgewaehltesJahr
            })
            .ToArray();
    }

    private static string FormatiereBeschaeftigungsart(
        Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart beschaeftigungsart)
    {
        return beschaeftigungsart switch
        {
            Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer => "Freelancer",
            Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt => "Festangestellt",
            Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Honorarkraft => "Honorarkraft",
            _ => "Unbekannt"
        };
    }
}