namespace ITW.Domain.Personnel.Entities;

public sealed class MitarbeiterUrlaubsanspruch
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int Jahr { get; set; }

    public int Anspruchstage { get; set; }

    public int Uebertragstage { get; set; }

    public string? Bemerkung { get; set; }

    public DateTimeOffset ErstelltAmUtc { get; set; }

    public DateTimeOffset AktualisiertAmUtc { get; set; }
}