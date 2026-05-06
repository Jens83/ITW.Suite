using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Aktivitaet;

public sealed class AktivitaetsEintrag
{
    private AktivitaetsEintrag() { }

    public AktivitaetsEintrag(
        Guid id,
        OrganisationsbereichCode bereich,
        string text,
        AktivitaetsKategorie kategorie,
        string iconCssClass,
        DateTimeOffset zeitpunkt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id darf nicht leer sein.", nameof(id));
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text darf nicht leer sein.", nameof(text));
        if (string.IsNullOrWhiteSpace(iconCssClass))
            throw new ArgumentException("IconCssClass darf nicht leer sein.", nameof(iconCssClass));

        Id           = id;
        Bereich      = bereich;
        Text         = text.Trim();
        Kategorie    = kategorie;
        IconCssClass = iconCssClass.Trim();
        Zeitpunkt    = zeitpunkt;
    }

    public Guid                   Id           { get; private set; }
    public OrganisationsbereichCode Bereich    { get; private set; }
    public string                 Text         { get; private set; } = "";
    public AktivitaetsKategorie   Kategorie    { get; private set; }
    public string                 IconCssClass { get; private set; } = "";
    public DateTimeOffset         Zeitpunkt    { get; private set; }
}
