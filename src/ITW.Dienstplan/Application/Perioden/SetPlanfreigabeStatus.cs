using System.Text;
using ITW.Dienstplan.Application.Contracts;
using ITW.Domain.Kalender;

namespace ITW.Dienstplan.Application.Perioden;

public sealed record SetPlanfreigabeStatusCommand(
    Guid PeriodeId,
    bool Freigeben,
    string BearbeitetVonUserId);

public sealed class SetPlanfreigabeStatusResult
{
    private SetPlanfreigabeStatusResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SetPlanfreigabeStatusResult Erfolg()
        => new(true, null);

    public static SetPlanfreigabeStatusResult Fehler(string message)
        => new(false, message);
}

public sealed class SetPlanfreigabeStatusService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;

    public SetPlanfreigabeStatusService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IGeplanterDienstTagRepository geplanterDienstTagRepository)
    {
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository
            ?? throw new ArgumentNullException(nameof(dienstplanPeriodeRepository));

        _geplanterDienstTagRepository = geplanterDienstTagRepository
            ?? throw new ArgumentNullException(nameof(geplanterDienstTagRepository));
    }

    public async Task<SetPlanfreigabeStatusResult> ExecuteAsync(
        SetPlanfreigabeStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PeriodeId == Guid.Empty)
        {
            return SetPlanfreigabeStatusResult.Fehler("Die Dienstplanperiode ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.BearbeitetVonUserId))
        {
            return SetPlanfreigabeStatusResult.Fehler("Die UserId des Bearbeiters ist erforderlich.");
        }

        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(
            command.PeriodeId,
            cancellationToken);

        if (periode is null)
        {
            return SetPlanfreigabeStatusResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        if (command.Freigeben)
        {
            if (periode.WunschphaseIstOffen)
            {
                return SetPlanfreigabeStatusResult.Fehler(
                    "Der Plan kann erst freigegeben werden, wenn die Wunschphase geschlossen ist.");
            }

            var planbareTage = ErmittlePlanbareTage(periode.Jahr, periode.Monat);

            if (planbareTage.Count == 0)
            {
                return SetPlanfreigabeStatusResult.Fehler(
                    "Für die Periode konnten keine planbaren Werktage ermittelt werden.");
            }

            var geplanteDiensttage = await _geplanterDienstTagRepository.GetAlleFuerPeriodeAsync(
                periode.Id,
                cancellationToken);

            var geplanteDiensttageLookup = geplanteDiensttage.ToDictionary(x => x.DienstDatum);

            var fehlendeTage = new List<DateOnly>();
            var unvollstaendigeTage = new List<DateOnly>();

            foreach (var datum in planbareTage)
            {
                if (!geplanteDiensttageLookup.TryGetValue(datum, out var geplanterDienstTag))
                {
                    fehlendeTage.Add(datum);
                    continue;
                }

                if (!geplanterDienstTag.HatVollstaendigeBesatzung)
                {
                    unvollstaendigeTage.Add(datum);
                }
            }

            if (fehlendeTage.Count > 0 || unvollstaendigeTage.Count > 0)
            {
                return SetPlanfreigabeStatusResult.Fehler(
                    BaueFreigabeFehlertext(fehlendeTage, unvollstaendigeTage));
            }

            periode.PlanFreigeben(
                command.BearbeitetVonUserId,
                DateTimeOffset.UtcNow);
        }
        else
        {
            periode.PlanFreigabeZuruecknehmen();
        }

        await _dienstplanPeriodeRepository.SaveChangesAsync(cancellationToken);

        return SetPlanfreigabeStatusResult.Erfolg();
    }

    private static IReadOnlyList<DateOnly> ErmittlePlanbareTage(int jahr, int monat)
    {
        var feiertage = MecklenburgVorpommernFeiertage.GetFeiertage(jahr);
        var start = new DateOnly(jahr, monat, 1);
        var ende = start.AddMonths(1).AddDays(-1);

        var tage = new List<DateOnly>();

        for (var datum = start; datum <= ende; datum = datum.AddDays(1))
        {
            if (datum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            if (feiertage.ContainsKey(datum))
            {
                continue;
            }

            tage.Add(datum);
        }

        return tage;
    }

    private static string BaueFreigabeFehlertext(
        IReadOnlyList<DateOnly> fehlendeTage,
        IReadOnlyList<DateOnly> unvollstaendigeTage)
    {
        if (fehlendeTage.Count > 0 && unvollstaendigeTage.Count == 0)
        {
            return
                $"Der Plan kann erst freigegeben werden, wenn für alle planbaren Werktage eine Besatzung gespeichert wurde. Offen: {FormatiereDatumsListe(fehlendeTage)}.";
        }

        if (fehlendeTage.Count == 0 && unvollstaendigeTage.Count > 0)
        {
            return
                $"Der Plan kann erst freigegeben werden, wenn alle planbaren Werktage vollständig besetzt sind. Unterbesetzt: {FormatiereDatumsListe(unvollstaendigeTage)}.";
        }

        var builder = new StringBuilder();
        builder.Append("Der Plan kann erst freigegeben werden, wenn alle planbaren Werktage vollständig vorbereitet sind. ");

        if (fehlendeTage.Count > 0)
        {
            builder.Append("Ohne gespeicherte Besatzung: ");
            builder.Append(FormatiereDatumsListe(fehlendeTage));
            builder.Append(". ");
        }

        if (unvollstaendigeTage.Count > 0)
        {
            builder.Append("Nicht vollständig besetzt: ");
            builder.Append(FormatiereDatumsListe(unvollstaendigeTage));
            builder.Append('.');
        }

        return builder.ToString().Trim();
    }

    private static string FormatiereDatumsListe(IReadOnlyList<DateOnly> daten)
    {
        const int maxAnzahl = 5;

        var sortierteDaten = daten
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        var sichtbareDaten = sortierteDaten
            .Take(maxAnzahl)
            .Select(x => x.ToString("dd.MM.yyyy"))
            .ToArray();

        if (sortierteDaten.Length <= maxAnzahl)
        {
            return string.Join(", ", sichtbareDaten);
        }

        var rest = sortierteDaten.Length - maxAnzahl;
        return $"{string.Join(", ", sichtbareDaten)} und {rest} weitere";
    }
}