using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Wunschphase;

public sealed class AktiveWunschphaseDto
{
    public Guid PeriodeId { get; init; }

    public string Bezeichnung { get; init; } = string.Empty;

    public int Jahr { get; init; }

    public int Monat { get; init; }

    public IReadOnlyList<int> WunschTage { get; init; } = Array.Empty<int>();

    public IReadOnlyList<int> NichtVerfuegbareTage { get; init; } = Array.Empty<int>();

    public IReadOnlyList<int> GewaehlteTage => WunschTage;

    public int? FreelancerGewuenschteDienste { get; init; }
}

public sealed class ReadAktiveWunschphaseResult
{
    private ReadAktiveWunschphaseResult(
        bool isSuccess,
        string? errorMessage,
        AktiveWunschphaseDto? aktiveWunschphase)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        AktiveWunschphase = aktiveWunschphase;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public AktiveWunschphaseDto? AktiveWunschphase { get; }

    public static ReadAktiveWunschphaseResult Erfolg(AktiveWunschphaseDto? aktiveWunschphase)
        => new(true, null, aktiveWunschphase);

    public static ReadAktiveWunschphaseResult Fehler(string message)
        => new(false, message, null);
}

public sealed class ReadAktiveWunschphaseService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IDienstwunschRepository _dienstwunschRepository;
    private readonly IFreelancerMonatswunschRepository _freelancerMonatswunschRepository;

    public ReadAktiveWunschphaseService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IDienstwunschRepository dienstwunschRepository,
        IFreelancerMonatswunschRepository freelancerMonatswunschRepository)
    {
        ArgumentNullException.ThrowIfNull(dienstplanPeriodeRepository);
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository;

        ArgumentNullException.ThrowIfNull(dienstwunschRepository);
        _dienstwunschRepository = dienstwunschRepository;

        ArgumentNullException.ThrowIfNull(freelancerMonatswunschRepository);
        _freelancerMonatswunschRepository = freelancerMonatswunschRepository;
    }

    public async Task<ReadAktiveWunschphaseResult> ExecuteAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ReadAktiveWunschphaseResult.Fehler("Die UserId ist erforderlich.");
        }

        var periode = await _dienstplanPeriodeRepository.GetAktuelleOffeneAsync(cancellationToken);
        if (periode is null)
        {
            return ReadAktiveWunschphaseResult.Erfolg(null);
        }

        var wuensche = await _dienstwunschRepository.GetAlleFuerBenutzerAsync(
            periode.Id,
            userId.Trim(),
            cancellationToken);

        var freelancerMonatswunsch = await _freelancerMonatswunschRepository.GetAsync(
            periode.Id,
            userId.Trim(),
            cancellationToken);

        var dto = new AktiveWunschphaseDto
        {
            PeriodeId = periode.Id,
            Bezeichnung = periode.Bezeichnung,
            Jahr = periode.Jahr,
            Monat = periode.Monat,
            WunschTage = wuensche
                .Where(x => x.WunschTyp == DienstwunschTyp.Wunsch)
                .Select(x => x.WunschDatum.Day)
                .OrderBy(x => x)
                .ToArray(),
            NichtVerfuegbareTage = wuensche
                .Where(x => x.WunschTyp == DienstwunschTyp.NichtVerfuegbar)
                .Select(x => x.WunschDatum.Day)
                .OrderBy(x => x)
                .ToArray(),
            FreelancerGewuenschteDienste = freelancerMonatswunsch?.GewuenschteDienste
        };

        return ReadAktiveWunschphaseResult.Erfolg(dto);
    }
}