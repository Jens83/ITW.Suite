using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Domain.Entities;

public sealed class FahrzeugDokument
{
    private FahrzeugDokument()
    {
        Dateiname = string.Empty;
        Bezeichnung = string.Empty;
        ContentType = string.Empty;
        Speicherpfad = string.Empty;
        HochgeladenVonUserId = string.Empty;
    }

    public FahrzeugDokument(
        Guid id,
        Guid fahrzeugId,
        FahrzeugDokumentKategorie kategorie,
        string dateiname,
        string bezeichnung,
        string contentType,
        string speicherpfad,
        DateOnly? gueltigBis,
        string hochgeladenVonUserId,
        DateTimeOffset hochgeladenAm)
    {
        if (id == Guid.Empty) throw new ArgumentException("Die Dokument-ID darf nicht leer sein.", nameof(id));
        if (fahrzeugId == Guid.Empty) throw new ArgumentException("Die Fahrzeug-ID ist erforderlich.", nameof(fahrzeugId));
        if (string.IsNullOrWhiteSpace(dateiname)) throw new ArgumentException("Der Dateiname ist erforderlich.", nameof(dateiname));
        if (string.IsNullOrWhiteSpace(bezeichnung)) throw new ArgumentException("Die Bezeichnung ist erforderlich.", nameof(bezeichnung));
        if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("Der Content-Type ist erforderlich.", nameof(contentType));
        if (string.IsNullOrWhiteSpace(speicherpfad)) throw new ArgumentException("Der Speicherpfad ist erforderlich.", nameof(speicherpfad));
        if (string.IsNullOrWhiteSpace(hochgeladenVonUserId)) throw new ArgumentException("Die UserId des Uploaders ist erforderlich.", nameof(hochgeladenVonUserId));

        Id = id;
        FahrzeugId = fahrzeugId;
        Kategorie = kategorie;
        Dateiname = dateiname.Trim();
        Bezeichnung = bezeichnung.Trim();
        ContentType = contentType.Trim();
        Speicherpfad = speicherpfad.Trim();
        GueltigBis = gueltigBis;
        HochgeladenVonUserId = hochgeladenVonUserId.Trim();
        HochgeladenAm = hochgeladenAm;
    }

    public Guid Id { get; private set; }

    public Guid FahrzeugId { get; private set; }

    public FahrzeugDokumentKategorie Kategorie { get; private set; }

    public string Dateiname { get; private set; }

    public string Bezeichnung { get; private set; }

    public string ContentType { get; private set; }

    public string Speicherpfad { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public DateTimeOffset HochgeladenAm { get; private set; }

    public string HochgeladenVonUserId { get; private set; }
}