using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Entities;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.Urlaub;

public sealed class SaveMitarbeiterUrlaubsanspruchCommand
{
    public string UserId { get; init; } = string.Empty;

    public int Jahr { get; init; }

    public int Anspruchstage { get; init; }

    public int Uebertragstage { get; init; }

    public string? Bemerkung { get; init; }
}

public sealed class SaveMitarbeiterUrlaubsanspruchResult
{
    private SaveMitarbeiterUrlaubsanspruchResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SaveMitarbeiterUrlaubsanspruchResult Erfolg()
        => new(true, null);

    public static SaveMitarbeiterUrlaubsanspruchResult Fehler(string message)
        => new(false, message);
}

public sealed class SaveMitarbeiterUrlaubsanspruchService
{
    private readonly IMitarbeiterUrlaubsanspruchRepository _repository;
    private readonly ILogger<SaveMitarbeiterUrlaubsanspruchService> _logger;

    public SaveMitarbeiterUrlaubsanspruchService(
        IMitarbeiterUrlaubsanspruchRepository repository,
        ILogger<SaveMitarbeiterUrlaubsanspruchService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SaveMitarbeiterUrlaubsanspruchResult> ExecuteAsync(
        SaveMitarbeiterUrlaubsanspruchCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(SaveMitarbeiterUrlaubsanspruchService));

        if (command is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Command ist null", nameof(SaveMitarbeiterUrlaubsanspruchService));
            return SaveMitarbeiterUrlaubsanspruchResult.Fehler("Die Anfrage ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(SaveMitarbeiterUrlaubsanspruchService));
            return SaveMitarbeiterUrlaubsanspruchResult.Fehler("Es wurde kein Mitarbeiter ausgewählt.");
        }

        if (command.Jahr is < 2000 or > 2100)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Jahr ungültig", nameof(SaveMitarbeiterUrlaubsanspruchService));
            return SaveMitarbeiterUrlaubsanspruchResult.Fehler("Das ausgewählte Jahr ist ungültig.");
        }

        if (command.Anspruchstage is < 0 or > 366)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Anspruchstage ungültig", nameof(SaveMitarbeiterUrlaubsanspruchService));
            return SaveMitarbeiterUrlaubsanspruchResult.Fehler("Die Anspruchstage sind ungültig.");
        }

        if (command.Uebertragstage is < 0 or > 366)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Uebertragstage ungültig", nameof(SaveMitarbeiterUrlaubsanspruchService));
            return SaveMitarbeiterUrlaubsanspruchResult.Fehler("Die Übertragstage sind ungültig.");
        }

        var bestehend = await _repository.GetAsync(
            command.UserId,
            command.Jahr,
            cancellationToken);

        var entity = bestehend ?? new MitarbeiterUrlaubsanspruch
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId.Trim(),
            Jahr = command.Jahr,
            ErstelltAmUtc = DateTimeOffset.UtcNow
        };

        entity.Anspruchstage = command.Anspruchstage;
        entity.Uebertragstage = command.Uebertragstage;
        entity.Bemerkung = string.IsNullOrWhiteSpace(command.Bemerkung)
            ? null
            : command.Bemerkung.Trim();

        entity.AktualisiertAmUtc = DateTimeOffset.UtcNow;

        await _repository.UpsertAsync(entity, cancellationToken);

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SaveMitarbeiterUrlaubsanspruchService));
        return SaveMitarbeiterUrlaubsanspruchResult.Erfolg();
    }
}