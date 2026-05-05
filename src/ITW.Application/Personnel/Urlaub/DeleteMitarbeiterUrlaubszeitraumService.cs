using ITW.Application.Personnel.Urlaub.Contracts;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.Urlaub;

public sealed class DeleteMitarbeiterUrlaubszeitraumCommand
{
    public Guid Id { get; init; }
}

public sealed class DeleteMitarbeiterUrlaubszeitraumResult
{
    private DeleteMitarbeiterUrlaubszeitraumResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static DeleteMitarbeiterUrlaubszeitraumResult Erfolg()
        => new(true, null);

    public static DeleteMitarbeiterUrlaubszeitraumResult Fehler(string message)
        => new(false, message);
}

public sealed class DeleteMitarbeiterUrlaubszeitraumService
{
    private readonly IMitarbeiterUrlaubszeitraumRepository _repository;
    private readonly ILogger<DeleteMitarbeiterUrlaubszeitraumService> _logger;

    public DeleteMitarbeiterUrlaubszeitraumService(
        IMitarbeiterUrlaubszeitraumRepository repository,
        ILogger<DeleteMitarbeiterUrlaubszeitraumService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<DeleteMitarbeiterUrlaubszeitraumResult> ExecuteAsync(
        DeleteMitarbeiterUrlaubszeitraumCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(DeleteMitarbeiterUrlaubszeitraumService));

        if (command is null || command.Id == Guid.Empty)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Id leer oder ungültig", nameof(DeleteMitarbeiterUrlaubszeitraumService));
            return DeleteMitarbeiterUrlaubszeitraumResult.Fehler("Der ausgewählte Urlaubszeitraum ist ungültig.");
        }

        await _repository.DeleteAsync(command.Id, cancellationToken);

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(DeleteMitarbeiterUrlaubszeitraumService));
        return DeleteMitarbeiterUrlaubszeitraumResult.Erfolg();
    }
}