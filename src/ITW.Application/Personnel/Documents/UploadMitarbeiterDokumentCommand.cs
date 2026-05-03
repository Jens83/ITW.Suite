// Datei: src/ITW.Application/Personnel/Documents/UploadMitarbeiterDokumentCommand.cs
namespace ITW.Application.Personnel.Documents;

public sealed record UploadMitarbeiterDokumentCommand(
    string UserId,
    string HochgeladenVonUserId,
    string Kategorie,
    string DateinameOriginal,
    byte[] Dateiinhalt);