using ITW.Fahrzeugmanagement.Application.Contracts;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugDokumente;

public sealed record DeleteFahrzeugDokumentCommand(
    Guid FahrzeugId,
    Guid DokumentId);

public sealed class DeleteFahrzeugDokumentResult
{
    private DeleteFahrzeugDokumentResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static DeleteFahrzeugDokumentResult Erfolg()
        => new(true, null);

    public static DeleteFahrzeugDokumentResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class DeleteFahrzeugDokumentService
{
    private readonly IFahrzeugDokumentRepository _repository;
    private readonly IFahrzeugDokumentDateiSpeicher _dateiSpeicher;

    public DeleteFahrzeugDokumentService(
        IFahrzeugDokumentRepository repository,
        IFahrzeugDokumentDateiSpeicher dateiSpeicher)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dateiSpeicher = dateiSpeicher ?? throw new ArgumentNullException(nameof(dateiSpeicher));
    }

    public async Task<DeleteFahrzeugDokumentResult> ExecuteAsync(
        DeleteFahrzeugDokumentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.FahrzeugId == Guid.Empty)
        {
            return DeleteFahrzeugDokumentResult.Fehler("Die Fahrzeug-ID ist erforderlich.");
        }

        if (command.DokumentId == Guid.Empty)
        {
            return DeleteFahrzeugDokumentResult.Fehler("Die Dokument-ID ist ungültig.");
        }

        var dokument = await _repository.GetByIdAsync(
            command.DokumentId,
            cancellationToken);

        if (dokument is null || dokument.FahrzeugId != command.FahrzeugId)
        {
            return DeleteFahrzeugDokumentResult.Fehler("Das Dokument wurde nicht gefunden.");
        }

        var speicherpfad = dokument.Speicherpfad;

        await _repository.DeleteAsync(
            command.DokumentId,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        await _dateiSpeicher.LoescheAsync(
            speicherpfad,
            cancellationToken);

        return DeleteFahrzeugDokumentResult.Erfolg();
    }
}