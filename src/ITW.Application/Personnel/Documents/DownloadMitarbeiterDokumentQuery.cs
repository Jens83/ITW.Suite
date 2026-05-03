// Datei: src/ITW.Application/Personnel/Documents/DownloadMitarbeiterDokumentQuery.cs
namespace ITW.Application.Personnel.Documents;

public sealed record DownloadMitarbeiterDokumentQuery(
    string UserId,
    Guid DokumentId);