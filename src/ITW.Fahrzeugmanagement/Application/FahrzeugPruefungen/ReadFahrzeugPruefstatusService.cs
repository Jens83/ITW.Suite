using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugPruefungen;

public sealed class FahrzeugPruefungListenEintrag
{
    public Guid PruefungId { get; init; }

    public Guid FahrzeugId { get; init; }

    public FahrzeugPruefungTyp Typ { get; init; }

    public DateOnly FaelligAm { get; init; }

    public DateOnly? LetzteErledigungAm { get; init; }

    public string? Bemerkung { get; init; }
}

public sealed class ReadFahrzeugPruefstatusResult
{
    public IReadOnlyList<FahrzeugPruefungListenEintrag> Pruefungen { get; init; } = [];
}

public sealed class ReadFahrzeugPruefstatusService
{
    private readonly IFahrzeugPruefungRepository _repository;

    public ReadFahrzeugPruefstatusService(IFahrzeugPruefungRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<ReadFahrzeugPruefstatusResult> ExecuteAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return new ReadFahrzeugPruefstatusResult();
        }

        var pruefungen = await _repository.GetByFahrzeugIdAsync(
            fahrzeugId,
            cancellationToken);

        return new ReadFahrzeugPruefstatusResult
        {
            Pruefungen = pruefungen
                .Select(x => new FahrzeugPruefungListenEintrag
                {
                    PruefungId = x.Id,
                    FahrzeugId = x.FahrzeugId,
                    Typ = x.Typ,
                    FaelligAm = x.FaelligAm,
                    LetzteErledigungAm = x.LetzteErledigungAm,
                    Bemerkung = x.Bemerkung
                })
                .ToList()
        };
    }
}