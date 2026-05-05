using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.Fahrzeuge;

public sealed class FahrzeugDetail
{
    public Guid FahrzeugId { get; init; }

    public string InterneNummer { get; init; } = string.Empty;

    public string Kennzeichen { get; init; } = string.Empty;

    public string Vin { get; init; } = string.Empty;

    public string Hersteller { get; init; } = string.Empty;

    public string Modell { get; init; } = string.Empty;

    public string Fahrzeugtyp { get; init; } = string.Empty;

    public int? Baujahr { get; init; }

    public DateOnly? Erstzulassung { get; init; }

    public Kraftstoffart Kraftstoffart { get; init; }

    public int? LeistungKw { get; init; }

    public int KilometerstandAktuell { get; init; }

    public FahrzeugStatus Status { get; init; }

    public string? StandardStandort { get; init; }
}

public sealed class ReadFahrzeugDetailService
{
    private readonly IFahrzeugRepository _repository;

    public ReadFahrzeugDetailService(IFahrzeugRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<FahrzeugDetail?> ExecuteAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return null;
        }

        var fahrzeug = await _repository.GetFahrzeugByIdAsync(
            fahrzeugId,
            cancellationToken);

        if (fahrzeug is null)
        {
            return null;
        }

        return new FahrzeugDetail
        {
            FahrzeugId = fahrzeug.Id,
            InterneNummer = fahrzeug.InterneNummer,
            Kennzeichen = fahrzeug.Kennzeichen,
            Vin = fahrzeug.Vin,
            Hersteller = fahrzeug.Hersteller,
            Modell = fahrzeug.Modell,
            Fahrzeugtyp = fahrzeug.Fahrzeugtyp,
            Baujahr = fahrzeug.Baujahr,
            Erstzulassung = fahrzeug.Erstzulassung,
            Kraftstoffart = fahrzeug.Kraftstoffart,
            LeistungKw = fahrzeug.LeistungKw,
            KilometerstandAktuell = fahrzeug.KilometerstandAktuell,
            Status = fahrzeug.Status,
            StandardStandort = fahrzeug.StandardStandort
        };
    }
}