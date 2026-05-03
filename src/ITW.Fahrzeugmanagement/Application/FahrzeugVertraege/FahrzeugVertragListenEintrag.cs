using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugVertraege;

public sealed class FahrzeugVertragListenEintrag
{
    public Guid VertragId { get; init; }

    public Guid FahrzeugId { get; init; }

    public FahrzeugVertragTyp VertragTyp { get; init; }

    public string Anbieter { get; init; } = string.Empty;

    public string Vertragsnummer { get; init; } = string.Empty;

    public DateOnly GueltigVon { get; init; }

    public DateOnly? GueltigBis { get; init; }

    public decimal? BetragProPeriode { get; init; }

    public int? Periodizitaet { get; init; }

    public int? KuendigungsfristTage { get; init; }

    public Guid? DokumentId { get; init; }

    public string? Notiz { get; init; }

    public DateTimeOffset ErstelltAm { get; init; }
}