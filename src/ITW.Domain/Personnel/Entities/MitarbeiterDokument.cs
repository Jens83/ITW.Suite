// Datei: src/ITW.Domain/Personnel/Entities/MitarbeiterDokument.cs
namespace ITW.Domain.Personnel.Entities;

public sealed class MitarbeiterDokument
{
    private MitarbeiterDokument()
    {
        UserId = string.Empty;
        Kategorie = string.Empty;
        DateinameOriginal = string.Empty;
        Speicherpfad = string.Empty;
        Inhaltstyp = string.Empty;
        HochgeladenVonUserId = string.Empty;
    }

    public MitarbeiterDokument(
        Guid id,
        string userId,
        string kategorie,
        string dateinameOriginal,
        string speicherpfad,
        string inhaltstyp,
        long dateigroesseBytes,
        DateTimeOffset hochgeladenAm,
        string hochgeladenVonUserId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die Dokument-ID darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(kategorie))
        {
            throw new ArgumentException("Die Kategorie ist erforderlich.", nameof(kategorie));
        }

        if (string.IsNullOrWhiteSpace(dateinameOriginal))
        {
            throw new ArgumentException("Der Original-Dateiname ist erforderlich.", nameof(dateinameOriginal));
        }

        if (string.IsNullOrWhiteSpace(speicherpfad))
        {
            throw new ArgumentException("Der Speicherpfad ist erforderlich.", nameof(speicherpfad));
        }

        if (string.IsNullOrWhiteSpace(inhaltstyp))
        {
            throw new ArgumentException("Der Inhaltstyp ist erforderlich.", nameof(inhaltstyp));
        }

        if (dateigroesseBytes <= 0)
        {
            throw new ArgumentException("Die Dateigröße muss größer als 0 sein.", nameof(dateigroesseBytes));
        }

        if (string.IsNullOrWhiteSpace(hochgeladenVonUserId))
        {
            throw new ArgumentException("Die Bearbeiter-UserId ist erforderlich.", nameof(hochgeladenVonUserId));
        }

        Id = id;
        UserId = userId.Trim();
        Kategorie = kategorie.Trim();
        DateinameOriginal = dateinameOriginal.Trim();
        Speicherpfad = speicherpfad.Trim();
        Inhaltstyp = inhaltstyp.Trim();
        DateigroesseBytes = dateigroesseBytes;
        HochgeladenAm = hochgeladenAm;
        HochgeladenVonUserId = hochgeladenVonUserId.Trim();
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public string Kategorie { get; private set; }

    public string DateinameOriginal { get; private set; }

    public string Speicherpfad { get; private set; }

    public string Inhaltstyp { get; private set; }

    public long DateigroesseBytes { get; private set; }

    public DateTimeOffset HochgeladenAm { get; private set; }

    public string HochgeladenVonUserId { get; private set; }
}