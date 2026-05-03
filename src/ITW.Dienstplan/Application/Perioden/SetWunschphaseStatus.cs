using ITW.Dienstplan.Application.Contracts;

namespace ITW.Dienstplan.Application.Perioden;

public sealed record SetWunschphaseStatusCommand(
    Guid PeriodeId,
    bool Oeffnen);

public sealed class SetWunschphaseStatusResult
{
    private SetWunschphaseStatusResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SetWunschphaseStatusResult Erfolg()
        => new(true, null);

    public static SetWunschphaseStatusResult Fehler(string message)
        => new(false, message);
}

public sealed class SetWunschphaseStatusService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;

    public SetWunschphaseStatusService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository)
    {
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository
            ?? throw new ArgumentNullException(nameof(dienstplanPeriodeRepository));
    }

    public async Task<SetWunschphaseStatusResult> ExecuteAsync(
        SetWunschphaseStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PeriodeId == Guid.Empty)
        {
            return SetWunschphaseStatusResult.Fehler("Die Dienstplanperiode ist ungültig.");
        }

        var zielPeriode = await _dienstplanPeriodeRepository.GetByIdAsync(
            command.PeriodeId,
            cancellationToken);

        if (zielPeriode is null)
        {
            return SetWunschphaseStatusResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        if (command.Oeffnen)
        {
            var offenePerioden = await _dienstplanPeriodeRepository.GetOffeneAsync(cancellationToken);

            foreach (var offenePeriode in offenePerioden.Where(x => x.Id != zielPeriode.Id))
            {
                offenePeriode.WunschphaseSchliessen();
            }

            zielPeriode.WunschphaseOeffnen();
            await _dienstplanPeriodeRepository.SaveChangesAsync(cancellationToken);

            return SetWunschphaseStatusResult.Erfolg();
        }

        zielPeriode.WunschphaseSchliessen();
        await _dienstplanPeriodeRepository.SaveChangesAsync(cancellationToken);

        return SetWunschphaseStatusResult.Erfolg();
    }
}