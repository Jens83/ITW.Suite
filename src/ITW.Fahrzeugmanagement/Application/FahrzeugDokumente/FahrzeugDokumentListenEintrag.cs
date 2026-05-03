using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugDokumente;

public sealed class FahrzeugDokumentListenEintrag
{
    public Guid DokumentId { get; init; }

    public Guid FahrzeugId { get; init; }

    public FahrzeugDokumentKategorie Kategorie { get; init; }

    public string Dateiname { get; init; } = string.Empty;

    public string Bezeichnung { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public DateOnly? GueltigBis { get; init; }

    public DateTimeOffset HochgeladenAm { get; init; }
}