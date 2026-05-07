using ITW.Domain.Personnel.Enums;

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

    // Workflow-Felder (alle nullable/default-kompatibel für bestehende Datensätze)

    public UrlaubszeitraumStatus Status { get; set; } = UrlaubszeitraumStatus.Genehmigt;

    public string? EingereichtVonUserId { get; set; }

    public string? Begruendung { get; set; }

    public string? Loesung { get; set; }

    public DateTimeOffset? EntschiedenAm { get; set; }

    public string? EntschiedenVonUserId { get; set; }
}
