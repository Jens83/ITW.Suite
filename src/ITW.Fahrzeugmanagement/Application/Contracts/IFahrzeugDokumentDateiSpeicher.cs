namespace ITW.Fahrzeugmanagement.Application.Contracts;

public interface IFahrzeugDokumentDateiSpeicher
{
    Task<string> SpeichereAsync(
        Guid fahrzeugId,
        string dateinameOriginal,
        byte[] dateiinhalt,
        CancellationToken cancellationToken = default);

    Task<byte[]?> LadeAsync(
        string speicherpfad,
        CancellationToken cancellationToken = default);

    Task LoescheAsync(
        string speicherpfad,
        CancellationToken cancellationToken = default);
}