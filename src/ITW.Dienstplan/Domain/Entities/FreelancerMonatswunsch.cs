namespace ITW.Dienstplan.Domain.Entities;

public sealed class FreelancerMonatswunsch
{
    private FreelancerMonatswunsch()
    {
        UserId = string.Empty;
    }

    public FreelancerMonatswunsch(
        Guid id,
        Guid dienstplanPeriodeId,
        string userId,
        int gewuenschteDienste,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID des Freelancer-Monatswunsches darf nicht leer sein.", nameof(id));
        }

        if (dienstplanPeriodeId == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Dienstplanperiode darf nicht leer sein.", nameof(dienstplanPeriodeId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        PruefeGewuenschteDienste(gewuenschteDienste);

        Id = id;
        DienstplanPeriodeId = dienstplanPeriodeId;
        UserId = userId.Trim();
        GewuenschteDienste = gewuenschteDienste;
        ErstelltAm = erstelltAm;
        AktualisiertAm = erstelltAm;
    }

    public Guid Id { get; private set; }

    public Guid DienstplanPeriodeId { get; private set; }

    public string UserId { get; private set; }

    public int GewuenschteDienste { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public DateTimeOffset AktualisiertAm { get; private set; }

    public void AktualisiereGewuenschteDienste(
        int gewuenschteDienste,
        DateTimeOffset aktualisiertAm)
    {
        PruefeGewuenschteDienste(gewuenschteDienste);

        GewuenschteDienste = gewuenschteDienste;
        AktualisiertAm = aktualisiertAm;
    }

    private static void PruefeGewuenschteDienste(int gewuenschteDienste)
    {
        if (gewuenschteDienste is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gewuenschteDienste),
                "Für Freelancer sind nur 1, 2 oder 3 gewünschte Dienste pro Monat zulässig.");
        }
    }
}