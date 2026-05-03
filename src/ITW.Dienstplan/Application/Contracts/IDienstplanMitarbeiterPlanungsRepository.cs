using ITW.Domain.Personnel.Enums;

namespace ITW.Dienstplan.Application.Contracts;

public sealed class DienstplanMitarbeiterPlanungsstammdaten
{
    public string UserId { get; init; } = string.Empty;

    public string AnzeigeName { get; init; } = string.Empty;

    public MitarbeiterBeschaeftigungsart Beschaeftigungsart { get; init; }

    public string HauptqualifikationCode { get; init; } = string.Empty;

    public string HauptqualifikationBezeichnung { get; init; } = string.Empty;

    public bool IstGesperrt { get; init; }

    public bool HatStammdatenprofil { get; init; }

    public bool HatItwProfil { get; init; }
}

public interface IDienstplanMitarbeiterPlanungsRepository
{
    Task<IReadOnlyList<DienstplanMitarbeiterPlanungsstammdaten>> GetAktivePlanungsmitarbeiterAsync(
        CancellationToken cancellationToken = default);
}