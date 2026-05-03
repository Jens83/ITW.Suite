namespace ITW.Domain.Personnel.Entities;

public sealed class ItwMitarbeiterprofil
{
    private readonly List<ItwMitarbeiterQualifikation> _qualifikationen = new();

    private ItwMitarbeiterprofil()
    {
        UserId = string.Empty;
    }

    public ItwMitarbeiterprofil(
        Guid id,
        string userId,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID des ITW-Mitarbeiterprofils darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId darf nicht leer sein.", nameof(userId));
        }

        Id = id;
        UserId = userId.Trim();
        ErstelltAm = erstelltAm;
        AktualisiertAm = erstelltAm;
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public DateTimeOffset AktualisiertAm { get; private set; }

    public IReadOnlyCollection<ItwMitarbeiterQualifikation> Qualifikationen => _qualifikationen;

    public void MarkiereAktualisiert(DateTimeOffset aktualisiertAm)
    {
        AktualisiertAm = aktualisiertAm;
    }

    public void AktualisiereQualifikationen(
        Guid hauptqualifikationId,
        IEnumerable<Guid>? zusatzqualifikationIds,
        DateTimeOffset aktualisiertAm)
    {
        if (hauptqualifikationId == Guid.Empty)
        {
            throw new ArgumentException("Die Hauptqualifikation ist erforderlich.", nameof(hauptqualifikationId));
        }

        var distinctZusatzqualifikationIds = (zusatzqualifikationIds ?? Enumerable.Empty<Guid>())
            .Where(x => x != Guid.Empty && x != hauptqualifikationId)
            .Distinct()
            .ToArray();

        _qualifikationen.Clear();

        _qualifikationen.Add(new ItwMitarbeiterQualifikation(
            Guid.NewGuid(),
            Id,
            hauptqualifikationId,
            true,
            aktualisiertAm));

        foreach (var zusatzqualifikationId in distinctZusatzqualifikationIds)
        {
            _qualifikationen.Add(new ItwMitarbeiterQualifikation(
                Guid.NewGuid(),
                Id,
                zusatzqualifikationId,
                false,
                aktualisiertAm));
        }

        AktualisiertAm = aktualisiertAm;
    }
}