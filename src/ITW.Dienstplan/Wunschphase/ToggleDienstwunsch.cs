using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Kalender;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Wunschphase;

public sealed record ToggleDienstwunschCommand(
    Guid PeriodeId,
    string UserId,
    DateOnly WunschDatum,
    DienstwunschTyp WunschTyp);

public sealed class ToggleDienstwunschResult
{
    private ToggleDienstwunschResult(
        bool isSuccess,
        string? errorMessage,
        bool istJetztGesetzt)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        IstJetztGesetzt = istJetztGesetzt;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public bool IstJetztGesetzt { get; }

    public static ToggleDienstwunschResult Erfolg(bool istJetztGesetzt)
        => new(true, null, istJetztGesetzt);

    public static ToggleDienstwunschResult Fehler(string message)
        => new(false, message, false);
}

public sealed class ToggleDienstwunschService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IDienstwunschRepository _dienstwunschRepository;

    public ToggleDienstwunschService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IDienstwunschRepository dienstwunschRepository)
    {
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository
            ?? throw new ArgumentNullException(nameof(dienstplanPeriodeRepository));

        _dienstwunschRepository = dienstwunschRepository
            ?? throw new ArgumentNullException(nameof(dienstwunschRepository));
    }

    public async Task<ToggleDienstwunschResult> ExecuteAsync(
        ToggleDienstwunschCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PeriodeId == Guid.Empty)
        {
            return ToggleDienstwunschResult.Fehler("Die Dienstplanperiode ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return ToggleDienstwunschResult.Fehler("Die UserId ist erforderlich.");
        }

        if (!Enum.IsDefined(typeof(DienstwunschTyp), command.WunschTyp))
        {
            return ToggleDienstwunschResult.Fehler("Der ausgewählte Wunschtyp ist ungültig.");
        }

        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(
            command.PeriodeId,
            cancellationToken);

        if (periode is null)
        {
            return ToggleDienstwunschResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        if (!periode.WunschphaseIstOffen)
        {
            return ToggleDienstwunschResult.Fehler("Für diese Dienstplanperiode ist die Wunschphase nicht offen.");
        }

        if (command.WunschDatum.Year != periode.Jahr || command.WunschDatum.Month != periode.Monat)
        {
            return ToggleDienstwunschResult.Fehler("Der ausgewählte Tag gehört nicht zur aktiven Dienstplanperiode.");
        }

        if (command.WunschDatum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return ToggleDienstwunschResult.Fehler(
                $"Der {command.WunschDatum:dd.MM.yyyy} liegt an einem Wochenende und kann nicht ausgewählt werden.");
        }

        if (MecklenburgVorpommernFeiertage.TryGetFeiertagsname(command.WunschDatum, out var feiertagsname))
        {
            return ToggleDienstwunschResult.Fehler(
                $"Der {command.WunschDatum:dd.MM.yyyy} ist ein gesetzlicher Feiertag in Mecklenburg-Vorpommern ({feiertagsname}) und kann nicht ausgewählt werden.");
        }

        var existing = await _dienstwunschRepository.GetAsync(
            command.PeriodeId,
            command.UserId.Trim(),
            command.WunschDatum,
            cancellationToken);

        if (existing is null)
        {
            var neuerWunsch = new Dienstwunsch(
                Guid.NewGuid(),
                command.PeriodeId,
                command.UserId.Trim(),
                command.WunschDatum,
                command.WunschTyp,
                DateTimeOffset.UtcNow);

            await _dienstwunschRepository.AddAsync(neuerWunsch, cancellationToken);
            await _dienstwunschRepository.SaveChangesAsync(cancellationToken);

            return ToggleDienstwunschResult.Erfolg(true);
        }

        if (existing.WunschTyp == command.WunschTyp)
        {
            _dienstwunschRepository.Remove(existing);
            await _dienstwunschRepository.SaveChangesAsync(cancellationToken);

            return ToggleDienstwunschResult.Erfolg(false);
        }

        existing.AktualisiereWunschTyp(command.WunschTyp);
        await _dienstwunschRepository.SaveChangesAsync(cancellationToken);

        return ToggleDienstwunschResult.Erfolg(true);
    }
}