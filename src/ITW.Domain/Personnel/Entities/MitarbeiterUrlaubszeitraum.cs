namespace ITW.Domain.Personnel.Entities;

public sealed class MitarbeiterUrlaubszeitraum
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateOnly Von { get; set; }

    public DateOnly Bis { get; set; }

    public string? Notiz { get; set; }

    public bool IstAktiv { get; set; }

    public DateTimeOffset ErstelltAmUtc { get; set; }

    public DateTimeOffset AktualisiertAmUtc { get; set; }
}