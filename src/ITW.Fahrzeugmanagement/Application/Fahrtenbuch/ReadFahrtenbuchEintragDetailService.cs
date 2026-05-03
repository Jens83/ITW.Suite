using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.Fahrtenbuch;

public sealed class FahrtenbuchEintragDetail
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

public sealed class ReadFahrtenbuchEintragDetailService
{
    private readonly IFahrtenbuchRepository _repository;

    public ReadFahrtenbuchEintragDetailService(IFahrtenbuchRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<FahrtenbuchEintragDetail?> ExecuteAsync(
        Guid fahrzeugId,
        Guid eintragId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty || eintragId == Guid.Empty)
        {
            return null;
        }

        var eintrag = await _repository.GetByIdAsync(
            eintragId,
            cancellationToken);

        if (eintrag is null || eintrag.FahrzeugId != fahrzeugId)
        {
            return null;
        }

        return new FahrtenbuchEintragDetail
        {
            EintragId = eintrag.Id,
            FahrzeugId = eintrag.FahrzeugId,
            FahrerUserId = eintrag.FahrerUserId,
            FahrerName = eintrag.FahrerName,
            BeifahrerName = eintrag.BeifahrerName,
            FahrtKategorie = eintrag.FahrtKategorie,
            Fahrtzweck = eintrag.Fahrtzweck,
            StartzeitUtc = eintrag.StartzeitUtc,
            EndzeitUtc = eintrag.EndzeitUtc,
            Startort = eintrag.Startort,
            Zielort = eintrag.Zielort,
            StartKilometerstand = eintrag.StartKilometerstand,
            EndKilometerstand = eintrag.EndKilometerstand,
            GefahreneKilometer = eintrag.GefahreneKilometer,
            TankmengeLiter = eintrag.TankmengeLiter,
            KilometerstandBeimTanken = eintrag.KilometerstandBeimTanken,
            Status = eintrag.Status,
            IstAutomatischVorbelegt = eintrag.IstAutomatischVorbelegt,
            Bemerkung = eintrag.Bemerkung
        };
    }
}