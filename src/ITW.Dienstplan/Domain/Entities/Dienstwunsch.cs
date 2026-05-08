using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Domain.Entities;

public sealed class Dienstwunsch
{
    private Dienstwunsch()
    {
        UserId = string.Empty;
        WunschTyp = DienstwunschTyp.Wunsch;
    }

    public Dienstwunsch(
        Guid id,
        Guid dienstplanPeriodeId,
        string userId,
        DateOnly wunschDatum,
        DienstwunschTyp wunschTyp,
        DateTimeOffset erstelltAm)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die ID des Dienstwunsches darf nicht leer sein.", nameof(id));
        }

        if (dienstplanPeriodeId == Guid.Empty)
        {
            throw new ArgumentException("Die ID der Dienstplanperiode darf nicht leer sein.", nameof(dienstplanPeriodeId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        if (!Enum.IsDefined(wunschTyp))
        {
            throw new ArgumentException("Der Wunschtyp ist ungültig.", nameof(wunschTyp));
        }

        Id = id;
        DienstplanPeriodeId = dienstplanPeriodeId;
        UserId = userId.Trim();
        WunschDatum = wunschDatum;
        WunschTyp = wunschTyp;
        ErstelltAm = erstelltAm;
    }

    public Guid Id { get; private set; }

    public Guid DienstplanPeriodeId { get; private set; }

    public string UserId { get; private set; }

    public DateOnly WunschDatum { get; private set; }

    public DienstwunschTyp WunschTyp { get; private set; }

    public DateTimeOffset ErstelltAm { get; private set; }

    public void AktualisiereWunschTyp(DienstwunschTyp wunschTyp)
    {
        if (!Enum.IsDefined(wunschTyp))
        {
            throw new ArgumentException("Der Wunschtyp ist ungültig.", nameof(wunschTyp));
        }

        WunschTyp = wunschTyp;
    }
}