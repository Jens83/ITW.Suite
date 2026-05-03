namespace ITW.Domain.Personnel.Entities;

public sealed class ItwMitarbeiterQualifikation
{
    private ItwMitarbeiterQualifikation()
    {
    }

    public ItwMitarbeiterQualifikation(
        Guid id,
        Guid itwMitarbeiterprofilId,
        Guid qualifikationId,
        bool istHauptqualifikation,
        DateTimeOffset zugewiesenAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Mitarbeiterqualifikation darf nicht leer sein.", nameof(id));
        }

        if (itwMitarbeiterprofilId == Guid.Empty)
        {
            throw new ArgumentException("Die ID des ITW-Mitarbeiterprofils darf nicht leer sein.", nameof(itwMitarbeiterprofilId));
        }

        if (qualifikationId == Guid.Empty)
        {
            throw new ArgumentException("Die Qualifikation darf nicht leer sein.", nameof(qualifikationId));
        }

        Id = id;
        ItwMitarbeiterprofilId = itwMitarbeiterprofilId;
        QualifikationId = qualifikationId;
        IstHauptqualifikation = istHauptqualifikation;
        ZugewiesenAm = zugewiesenAm;
    }

    public Guid Id { get; private set; }

    public Guid ItwMitarbeiterprofilId { get; private set; }

    public Guid QualifikationId { get; private set; }

    public bool IstHauptqualifikation { get; private set; }

    public DateTimeOffset ZugewiesenAm { get; private set; }
}