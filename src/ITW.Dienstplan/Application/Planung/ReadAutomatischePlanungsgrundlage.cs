using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Domain.Personnel.Enums;
using ITW.Domain.Personnel.Qualifications;

namespace ITW.Dienstplan.Application.Planung;

public sealed class AutomatischePlanungsgrundlageMitarbeiter
{
    public string UserId { get; init; } = string.Empty;

    public string AnzeigeName { get; init; } = string.Empty;

    public MitarbeiterBeschaeftigungsart Beschaeftigungsart { get; init; }

    public string HauptqualifikationCode { get; init; } = string.Empty;

    public string HauptqualifikationBezeichnung { get; init; } = string.Empty;

    public IReadOnlyList<DateOnly> WunschTage { get; init; } = Array.Empty<DateOnly>();

    public IReadOnlyList<DateOnly> BlockierteTage { get; init; } = Array.Empty<DateOnly>();

    public int? FreelancerGewuenschteDienste { get; init; }

    public bool IstFuerAutomatischePlanungGeeignet { get; init; }

    public string? Ausschlussgrund { get; init; }

    public bool IstArzt => string.Equals(
        HauptqualifikationCode,
        ItwQualifikationsCodes.Arzt,
        StringComparison.OrdinalIgnoreCase);

    public bool IstNotfallsanitaeter => string.Equals(
        HauptqualifikationCode,
        ItwQualifikationsCodes.Notfallsanitaeter,
        StringComparison.OrdinalIgnoreCase);

    public bool IstFreelancer => Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Freelancer;

    public bool IstFestangestellt => Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Festangestellt;
}

public sealed class ReadAutomatischePlanungsgrundlageResult
{
    private ReadAutomatischePlanungsgrundlageResult(
        bool isSuccess,
        string? errorMessage,
        Guid periodeId,
        string periodenBezeichnung,
        int jahr,
        int monat,
        IReadOnlyList<AutomatischePlanungsgrundlageMitarbeiter> mitarbeiter)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        PeriodeId = periodeId;
        PeriodenBezeichnung = periodenBezeichnung;
        Jahr = jahr;
        Monat = monat;
        Mitarbeiter = mitarbeiter;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public Guid PeriodeId { get; }

    public string PeriodenBezeichnung { get; }

    public int Jahr { get; }

    public int Monat { get; }

    public IReadOnlyList<AutomatischePlanungsgrundlageMitarbeiter> Mitarbeiter { get; }

    public int AnzahlGeeigneteAerzte => Mitarbeiter.Count(x => x.IstFuerAutomatischePlanungGeeignet && x.IstArzt);

    public int AnzahlGeeigneteNotfallsanitaeter => Mitarbeiter.Count(x => x.IstFuerAutomatischePlanungGeeignet && x.IstNotfallsanitaeter);

    public static ReadAutomatischePlanungsgrundlageResult Erfolg(
        Guid periodeId,
        string periodenBezeichnung,
        int jahr,
        int monat,
        IReadOnlyList<AutomatischePlanungsgrundlageMitarbeiter> mitarbeiter)
        => new(
            true,
            null,
            periodeId,
            periodenBezeichnung,
            jahr,
            monat,
            mitarbeiter);

    public static ReadAutomatischePlanungsgrundlageResult Fehler(string message)
        => new(
            false,
            message,
            Guid.Empty,
            string.Empty,
            0,
            0,
            Array.Empty<AutomatischePlanungsgrundlageMitarbeiter>());
}

