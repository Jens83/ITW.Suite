using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;

namespace ITW.Dienstplan.Application.Wunschphase;

public sealed record SaveFreelancerMonatswunschCommand(
    Guid PeriodeId,
    string UserId,
    int GewuenschteDienste);

public sealed class SaveFreelancerMonatswunschResult
{
    private SaveFreelancerMonatswunschResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SaveFreelancerMonatswunschResult Erfolg()
        => new(true, null);

    public static SaveFreelancerMonatswunschResult Fehler(string message)
        => new(false, message);
}

public sealed class SaveFreelancerMonatswunschService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IFreelancerMonatswunschRepository _freelancerMonatswunschRepository;

    public SaveFreelancerMonatswunschService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IFreelancerMonatswunschRepository freelancerMonatswunschRepository)
    {
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository
            ?? throw new ArgumentNullException(nameof(dienstplanPeriodeRepository));

        _freelancerMonatswunschRepository = freelancerMonatswunschRepository
            ?? throw new ArgumentNullException(nameof(freelancerMonatswunschRepository));
    }

    public async Task<SaveFreelancerMonatswunschResult> ExecuteAsync(
        SaveFreelancerMonatswunschCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PeriodeId == Guid.Empty)
        {
            return SaveFreelancerMonatswunschResult.Fehler("Die Dienstplanperiode ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return SaveFreelancerMonatswunschResult.Fehler("Die UserId ist erforderlich.");
        }

        if (command.GewuenschteDienste is < 1 or > 3)
        {
            return SaveFreelancerMonatswunschResult.Fehler(
                "Für Freelancer sind nur 1, 2 oder 3 gewünschte Dienste pro Monat zulässig.");
        }

        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(
            command.PeriodeId,
            cancellationToken);

        if (periode is null)
        {
            return SaveFreelancerMonatswunschResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        if (!periode.WunschphaseIstOffen)
        {
            return SaveFreelancerMonatswunschResult.Fehler("Für diese Dienstplanperiode ist die Wunschphase nicht offen.");
        }

        var vorhanden = await _freelancerMonatswunschRepository.GetAsync(
            command.PeriodeId,
            command.UserId.Trim(),
            cancellationToken);

        if (vorhanden is null)
        {
            var neu = new FreelancerMonatswunsch(
                Guid.NewGuid(),
                command.PeriodeId,
                command.UserId.Trim(),
                command.GewuenschteDienste,
                DateTimeOffset.UtcNow);

            await _freelancerMonatswunschRepository.AddAsync(neu, cancellationToken);
            await _freelancerMonatswunschRepository.SaveChangesAsync(cancellationToken);

            return SaveFreelancerMonatswunschResult.Erfolg();
        }

        vorhanden.AktualisiereGewuenschteDienste(
            command.GewuenschteDienste,
            DateTimeOffset.UtcNow);

        await _freelancerMonatswunschRepository.SaveChangesAsync(cancellationToken);

        return SaveFreelancerMonatswunschResult.Erfolg();
    }
}