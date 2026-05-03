using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.Fahrtenbuch;

public sealed class FahrtenbuchEintragListenEintrag
{
    public Guid EintragId { get; init; }

    public Guid FahrzeugId { get; init; }

    public string FahrerUserId { get; init; } = string.Empty;

    public string FahrerName { get; init; } = string.Empty;

    public string? BeifahrerName { get; init; }

    public FahrtKategorie FahrtKategorie { get; init; }

    public string Fahrtzweck { get; init; } = string.Empty;

    public DateTimeOffset StartzeitUtc { get; init; }

    public DateTimeOffset? EndzeitUtc { get; init; }

    public string? Startort { get; init; }

    public string? Zielort { get; init; }

    public int StartKilometerstand { get; init; }

    public int? EndKilometerstand { get; init; }

    public int? GefahreneKilometer { get; init; }

    public decimal? TankmengeLiter { get; init; }

    public int? KilometerstandBeimTanken { get; init; }

    public FahrtenbuchStatus Status { get; init; }

    public bool IstAutomatischVorbelegt { get; init; }

    public string? Bemerkung { get; init; }
}