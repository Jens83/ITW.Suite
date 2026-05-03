namespace ITW.Dienstplan.Domain.Entities;

public sealed class DienstplanPeriode
{
    private DienstplanPeriode()
    {
        Bezeichnung = string.Empty;
        ErstelltVonUserId = string.Empty;
    }

    public DienstplanPeriode(
        Guid id,
        int jahr,
        int monat,
        string bezeichnung,
        bool wunschphaseIstOffen,
        string erstelltVonUserId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Dienstplanperiode darf nicht leer sein.", nameof(id));
        }

        if (jahr < 2000 || jahr > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(jahr), "Das Jahr ist ungültig.");
        }

        if (monat < 1 || monat > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(monat), "Der Monat ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(bezeichnung))
        {
            throw new ArgumentException("Die Bezeichnung ist erforderlich.", nameof(bezeichnung));
        }

        if (string.IsNullOrWhiteSpace(erstelltVonUserId))
        {
            throw new ArgumentException("Die UserId des Erstellers ist erforderlich.", nameof(erstelltVonUserId));
        }

        Id = id;
        Jahr = jahr;
        Monat = monat;
        Bezeichnung = bezeichnung.Trim();
        WunschphaseIstOffen = wunschphaseIstOffen;
        ErstelltVonUserId = erstelltVonUserId.Trim();
        ErstelltAm = erstelltAm;
        PlanIstFreigegeben = false;
        PlanFreigegebenAm = null;
        PlanFreigegebenVonUserId = null;
    }

    public Guid Id { get; private set; }

    public int Jahr { get; private set; }

    public int Monat { get; private set; }

    public string Bezeichnung { get; private set; }

    public bool WunschphaseIstOffen { get; private set; }

    public bool PlanIstFreigegeben { get; private set; }

    public DateTimeOffset? PlanFreigegebenAm { get; private set; }

    public string? PlanFreigegebenVonUserId { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public string ErstelltVonUserId { get; private set; }

    public void WunschphaseOeffnen()
    {
        WunschphaseIstOffen = true;
        PlanFreigabeZuruecknehmen();
    }

    public void WunschphaseSchliessen()
    {
        WunschphaseIstOffen = false;
    }

    public void PlanFreigeben(
        string freigegebenVonUserId,
        DateTimeOffset freigegebenAm)
    {
        if (string.IsNullOrWhiteSpace(freigegebenVonUserId))
        {
            throw new ArgumentException("Die UserId für die Planfreigabe ist erforderlich.", nameof(freigegebenVonUserId));
        }

        PlanIstFreigegeben = true;
        PlanFreigegebenAm = freigegebenAm;
        PlanFreigegebenVonUserId = freigegebenVonUserId.Trim();
    }

    public void PlanFreigabeZuruecknehmen()
    {
        PlanIstFreigegeben = false;
        PlanFreigegebenAm = null;
        PlanFreigegebenVonUserId = null;
    }
}