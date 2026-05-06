using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Aufgaben;

public sealed class Aufgabe
{
    private Aufgabe() { }

    public Aufgabe(
        Guid id,
        OrganisationsbereichCode bereich,
        string titel,
        AufgabePrioritaet prioritaet,
        AufgabeQuelle quelle,
        DateOnly? faelligkeitsdatum,
        string? systemSchluessel,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (string.IsNullOrWhiteSpace(titel))
            throw new ArgumentException("Titel darf nicht leer sein.", nameof(titel));

        Id               = id;
        Bereich          = bereich;
        Titel            = titel.Trim();
        Prioritaet       = prioritaet;
        Status           = AufgabeStatus.Offen;
        Quelle           = quelle;
        SystemSchluessel = systemSchluessel?.Trim();
        Faelligkeitsdatum = faelligkeitsdatum;
        ErstelltAm       = erstelltAm;
    }

    public Guid                    Id                { get; private set; }
    public OrganisationsbereichCode Bereich          { get; private set; }
    public string                  Titel             { get; private set; } = "";
    public AufgabePrioritaet       Prioritaet        { get; private set; }
    public AufgabeStatus           Status            { get; private set; }
    public AufgabeQuelle           Quelle            { get; private set; }
    public string?                 SystemSchluessel  { get; private set; }
    public DateOnly?               Faelligkeitsdatum { get; private set; }
    public DateTimeOffset          ErstelltAm        { get; private set; }
    public DateTimeOffset?         ErledigtAm        { get; private set; }

    public void AlsErledigtMarkieren(DateTimeOffset zeitpunkt)
    {
        Status     = AufgabeStatus.Erledigt;
        ErledigtAm = zeitpunkt;
    }

    public void AlsOffenMarkieren()
    {
        Status     = AufgabeStatus.Offen;
        ErledigtAm = null;
    }

    public void AktualisiereFaelligkeitsdatum(DateOnly? datum)
    {
        Faelligkeitsdatum = datum;
    }
}
