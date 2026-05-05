using ITW.Dienstplan.Application.Contracts;

namespace ITW.Dienstplan.Application.Perioden;

public sealed class DienstplanPeriodeListenEintrag
{
    public Guid Id { get; init; }

    public int Jahr { get; init; }

    public int Monat { get; init; }

    public string Bezeichnung { get; init; } = string.Empty;

    public bool WunschphaseIstOffen { get; init; }

    public bool PlanIstFreigegeben { get; init; }

    public DateTimeOffset? PlanFreigegebenAm { get; init; }

    public DateTimeOffset ErstelltAm { get; init; }
}

public sealed class ReadDienstplanperiodenResult
{
    private ReadDienstplanperiodenResult(
        bool isSuccess,
        string? errorMessage,
        IReadOnlyList<DienstplanPeriodeListenEintrag> perioden)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Perioden = perioden;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public IReadOnlyList<DienstplanPeriodeListenEintrag> Perioden { get; }

    public static ReadDienstplanperiodenResult Erfolg(
        IReadOnlyList<DienstplanPeriodeListenEintrag> perioden)
        => new(true, null, perioden);

    public static ReadDienstplanperiodenResult Fehler(string message)
        => new(false, message, Array.Empty<DienstplanPeriodeListenEintrag>());
}

public sealed class ReadDienstplanperiodenService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;

    public ReadDienstplanperiodenService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository)
    {
        ArgumentNullException.ThrowIfNull(dienstplanPeriodeRepository);
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository;
    }

    public async Task<ReadDienstplanperiodenResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var perioden = await _dienstplanPeriodeRepository.GetAlleAsync(cancellationToken);

        var result = perioden
            .Select(x => new DienstplanPeriodeListenEintrag
            {
                Id = x.Id,
                Jahr = x.Jahr,
                Monat = x.Monat,
                Bezeichnung = x.Bezeichnung,
                WunschphaseIstOffen = x.WunschphaseIstOffen,
                PlanIstFreigegeben = x.PlanIstFreigegeben,
                PlanFreigegebenAm = x.PlanFreigegebenAm,
                ErstelltAm = x.ErstelltAm
            })
            .ToArray();

        return ReadDienstplanperiodenResult.Erfolg(result);
    }
}