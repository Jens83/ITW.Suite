using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Entities;

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

    public SaveMitarbeiterUrlaubszeitraumService(
        IMitarbeiterUrlaubszeitraumRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<SaveMitarbeiterUrlaubszeitraumResult> ExecuteAsync(
     SaveMitarbeiterUrlaubszeitraumCommand command,
     CancellationToken cancellationToken = default)
    {
        if (command is null)
        {
            return SaveMitarbeiterUrlaubszeitraumResult.Fehler("Die Anfrage ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return SaveMitarbeiterUrlaubszeitraumResult.Fehler("Es wurde kein Mitarbeiter ausgewählt.");
        }

        if (command.Von == default || command.Bis == default)
        {
            return SaveMitarbeiterUrlaubszeitraumResult.Fehler("Bitte wählen Sie einen gültigen Zeitraum mit Start- und Enddatum aus.");
        }

        if (command.Bis < command.Von)
        {
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

        return SaveMitarbeiterUrlaubszeitraumResult.Erfolg();
    }
}