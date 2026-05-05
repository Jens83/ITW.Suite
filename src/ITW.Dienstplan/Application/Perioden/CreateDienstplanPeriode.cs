using System.Globalization;
using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;

namespace ITW.Dienstplan.Application.Perioden;

public sealed record CreateDienstplanPeriodeCommand(
    int Jahr,
    int Monat,
    bool WunschphaseDirektOeffnen,
    string ErstelltVonUserId);

public sealed class CreateDienstplanPeriodeResult
{
    private CreateDienstplanPeriodeResult(
        bool isSuccess,
        string? errorMessage,
        Guid? periodeId)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        PeriodeId = periodeId;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public Guid? PeriodeId { get; }

    public static CreateDienstplanPeriodeResult Erfolg(Guid periodeId)
        => new(true, null, periodeId);

    public static CreateDienstplanPeriodeResult Fehler(string message)
        => new(false, message, null);
}

public sealed class CreateDienstplanPeriodeService
{
    private static readonly CultureInfo DeutscheKultur = CultureInfo.GetCultureInfo("de-DE");

    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;

    public CreateDienstplanPeriodeService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository)
    {
        ArgumentNullException.ThrowIfNull(dienstplanPeriodeRepository);
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository;
    }

    public async Task<CreateDienstplanPeriodeResult> ExecuteAsync(
        CreateDienstplanPeriodeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Jahr < 2000 || command.Jahr > 2100)
        {
            return CreateDienstplanPeriodeResult.Fehler("Bitte ein gültiges Jahr auswählen.");
        }

        if (command.Monat < 1 || command.Monat > 12)
        {
            return CreateDienstplanPeriodeResult.Fehler("Bitte einen gültigen Monat auswählen.");
        }

        if (string.IsNullOrWhiteSpace(command.ErstelltVonUserId))
        {
            return CreateDienstplanPeriodeResult.Fehler("Die UserId des Erstellers ist erforderlich.");
        }

        var existiertBereits = await _dienstplanPeriodeRepository.ExistiertAsync(
            command.Jahr,
            command.Monat,
            cancellationToken);

        if (existiertBereits)
        {
            return CreateDienstplanPeriodeResult.Fehler(
                "Für den ausgewählten Monat existiert bereits eine Dienstplanperiode.");
        }

        var bezeichnung = BaueBezeichnung(command.Monat, command.Jahr);

        var periode = new DienstplanPeriode(
            Guid.NewGuid(),
            command.Jahr,
            command.Monat,
            bezeichnung,
            command.WunschphaseDirektOeffnen,
            command.ErstelltVonUserId,
            DateTimeOffset.UtcNow);

        await _dienstplanPeriodeRepository.AddAsync(periode, cancellationToken);
        await _dienstplanPeriodeRepository.SaveChangesAsync(cancellationToken);

        return CreateDienstplanPeriodeResult.Erfolg(periode.Id);
    }

    private static string BaueBezeichnung(int monat, int jahr)
    {
        var monatsname = DeutscheKultur.DateTimeFormat.GetMonthName(monat);

        if (string.IsNullOrWhiteSpace(monatsname))
        {
            throw new InvalidOperationException("Der Monatsname konnte nicht ermittelt werden.");
        }

        monatsname = DeutscheKultur.TextInfo.ToTitleCase(monatsname.Trim());

        return $"{monatsname} {jahr}";
    }
}