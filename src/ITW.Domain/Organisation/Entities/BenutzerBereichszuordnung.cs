using ITW.Domain.Organisation.Enums;

namespace ITW.Domain.Organisation.Entities;

public sealed class BenutzerBereichszuordnung
{
    private BenutzerBereichszuordnung()
    {
        UserId = string.Empty;
    }

    public BenutzerBereichszuordnung(
        Guid id,
        string userId,
        Organisationsbereich bereich,
        Bereichsrolle rolle,
        Fuehrungsverantwortung fuehrungsverantwortung,
        bool isPrimary,
        DateTimeOffset zugewiesenAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Bereichszuordnung darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId darf nicht leer sein.", nameof(userId));
        }

        if (bereich == Organisationsbereich.Unbekannt)
        {
            throw new ArgumentException("Der Organisationsbereich ist ungültig.", nameof(bereich));
        }

        if (rolle == Bereichsrolle.Unbekannt)
        {
            throw new ArgumentException("Die Bereichsrolle ist ungültig.", nameof(rolle));
        }

        StelleSicherDassRolleZumBereichPasst(bereich, rolle);

        Id = id;
        UserId = userId.Trim();
        Bereich = bereich;
        Rolle = rolle;
        Fuehrungsverantwortung = fuehrungsverantwortung;
        IsPrimary = isPrimary;
        IsActive = true;
        ZugewiesenAm = zugewiesenAm;
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public Organisationsbereich Bereich { get; private set; }

    public Bereichsrolle Rolle { get; private set; }

    public Fuehrungsverantwortung Fuehrungsverantwortung { get; private set; }

    public bool IsPrimary { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset ZugewiesenAm { get; private set; }

    public DateTimeOffset? DeaktiviertAm { get; private set; }

    public void RolleAendern(
        Bereichsrolle neueRolle,
        Fuehrungsverantwortung neueFuehrungsverantwortung)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Eine inaktive Bereichszuordnung kann nicht geändert werden.");
        }

        if (neueRolle == Bereichsrolle.Unbekannt)
        {
            throw new ArgumentException("Die neue Bereichsrolle ist ungültig.", nameof(neueRolle));
        }

        StelleSicherDassRolleZumBereichPasst(Bereich, neueRolle);

        Rolle = neueRolle;
        Fuehrungsverantwortung = neueFuehrungsverantwortung;
    }

    public void AlsPrimaerMarkieren()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Eine inaktive Bereichszuordnung kann nicht als primär markiert werden.");
        }

        IsPrimary = true;
    }

    public void AlsSekundaerMarkieren()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Eine inaktive Bereichszuordnung kann nicht als sekundär markiert werden.");
        }

        IsPrimary = false;
    }

    public void Deaktivieren(DateTimeOffset deaktiviertAm)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        IsPrimary = false;
        DeaktiviertAm = deaktiviertAm;
    }

    private static void StelleSicherDassRolleZumBereichPasst(
        Organisationsbereich bereich,
        Bereichsrolle rolle)
    {
        var istGueltig = bereich switch
        {
            Organisationsbereich.Intensivtransport => rolle is Bereichsrolle.ItwMitarbeiter or Bereichsrolle.Wachleiter,
            Organisationsbereich.Verwaltung => rolle is Bereichsrolle.Verwaltungsmitarbeiter or Bereichsrolle.GeschaeftsfuehrerVerwaltung,
            Organisationsbereich.Geschaeftsfuehrung => rolle is Bereichsrolle.Geschaeftsfuehrung,
            _ => false
        };

        if (!istGueltig)
        {
            throw new InvalidOperationException(
                $"Die Rolle '{rolle}' ist für den Bereich '{bereich}' nicht zulässig.");
        }
    }
}