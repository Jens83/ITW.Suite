// Datei: src/ITW.Application/Personnel/Documents/MitarbeiterDokumentListenEintragDto.cs
namespace ITW.Application.Personnel.Documents;

public sealed record MitarbeiterDokumentListenEintragDto(
    Guid DokumentId,
    string Kategorie,
    string DateinameOriginal,
    string Inhaltstyp,
    long DateigroesseBytes,
    DateTimeOffset HochgeladenAm);