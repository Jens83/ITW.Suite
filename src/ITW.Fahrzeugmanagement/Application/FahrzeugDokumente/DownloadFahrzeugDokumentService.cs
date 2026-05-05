using ITW.Fahrzeugmanagement.Application.Contracts;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugDokumente;

public sealed record DownloadFahrzeugDokumentQuery(
    Guid FahrzeugId,
    Guid DokumentId);

public sealed class DownloadFahrzeugDokumentResult
{
    private DownloadFahrzeugDokumentResult(
        bool isSuccess,
        string? errorMessage,
        string? dateiname,
        string? contentType,
        byte[]? dateiinhalt)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Dateiname = dateiname;
        ContentType = contentType;
        Dateiinhalt = dateiinhalt;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public string? Dateiname { get; }

    public string? ContentType { get; }

    public byte[]? Dateiinhalt { get; }

    public static DownloadFahrzeugDokumentResult Erfolg(
        string dateiname,
        string contentType,
        byte[] dateiinhalt)
        => new(true, null, dateiname, contentType, dateiinhalt);

    public static DownloadFahrzeugDokumentResult Fehler(string errorMessage)
        => new(false, errorMessage, null, null, null);
}

public sealed class DownloadFahrzeugDokumentService
{
    private readonly IFahrzeugDokumentRepository _repository;
    private readonly IFahrzeugDokumentDateiSpeicher _dateiSpeicher;

    public DownloadFahrzeugDokumentService(
        IFahrzeugDokumentRepository repository,
        IFahrzeugDokumentDateiSpeicher dateiSpeicher)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
        ArgumentNullException.ThrowIfNull(dateiSpeicher);
        _dateiSpeicher = dateiSpeicher;
    }

    public async Task<DownloadFahrzeugDokumentResult> ExecuteAsync(
        DownloadFahrzeugDokumentQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.FahrzeugId == Guid.Empty)
        {
            return DownloadFahrzeugDokumentResult.Fehler("Die Fahrzeug-ID ist erforderlich.");
        }

        if (query.DokumentId == Guid.Empty)
        {
            return DownloadFahrzeugDokumentResult.Fehler("Die Dokument-ID ist ungültig.");
        }

        var dokument = await _repository.GetByIdAsync(
            query.DokumentId,
            cancellationToken);

        if (dokument is null || dokument.FahrzeugId != query.FahrzeugId)
        {
            return DownloadFahrzeugDokumentResult.Fehler("Das Dokument wurde nicht gefunden.");
        }

        var dateiinhalt = await _dateiSpeicher.LadeAsync(
            dokument.Speicherpfad,
            cancellationToken);

        if (dateiinhalt is null || dateiinhalt.Length == 0)
        {
            return DownloadFahrzeugDokumentResult.Fehler(
                "Die Dokumentdatei konnte nicht geladen werden.");
        }

        return DownloadFahrzeugDokumentResult.Erfolg(
            dokument.Dateiname,
            dokument.ContentType,
            dateiinhalt);
    }
}