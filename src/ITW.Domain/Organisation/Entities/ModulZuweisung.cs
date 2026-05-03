using ITW.Domain.Organisation.Enums;

namespace ITW.Domain.Organisation.Entities;

public sealed class ModulZuweisung
{
    private ModulZuweisung()
    {
        ZugewiesenVonUserId = string.Empty;
    }

    public ModulZuweisung(
        Guid id,
        Modul modul,
        Organisationsbereich bereich,
        Bereichsrolle rolle,
        string zugewiesenVonUserId,
        DateTimeOffset zugewiesenAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Modulzuweisung darf nicht leer sein.", nameof(id));
        }

        if (modul == Modul.Unbekannt)
        {
            throw new ArgumentException("Das Modul ist ungültig.", nameof(modul));
        }

        if (bereich == Organisationsbereich.Unbekannt)
        {
            throw new ArgumentException("Der Bereich ist ungültig.", nameof(bereich));
        }

        if (rolle == Bereichsrolle.Unbekannt)
        {
            throw new ArgumentException("Die Rolle ist ungültig.", nameof(rolle));
        }

        if (string.IsNullOrWhiteSpace(zugewiesenVonUserId))
        {
            throw new ArgumentException("Die UserId des zuweisenden Benutzers darf nicht leer sein.", nameof(zugewiesenVonUserId));
        }

        Id = id;
        Modul = modul;
        Bereich = bereich;
        Rolle = rolle;
        IstAktiv = true;
        ZugewiesenAm = zugewiesenAm;
        ZugewiesenVonUserId = zugewiesenVonUserId.Trim();
    }

    public Guid Id { get; private set; }

    public Modul Modul { get; private set; }

    public Organisationsbereich Bereich { get; private set; }

    public Bereichsrolle Rolle { get; private set; }

    public bool IstAktiv { get; private set; }

    public DateTimeOffset ZugewiesenAm { get; private set; }

    public string ZugewiesenVonUserId { get; private set; }

    public DateTimeOffset? DeaktiviertAm { get; private set; }

    public string? DeaktiviertVonUserId { get; private set; }

    public void Aktivieren(
        string aktiviertVonUserId,
        DateTimeOffset aktiviertAm)
    {
        if (string.IsNullOrWhiteSpace(aktiviertVonUserId))
        {
            throw new ArgumentException("Die UserId des aktivierenden Benutzers darf nicht leer sein.", nameof(aktiviertVonUserId));
        }

        IstAktiv = true;
        DeaktiviertAm = null;
        DeaktiviertVonUserId = null;
        ZugewiesenVonUserId = aktiviertVonUserId.Trim();
        ZugewiesenAm = aktiviertAm;
    }

    public void Deaktivieren(
        string deaktiviertVonUserId,
        DateTimeOffset deaktiviertAm)
    {
        if (string.IsNullOrWhiteSpace(deaktiviertVonUserId))
        {
            throw new ArgumentException("Die UserId des deaktivierenden Benutzers darf nicht leer sein.", nameof(deaktiviertVonUserId));
        }

        IstAktiv = false;
        DeaktiviertAm = deaktiviertAm;
        DeaktiviertVonUserId = deaktiviertVonUserId.Trim();
    }
}