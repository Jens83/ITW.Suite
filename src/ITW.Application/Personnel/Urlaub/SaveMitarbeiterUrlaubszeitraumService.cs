using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Entities;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.Urlaub;

public sealed class SaveMitarbeiterUrlaubszeitraumCommand
{
    public Guid? Id { get; init; }

    public string UserId { get; init; } = string.Empty;

    public DateOnly Von { get; init; }

    public DateOnly Bis { get; init; }

    public string? Notiz { get; init; }
}

public sealed class SaveMitarbeiterUrlaubszeitraumResult
{
    private SaveMitarbeiterUrlaubszeitraumResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SaveMitarbeiterUrlaubszeitraumResult Erfolg()
        => new(true, null);

    public static SaveMitarbeiterUrlaubszeitraumResult Fehler(string message)
        => new(false, message);
}

public sealed class SaveMitarbeiterUrlaubszeitraumService
{
    private readonly IMitarbeiterUrlaubszeitraumRepository _repository;
    private readonly ILogger<SaveMitarbeiterUrlaubszeitraumService> _logger;

    public SaveMitarbeiterUrlaubszeitraumService(
        IMitarbeiterUrlaubszeitraumRepository repository,
        ILogger<SaveMitarbeiterUrlaubszeitraumService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<SaveMitarbeiterUrlaubszeitraumResult> ExecuteAsync(
     SaveMitarbeiterUrlaubszeitraumCommand command,
     CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(SaveMitarbeiterUrlaubszeitraumService));

        if (command is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Command ist null", nameof(SaveMitarbeiterUrlaubszeitraumService));
            return SaveMitarbeiterUrlaubszeitraumResult.Fehler("Die Anfrage ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(SaveMitarbeiterUrlaubszeitraumService));
            return SaveMitarbeiterUrlaubszeitraumResult.Fehler("Es wurde kein Mitarbeiter ausgewählt.");
        }

        if (command.Von == default || command.Bis == default)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Zeitraum ungültig", nameof(SaveMitarbeiterUrlaubszeitraumService));
            return SaveMitarbeiterUrlaubszeitraumResult.Fehler("Bitte wählen Sie einen gültigen Zeitraum mit Start- und Enddatum aus.");
        }

        if (command.Bis < command.Von)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Enddatum vor Startdatum", nameof(SaveMitarbeiterUrlaubszeitraumService));
            return SaveMitarbeiterUrlaubszeitraumResult.Fehler("Das Enddatum darf nicht vor dem Startdatum liegen.");
        }

        var hatUeberschneidung = await _repository.HatUeberschneidungAsync(
            command.UserId,
            command.Von,
            command.Bis,
            command.Id,
            cancellationToken);

        if (hatUeberschneidung)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(SaveMitarbeiterUrlaubszeitraumService), "Zeitraumüberschneidung");
            return SaveMitarbeiterUrlaubszeitraumResult.Fehler("Der Zeitraum überschneidet sich mit einem bereits hinterlegten Urlaub.");
        }

        MitarbeiterUrlaubszeitraum entity;

        if (command.Id.HasValue && command.Id.Value != Guid.Empty)
        {
            entity = await _repository.GetByIdAsync(command.Id.Value, cancellationToken)
                ?? new MitarbeiterUrlaubszeitraum
                {
                    Id = command.Id.Value,
                    ErstelltAmUtc = DateTimeOffset.UtcNow
                };
        }
        else
        {
            entity = new MitarbeiterUrlaubszeitraum
            {
                Id = Guid.NewGuid(),
                ErstelltAmUtc = DateTimeOffset.UtcNow
            };
        }

        entity.UserId = command.UserId.Trim();
        entity.Von = command.Von;
        entity.Bis = command.Bis;
        entity.Notiz = string.IsNullOrWhiteSpace(command.Notiz)
            ? null
            : command.Notiz.Trim();
        entity.IstAktiv = true;
        entity.AktualisiertAmUtc = DateTimeOffset.UtcNow;

        await _repository.AddOrUpdateAsync(entity, cancellationToken);

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SaveMitarbeiterUrlaubszeitraumService));
        return SaveMitarbeiterUrlaubszeitraumResult.Erfolg();
    }
}