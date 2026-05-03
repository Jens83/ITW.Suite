using ITW.Fahrzeugmanagement.Application.Contracts;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugVertraege;

public sealed record DeleteFahrzeugVertragCommand(
    Guid FahrzeugId,
    Guid VertragId);

public sealed class DeleteFahrzeugVertragResult
{
    private DeleteFahrzeugVertragResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static DeleteFahrzeugVertragResult Erfolg()
        => new(true, null);

    public static DeleteFahrzeugVertragResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class DeleteFahrzeugVertragService
{
    private readonly IFahrzeugVertragRepository _repository;

    public DeleteFahrzeugVertragService(IFahrzeugVertragRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<DeleteFahrzeugVertragResult> ExecuteAsync(
        DeleteFahrzeugVertragCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.FahrzeugId == Guid.Empty)
        {
            return DeleteFahrzeugVertragResult.Fehler("Die Fahrzeug-ID ist erforderlich.");
        }

        if (command.VertragId == Guid.Empty)
        {
            return DeleteFahrzeugVertragResult.Fehler("Die Vertrags-ID ist ungültig.");
        }

        var vertrag = await _repository.GetByIdAsync(
            command.VertragId,
            cancellationToken);

        if (vertrag is null || vertrag.FahrzeugId != command.FahrzeugId)
        {
            return DeleteFahrzeugVertragResult.Fehler("Der Vertrag wurde nicht gefunden.");
        }

        await _repository.DeleteAsync(
            command.VertragId,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        return DeleteFahrzeugVertragResult.Erfolg();
    }
}