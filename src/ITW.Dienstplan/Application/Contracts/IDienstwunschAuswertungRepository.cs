namespace ITW.Dienstplan.Application.Contracts;

public sealed class DienstwunschTagesstatistik
{
    public DateOnly Datum { get; init; }

    public int AnzahlWuenscheGesamt { get; init; }

    public int AnzahlAerzte { get; init; }

    public int AnzahlNotfallsanitaeter { get; init; }
}

public interface IDienstwunschAuswertungRepository
{
    Task<IReadOnlyList<DienstwunschTagesstatistik>> GetTagesstatistikAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default);
}