public sealed class ReadAutomatischePlanungsgrundlageService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IDienstplanMitarbeiterPlanungsRepository _dienstplanMitarbeiterPlanungsRepository;
    private readonly IDienstwunschRepository _dienstwunschRepository;
    private readonly IFreelancerMonatswunschRepository _freelancerMonatswunschRepository;

    public ReadAutomatischePlanungsgrundlageService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IDienstplanMitarbeiterPlanungsRepository dienstplanMitarbeiterPlanungsRepository,
        IDienstwunschRepository dienstwunschRepository,
        IFreelancerMonatswunschRepository freelancerMonatswunschRepository)
    {
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository
            ?? throw new ArgumentNullException(nameof(dienstplanPeriodeRepository));

        _dienstplanMitarbeiterPlanungsRepository = dienstplanMitarbeiterPlanungsRepository
            ?? throw new ArgumentNullException(nameof(dienstplanMitarbeiterPlanungsRepository));

        _dienstwunschRepository = dienstwunschRepository
            ?? throw new ArgumentNullException(nameof(dienstwunschRepository));

        _freelancerMonatswunschRepository = freelancerMonatswunschRepository
            ?? throw new ArgumentNullException(nameof(freelancerMonatswunschRepository));
    }

    public async Task<ReadAutomatischePlanungsgrundlageResult> ExecuteAsync(
        Guid periodeId,
        CancellationToken cancellationToken = default)
    {
        if (periodeId == Guid.Empty)
        {
            return ReadAutomatischePlanungsgrundlageResult.Fehler("Die Dienstplanperiode ist ungültig.");
        }

        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(
            periodeId,
            cancellationToken);

        if (periode is null)
        {
            return ReadAutomatischePlanungsgrundlageResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        var mitarbeiterStammdaten = await _dienstplanMitarbeiterPlanungsRepository.GetAktivePlanungsmitarbeiterAsync(cancellationToken);
        var wuensche = await _dienstwunschRepository.GetAlleFuerPeriodeAsync(periode.Id, cancellationToken);
        var freelancerMonatswuensche = await _freelancerMonatswunschRepository.GetAlleFuerPeriodeAsync(periode.Id, cancellationToken);

        var wuenscheProMitarbeiter = wuensche
            .GroupBy(x => x.UserId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.OrderBy(eintrag => eintrag.WunschDatum).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        var freelancerMonatswunschLookup = freelancerMonatswuensche.ToDictionary(
            x => x.UserId,
            x => x.GewuenschteDienste,
            StringComparer.OrdinalIgnoreCase);

        var mitarbeiter = mitarbeiterStammdaten
            .Select(stammdaten =>
            {
                wuenscheProMitarbeiter.TryGetValue(stammdaten.UserId, out var mitarbeiterWuensche);
                freelancerMonatswunschLookup.TryGetValue(stammdaten.UserId, out var freelancerGewuenschteDienste);

                var wunschTage = (mitarbeiterWuensche ?? Array.Empty<Dienstwunsch>())
                    .Where(x => x.WunschTyp == DienstwunschTyp.Wunsch)
                    .Select(x => x.WunschDatum)
                    .OrderBy(x => x)
                    .ToArray();

                var blockierteTage = (mitarbeiterWuensche ?? Array.Empty<Dienstwunsch>())
                    .Where(x => x.WunschTyp == DienstwunschTyp.NichtVerfuegbar)
                    .Select(x => x.WunschDatum)
                    .OrderBy(x => x)
                    .ToArray();

                var (istGeeignet, ausschlussgrund) = BestimmePlanbarkeit(
                    stammdaten,
                    wunschTage,
                    freelancerMonatswunschLookup.ContainsKey(stammdaten.UserId)
                        ? freelancerGewuenschteDienste
                        : null);

                return new AutomatischePlanungsgrundlageMitarbeiter
                {
                    UserId = stammdaten.UserId,
                    AnzeigeName = stammdaten.AnzeigeName,
                    Beschaeftigungsart = stammdaten.Beschaeftigungsart,
                    HauptqualifikationCode = stammdaten.HauptqualifikationCode,
                    HauptqualifikationBezeichnung = stammdaten.HauptqualifikationBezeichnung,
                    WunschTage = wunschTage,
                    BlockierteTage = blockierteTage,
                    FreelancerGewuenschteDienste = freelancerMonatswunschLookup.ContainsKey(stammdaten.UserId)
                        ? freelancerGewuenschteDienste
                        : null,
                    IstFuerAutomatischePlanungGeeignet = istGeeignet,
                    Ausschlussgrund = ausschlussgrund
                };
            })
            .OrderBy(x => x.HauptqualifikationBezeichnung, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ReadAutomatischePlanungsgrundlageResult.Erfolg(
            periode.Id,
            periode.Bezeichnung,
            periode.Jahr,
            periode.Monat,
            mitarbeiter);
    }

    private static (bool IstGeeignet, string? Ausschlussgrund) BestimmePlanbarkeit(
    DienstplanMitarbeiterPlanungsstammdaten stammdaten,
    IReadOnlyList<DateOnly> wunschTage,
    int? freelancerGewuenschteDienste)
    {
        if (stammdaten.IstGesperrt)
        {
            return (false, "Benutzerkonto ist gesperrt.");
        }

        if (!stammdaten.HatStammdatenprofil)
        {
            return (false, "Allgemeine Stammdaten fehlen.");
        }

        if (!stammdaten.HatItwProfil)
        {
            return (false, "ITW-Mitarbeiterprofil fehlt.");
        }

        if (stammdaten.Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Unbekannt)
        {
            return (false, "Beschäftigungsart ist nicht gepflegt.");
        }

        if (!string.Equals(stammdaten.HauptqualifikationCode, ItwQualifikationsCodes.Arzt, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(stammdaten.HauptqualifikationCode, ItwQualifikationsCodes.Notfallsanitaeter, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Hauptqualifikation ist nicht planungsfähig.");
        }

        if (stammdaten.Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Freelancer)
        {
            if (!freelancerGewuenschteDienste.HasValue)
            {
                return (false, "Freelancer-Monatswunsch fehlt.");
            }

            if (wunschTage.Count == 0)
            {
                return (false, "Freelancer hat keine Wunschtermine abgegeben.");
            }
        }

        if (stammdaten.Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Honorarkraft)
        {
            if (wunschTage.Count == 0)
            {
                return (false, "Honorarkraft hat keine Wunschtermine abgegeben und bleibt daher nur manuell planbar.");
            }
        }

        return (true, null);
    }
}