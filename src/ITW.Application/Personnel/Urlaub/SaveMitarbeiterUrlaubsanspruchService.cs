using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Entities;

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

    public SaveMitarbeiterUrlaubsanspruchService(
        IMitarbeiterUrlaubsanspruchRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<SaveMitarbeiterUrlaubsanspruchResult> ExecuteAsync(
        SaveMitarbeiterUrlaubsanspruchCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command is null)
        {
            return SaveMitarbeiterUrlaubsanspruchResult.Fehler("Die Anfrage ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return SaveMitarbeiterUrlaubsanspruchResult.Fehler("Es wurde kein Mitarbeiter ausgewählt.");
        }

        if (command.Jahr is < 2000 or > 2100)
        {
            return SaveMitarbeiterUrlaubsanspruchResult.Fehler("Das ausgewählte Jahr ist ungültig.");
        }

        if (command.Anspruchstage is < 0 or > 366)
        {
            return SaveMitarbeiterUrlaubsanspruchResult.Fehler("Die Anspruchstage sind ungültig.");
        }

        if (command.Uebertragstage is < 0 or > 366)
        {
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

        return SaveMitarbeiterUrlaubsanspruchResult.Erfolg();
    }
}