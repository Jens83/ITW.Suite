using ITW.Application.Abstractions.DateTime;
using ITW.Application.Personnel.ProfileQueries;
using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Kalender;
using ITW.Dienstplan.Application.Perioden;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Shared;
using ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;
using System.Globalization;

namespace ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Read;

public sealed record ReadSichtbarerDienstplanViewModelQuery(string AktuellerUserId, bool IstWachleiter = false);

public sealed class ReadSichtbarerDienstplanViewModelService
{
    private static readonly CultureInfo DeutscheKultur = CultureInfo.GetCultureInfo("de-DE");

    private readonly ReadDienstplanperiodenService _readDienstplanperiodenService;
    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly IDienstbesetzungsAusfallRepository _dienstbesetzungsAusfallRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReadSichtbarerDienstplanViewModelService(
        ReadDienstplanperiodenService readDienstplanperiodenService,
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        IGeplanterDienstTagRepository geplanterDienstTagRepository,
        IDienstbesetzungsAusfallRepository dienstbesetzungsAusfallRepository,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(readDienstplanperiodenService);
        _readDienstplanperiodenService = readDienstplanperiodenService;

        ArgumentNullException.ThrowIfNull(readItwMitarbeiterprofileService);
        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService;

        ArgumentNullException.ThrowIfNull(geplanterDienstTagRepository);
        _geplanterDienstTagRepository = geplanterDienstTagRepository;

        ArgumentNullException.ThrowIfNull(dienstbesetzungsAusfallRepository);
        _dienstbesetzungsAusfallRepository = dienstbesetzungsAusfallRepository;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<SichtbarerDienstplanViewModel> ExecuteAsync(
        ReadSichtbarerDienstplanViewModelQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var readPeriodenResult = await _readDienstplanperiodenService.ExecuteAsync(cancellationToken);

        var freigegebenePerioden = readPeriodenResult.Perioden
            .Where(x => !x.WunschphaseIstOffen && x.PlanIstFreigegeben)
            .OrderBy(x => x.Jahr)
            .ThenBy(x => x.Monat)
            .ToArray();

        var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);

        var isSuccess = readPeriodenResult.IsSuccess && mitarbeiterResult.IsSuccess;
        var errorMessage = readPeriodenResult.ErrorMessage ?? mitarbeiterResult.ErrorMessage;

        if (freigegebenePerioden.Length == 0)
        {
            return new SichtbarerDienstplanViewModel
            {
                Titel = "Veröffentlichter Plan",
                Beschreibung = "Hier sehen Mitarbeiter nur freigegebene und geschlossene Dienstpläne.",
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                HatFreigegebenePlaene = false,
                Abschnitte = Array.Empty<SichtbarerPlanAbschnittViewModel>()
            };
        }

        var profilLookup = mitarbeiterResult.IsSuccess
            ? mitarbeiterResult.Profile.ToDictionary(x => x.UserId, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, ItwMitarbeiterprofilUebersichtDto>(StringComparer.OrdinalIgnoreCase);

        var heute = _dateTimeProvider.Today;
        var aktuellerPlan = ErmittleAktuellenFreigegebenenPlan(freigegebenePerioden, heute);
        var neuerPlan = ErmittleNeuenFreigegebenenPlan(freigegebenePerioden, heute);

        var abschnitte = new List<SichtbarerPlanAbschnittViewModel>();

        if (aktuellerPlan is not null)
        {
            abschnitte.Add(await BaueSichtbarenPlanAbschnittAsync(
                "Aktueller Plan",
                aktuellerPlan,
                query.AktuellerUserId,
                query.IstWachleiter,
                profilLookup,
                cancellationToken));
        }

        if (neuerPlan is not null &&
            (aktuellerPlan is null || neuerPlan.Id != aktuellerPlan.Id))
        {
            abschnitte.Add(await BaueSichtbarenPlanAbschnittAsync(
                "Neuer Plan",
                neuerPlan,
                query.AktuellerUserId,
                query.IstWachleiter,
                profilLookup,
                cancellationToken));
        }

        return new SichtbarerDienstplanViewModel
        {
            Titel = "Veröffentlichter Plan",
            Beschreibung = "Hier sehen Mitarbeiter nur freigegebene und geschlossene Dienstpläne.",
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage,
            HatFreigegebenePlaene = abschnitte.Count > 0,
            Abschnitte = abschnitte
        };
    }

    private async Task<SichtbarerPlanAbschnittViewModel> BaueSichtbarenPlanAbschnittAsync(
        string ueberschrift,
        DienstplanPeriodeListenEintrag periode,
        string aktuellerUserId,
        bool istWachleiter,
        IReadOnlyDictionary<string, ItwMitarbeiterprofilUebersichtDto> profilLookup,
        CancellationToken cancellationToken)
    {
        var planungen = await _geplanterDienstTagRepository.GetAlleFuerPeriodeAsync(
            periode.Id,
            cancellationToken);

        var planungLookup = planungen.ToDictionary(x => x.DienstDatum);

        var ausfaelle = await _dienstbesetzungsAusfallRepository.GetAlleFuerPeriodeAsync(
            periode.Id,
            cancellationToken);

        var ausfallLookup = ausfaelle
            .GroupBy(x => x.DienstDatum)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<GeplanterDienstTagAusfall>)x.ToArray());

