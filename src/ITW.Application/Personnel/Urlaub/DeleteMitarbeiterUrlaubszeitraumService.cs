using ITW.Application.Personnel.Urlaub.Contracts;

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

    public DeleteMitarbeiterUrlaubszeitraumService(
        IMitarbeiterUrlaubszeitraumRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<DeleteMitarbeiterUrlaubszeitraumResult> ExecuteAsync(
        DeleteMitarbeiterUrlaubszeitraumCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command is null || command.Id == Guid.Empty)
        {
            return DeleteMitarbeiterUrlaubszeitraumResult.Fehler("Der ausgewählte Urlaubszeitraum ist ungültig.");
        }

        await _repository.DeleteAsync(command.Id, cancellationToken);

        return DeleteMitarbeiterUrlaubszeitraumResult.Erfolg();
    }
}