namespace ITW.Domain.Personnel.Qualifications;

public sealed class ItwQualifikation
{
    private ItwQualifikation()
    {
        Code = string.Empty;
        Bezeichnung = string.Empty;
    }

    public ItwQualifikation(
        Guid id,
        string code,
        string bezeichnung,
        int sortierung,
        bool isAktiv = true)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Qualifikation darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Der Code der Qualifikation darf nicht leer sein.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(bezeichnung))
        {
            throw new ArgumentException("Die Bezeichnung der Qualifikation darf nicht leer sein.", nameof(bezeichnung));
        }

        Id = id;
        Code = code.Trim();
        Bezeichnung = bezeichnung.Trim();
        Sortierung = sortierung;
        IsAktiv = isAktiv;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; }

    public string Bezeichnung { get; private set; }

    public int Sortierung { get; private set; }

    public bool IsAktiv { get; private set; }
}