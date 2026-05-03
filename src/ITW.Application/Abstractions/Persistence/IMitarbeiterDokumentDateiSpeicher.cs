// Datei: src/ITW.Application/Abstractions/Persistence/IMitarbeiterDokumentDateiSpeicher.cs
namespace ITW.Application.Abstractions.Persistence;

public interface IMitarbeiterDokumentDateiSpeicher
{
    Task<string> SpeichereAsync(
        string userId,
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