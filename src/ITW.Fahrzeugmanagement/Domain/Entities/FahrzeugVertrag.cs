using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Domain.Entities;

public sealed class FahrzeugVertrag
{
    private FahrzeugVertrag()
    {
        Anbieter = string.Empty;
        Vertragsnummer = string.Empty;
        ErstelltVonUserId = string.Empty;
    }

    public FahrzeugVertrag(
        Guid id,
        Guid fahrzeugId,
        FahrzeugVertragTyp vertragTyp,
        string anbieter,
        string vertragsnummer,
        DateOnly gueltigVon,
        DateOnly? gueltigBis,
        decimal? betragProPeriode,
        int? periodizitaet,
        int? kuendigungsfristTage,
        Guid? dokumentId,
        string? notiz,
        string erstelltVonUserId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty) throw new ArgumentException("Die Vertrags-ID darf nicht leer sein.", nameof(id));
        if (fahrzeugId == Guid.Empty) throw new ArgumentException("Die Fahrzeug-ID ist erforderlich.", nameof(fahrzeugId));
        if (string.IsNullOrWhiteSpace(anbieter)) throw new ArgumentException("Der Anbieter ist erforderlich.", nameof(anbieter));
        if (string.IsNullOrWhiteSpace(vertragsnummer)) throw new ArgumentException("Die Vertragsnummer ist erforderlich.", nameof(vertragsnummer));
        if (gueltigBis.HasValue && gueltigBis.Value < gueltigVon) throw new ArgumentException("GueltigBis darf nicht vor GueltigVon liegen.", nameof(gueltigBis));
        if (betragProPeriode < 0) throw new ArgumentOutOfRangeException(nameof(betragProPeriode), "Der Betrag darf nicht negativ sein.");
        if (periodizitaet < 0) throw new ArgumentOutOfRangeException(nameof(periodizitaet), "Die Periodizitaet darf nicht negativ sein.");
        if (kuendigungsfristTage < 0) throw new ArgumentOutOfRangeException(nameof(kuendigungsfristTage), "Die Kündigungsfrist darf nicht negativ sein.");
        if (string.IsNullOrWhiteSpace(erstelltVonUserId)) throw new ArgumentException("Die UserId des Erstellers ist erforderlich.", nameof(erstelltVonUserId));

        Id = id;
        FahrzeugId = fahrzeugId;
        VertragTyp = vertragTyp;
        Anbieter = anbieter.Trim();
        Vertragsnummer = vertragsnummer.Trim();
        GueltigVon = gueltigVon;
        GueltigBis = gueltigBis;
        BetragProPeriode = betragProPeriode;
        Periodizitaet = periodizitaet;
        KuendigungsfristTage = kuendigungsfristTage;
        DokumentId = dokumentId;
        Notiz = string.IsNullOrWhiteSpace(notiz) ? null : notiz.Trim();
        ErstelltVonUserId = erstelltVonUserId.Trim();
        ErstelltAm = erstelltAm;
    }

    public Guid Id { get; private set; }

    public Guid FahrzeugId { get; private set; }

    public FahrzeugVertragTyp VertragTyp { get; private set; }

    public string Anbieter { get; private set; }

    public string Vertragsnummer { get; private set; }

    public DateOnly GueltigVon { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public decimal? BetragProPeriode { get; private set; }

    public int? Periodizitaet { get; private set; }

    public int? KuendigungsfristTage { get; private set; }

    public Guid? DokumentId { get; private set; }

    public string? Notiz { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public string ErstelltVonUserId { get; private set; }
}