        var heute = _dateTimeProvider.Today;
        var feiertage = MecklenburgVorpommernFeiertage.GetFeiertage(periode.Jahr);
        var tageImMonat = DateTime.DaysInMonth(periode.Jahr, periode.Monat);

        var tage = Enumerable.Range(1, tageImMonat)
            .Select(tagNummer =>
            {
                var datum = new DateOnly(periode.Jahr, periode.Monat, tagNummer);

                planungLookup.TryGetValue(datum, out var planung);
                ausfallLookup.TryGetValue(datum, out var tagesAusfaelle);
                tagesAusfaelle ??= Array.Empty<GeplanterDienstTagAusfall>();

                var istFeiertag = feiertage.TryGetValue(datum, out var feiertagsName);
                var istWochenende = datum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

                var slots = planung is null
                    ? Array.Empty<SichtbarerPlanSlotViewModel>()
                    : BaueSichtbarePlanSlots(
                        planung,
                        tagesAusfaelle,
                        profilLookup);

                // Sondereinsätze an Wochenenden/Feiertagen sind nur für Wachleiter
                // und direkt betroffene Mitarbeiter sichtbar.
                if ((istWochenende || istFeiertag) && !istWachleiter && planung is not null)
                {
                    var aktuellerUserIstBetroffen =
                        string.Equals(planung.ArztUserId, aktuellerUserId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(planung.Notfallsanitaeter1UserId, aktuellerUserId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(planung.Notfallsanitaeter2UserId, aktuellerUserId, StringComparison.OrdinalIgnoreCase);

                    if (!aktuellerUserIstBetroffen)
                    {
                        slots = Array.Empty<SichtbarerPlanSlotViewModel>();
                    }
                }

                var eigeneSlots = slots
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.UserId) &&
                        string.Equals(x.UserId, aktuellerUserId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                return new SichtbarerPlanTagViewModel
                {
                    Datum = datum,
                    DatumAnzeige = datum.ToDateTime(TimeOnly.MinValue).ToString("dd.MM.yyyy", DeutscheKultur),
                    WochentagKurz = datum.ToDateTime(TimeOnly.MinValue).ToString("ddd", DeutscheKultur),
                    IstHeute = datum == heute,
                    IstWochenende = istWochenende,
                    IstFeiertag = istFeiertag,
                    FeiertagsName = istFeiertag ? feiertagsName! : string.Empty,
                    IstTagDesBenutzers = eigeneSlots.Length > 0,
                    EigenerDienstIstVertretung = eigeneSlots.Any(x => x.IstVertretung),
                    Slots = slots
                };
            })
            .ToArray();

        return new SichtbarerPlanAbschnittViewModel
        {
            Ueberschrift = ueberschrift,
            PeriodeId = periode.Id,
            PeriodeBezeichnung = periode.Bezeichnung,
            Jahr = periode.Jahr,
            Monat = periode.Monat,
            Tage = tage
        };
    }

    private static DienstplanPeriodeListenEintrag? ErmittleAktuellenFreigegebenenPlan(
        IReadOnlyList<DienstplanPeriodeListenEintrag> freigegebenePerioden,
        DateOnly heute)
    {
        var aktuellerSchluessel = heute.Year * 100 + heute.Month;

        return freigegebenePerioden
            .Where(x => ErzeugePeriodenSchluessel(x) <= aktuellerSchluessel)
            .OrderBy(x => x.Jahr)
            .ThenBy(x => x.Monat)
            .LastOrDefault();
    }

    private static DienstplanPeriodeListenEintrag? ErmittleNeuenFreigegebenenPlan(
        IReadOnlyList<DienstplanPeriodeListenEintrag> freigegebenePerioden,
        DateOnly heute)
    {
        var aktuellerSchluessel = heute.Year * 100 + heute.Month;

        return freigegebenePerioden
            .Where(x => ErzeugePeriodenSchluessel(x) > aktuellerSchluessel)
            .OrderBy(x => x.Jahr)
            .ThenBy(x => x.Monat)
            .FirstOrDefault();
    }

    private static int ErzeugePeriodenSchluessel(DienstplanPeriodeListenEintrag periode)
        => periode.Jahr * 100 + periode.Monat;

    private static IReadOnlyList<SichtbarerPlanSlotViewModel> BaueSichtbarePlanSlots(
        GeplanterDienstTag geplanterDienstTag,
        IReadOnlyList<GeplanterDienstTagAusfall> tagesAusfaelle,
        IReadOnlyDictionary<string, ItwMitarbeiterprofilUebersichtDto> profilLookup)
    {
        var arztAusfall = tagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Arzt);
        var nfs1Ausfall = tagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Notfallsanitaeter1);
        var nfs2Ausfall = tagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Notfallsanitaeter2);

        return new[]
        {
            BaueSichtbarenPlanSlot("Arzt", geplanterDienstTag.ArztUserId, arztAusfall, profilLookup),
            BaueSichtbarenPlanSlot("Notfallsanitäter 1", geplanterDienstTag.Notfallsanitaeter1UserId, nfs1Ausfall, profilLookup),
            BaueSichtbarenPlanSlot("Notfallsanitäter 2", geplanterDienstTag.Notfallsanitaeter2UserId, nfs2Ausfall, profilLookup)
        };
    }

    private static SichtbarerPlanSlotViewModel BaueSichtbarenPlanSlot(
        string slotBezeichnung,
        string? aktuelleUserId,
        GeplanterDienstTagAusfall? ausfall,
        IReadOnlyDictionary<string, ItwMitarbeiterprofilUebersichtDto> profilLookup)
    {
        if (string.IsNullOrWhiteSpace(aktuelleUserId))
        {
            return new SichtbarerPlanSlotViewModel
            {
                SlotBezeichnung = slotBezeichnung,
                UserId = null,
                AnzeigeName = "Noch offen",
                Hauptqualifikation = string.Empty,
                IstOffen = true,
                IstVertretung = false,
                VertretungFuerAnzeigeName = null
            };
        }

        var aktuelleBesetzung = DienstplanPlanungsHelper.ErmittleGeplantenMitarbeiter(profilLookup, aktuelleUserId);

        var istVertretung = ausfall is not null &&
                            !string.IsNullOrWhiteSpace(ausfall.VertretungsUserId) &&
                            string.Equals(ausfall.VertretungsUserId, aktuelleUserId, StringComparison.OrdinalIgnoreCase);

        string? vertretungFuerAnzeigeName = null;

        if (istVertretung)
        {
            vertretungFuerAnzeigeName = DienstplanPlanungsHelper.ErmittleGeplantenMitarbeiter(
                profilLookup,
                ausfall!.UrspruenglichGeplanterUserId).AnzeigeName;
        }

        return new SichtbarerPlanSlotViewModel
        {
            SlotBezeichnung = slotBezeichnung,
            UserId = aktuelleUserId,
            AnzeigeName = aktuelleBesetzung.AnzeigeName,
            Hauptqualifikation = aktuelleBesetzung.Hauptqualifikation,
            IstOffen = false,
            IstVertretung = istVertretung,
            VertretungFuerAnzeigeName = vertretungFuerAnzeigeName
        };
    }
}