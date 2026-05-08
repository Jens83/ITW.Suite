using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Domain.Entities;

public sealed class SauerstoffFlasche
{
    private SauerstoffFlasche()
    {
        ErstelltVonUserId = string.Empty;
    }

    public SauerstoffFlasche(
        Guid id,
        SauerstoffFlaschenGroesse groesse,
        string? flaschenNummer,
        Guid lieferungId,
        DateTimeOffset erstelltAm,
        string erstelltVonUserId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (lieferungId == Guid.Empty)
            throw new ArgumentException("LieferungId darf nicht leer sein.", nameof(lieferungId));
        if (string.IsNullOrWhiteSpace(erstelltVonUserId))
            throw new ArgumentException("ErstelltVonUserId ist erforderlich.", nameof(erstelltVonUserId));

        Id                = id;
        Groesse           = groesse;
        FlaschenNummer    = string.IsNullOrWhiteSpace(flaschenNummer) ? null : flaschenNummer.Trim();
        LieferungId       = lieferungId;
        Status            = SauerstoffFlaschenStatus.Voll;
        FahrzeugId        = null;
        IstAktiv          = true;
        ErstelltAm        = erstelltAm;
        ErstelltVonUserId = erstelltVonUserId.Trim();
    }

    public Guid                      Id                { get; private set; }
    public SauerstoffFlaschenGroesse Groesse           { get; private set; }
    public string?                   FlaschenNummer    { get; private set; }
    public Guid                      LieferungId       { get; private set; }
    public SauerstoffFlaschenStatus  Status            { get; private set; }
    public Guid?                     FahrzeugId        { get; private set; }
    public bool                      IstAktiv          { get; private set; }
    public DateTimeOffset            ErstelltAm        { get; private set; }
    public string                    ErstelltVonUserId { get; private set; }

    public bool IstImDepot    => FahrzeugId is null;
    public bool IstImFahrzeug => FahrzeugId is not null;

    public int TageImSystem(DateOnly lieferdatum, DateOnly heute)
        => Math.Max(0, heute.DayNumber - lieferdatum.DayNumber);

    public bool HatLangzeitmietRisiko(DateOnly lieferdatum, DateOnly heute, int vorwarnungTage = 30)
        => TageImSystem(lieferdatum, heute) >= (180 - vorwarnungTage);

    public void NachFahrzeugBewegen(Guid fahrzeugId)
    {
        if (fahrzeugId == Guid.Empty)
            throw new ArgumentException("FahrzeugId darf nicht leer sein.", nameof(fahrzeugId));
        if (!IstImDepot)
            throw new InvalidOperationException("Flasche befindet sich nicht im Depot.");
        if (Status != SauerstoffFlaschenStatus.Voll)
            throw new InvalidOperationException("Nur volle Flaschen können in ein Fahrzeug überführt werden.");

        FahrzeugId = fahrzeugId;
    }

    public void AlsLeerZurueckgeben()
    {
        if (IstImDepot)
            throw new InvalidOperationException("Flasche befindet sich bereits im Depot.");

        Status     = SauerstoffFlaschenStatus.Leer;
        FahrzeugId = null;
    }

    public void Deaktiviere() => IstAktiv = false;
}
