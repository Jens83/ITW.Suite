using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Wunschphase;
using ITW.Dienstplan.Domain.Enums;
using System.Globalization;

namespace ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Write;

public sealed record ToggleMitarbeiterWunschCommand(
    Guid PeriodeId,
    string UserId,
    DateOnly Datum,
    string? WunschTyp);

public sealed class ToggleMitarbeiterWunschResult
{
    private ToggleMitarbeiterWunschResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static ToggleMitarbeiterWunschResult Erfolg()
        => new(true, null);

    public static ToggleMitarbeiterWunschResult Fehler(string message)
        => new(false, message);
}

public sealed class ToggleMitarbeiterWunschService
{
    private static readonly CultureInfo DeutscheKultur = CultureInfo.GetCultureInfo("de-DE");

    private readonly IMitarbeiterUrlaubszeitraumRepository _mitarbeiterUrlaubszeitraumRepository;
    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly IFreelancerMonatswunschRepository _freelancerMonatswunschRepository;
    private readonly ToggleDienstwunschService _toggleDienstwunschService;

    public ToggleMitarbeiterWunschService(
        IMitarbeiterUrlaubszeitraumRepository mitarbeiterUrlaubszeitraumRepository,
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        IFreelancerMonatswunschRepository freelancerMonatswunschRepository,
        ToggleDienstwunschService toggleDienstwunschService)
    {
        ArgumentNullException.ThrowIfNull(mitarbeiterUrlaubszeitraumRepository);
        _mitarbeiterUrlaubszeitraumRepository = mitarbeiterUrlaubszeitraumRepository;

        ArgumentNullException.ThrowIfNull(readItwMitarbeiterprofileService);
        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService;

        ArgumentNullException.ThrowIfNull(freelancerMonatswunschRepository);
        _freelancerMonatswunschRepository = freelancerMonatswunschRepository;

        ArgumentNullException.ThrowIfNull(toggleDienstwunschService);
        _toggleDienstwunschService = toggleDienstwunschService;
    }

    public async Task<ToggleMitarbeiterWunschResult> ExecuteAsync(
        ToggleMitarbeiterWunschCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PeriodeId == Guid.Empty)
        {
            return ToggleMitarbeiterWunschResult.Fehler("Die aktive Dienstplanperiode ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return ToggleMitarbeiterWunschResult.Fehler("Der aktuelle Benutzer ist ungültig.");
        }

        var userIdsImUrlaub = await _mitarbeiterUrlaubszeitraumRepository.GetAktiveUserIdsFuerDatumAsync(
            command.Datum,
            cancellationToken);

        if (userIdsImUrlaub.Contains(command.UserId, StringComparer.OrdinalIgnoreCase))
        {
            return ToggleMitarbeiterWunschResult.Fehler(
                $"Der {command.Datum.ToString("dd.MM.yyyy", DeutscheKultur)} liegt in Ihrem hinterlegten Urlaub und kann nicht ausgewählt werden.");
        }

        var beschaeftigungsart = await ErmittleBeschaeftigungsartAsync(
            command.UserId,
            cancellationToken);

        var istFreelancer = beschaeftigungsart == Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer;
        var istHonorarkraft = beschaeftigungsart == Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Honorarkraft;
        var darfNurWuenscheSetzen = istFreelancer || istHonorarkraft;

        if (istFreelancer)
        {
            var freelancerMonatswunsch = await _freelancerMonatswunschRepository.GetAsync(
                command.PeriodeId,
                command.UserId,
                cancellationToken);

            if (freelancerMonatswunsch is null)
            {
                return ToggleMitarbeiterWunschResult.Fehler(
                    "Freelancer müssen zuerst die gewünschte Monatsanzahl speichern, bevor Wunschtage gewählt werden können.");
            }
        }

        DienstwunschTyp zielTyp;

        if (darfNurWuenscheSetzen)
        {
            if (!string.IsNullOrWhiteSpace(command.WunschTyp) &&
                !string.Equals(command.WunschTyp, nameof(DienstwunschTyp.Wunsch), StringComparison.OrdinalIgnoreCase))
            {
                return ToggleMitarbeiterWunschResult.Fehler(
                    "Für diese Beschäftigungsart können nur Wunschtage markiert werden.");
            }

            zielTyp = DienstwunschTyp.Wunsch;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(command.WunschTyp))
            {
                zielTyp = DienstwunschTyp.Wunsch;
            }
            else if (!Enum.TryParse(command.WunschTyp, ignoreCase: true, out zielTyp))
            {
                return ToggleMitarbeiterWunschResult.Fehler("Der ausgewählte Kalendermodus ist ungültig.");
            }
        }

        var result = await _toggleDienstwunschService.ExecuteAsync(
            new ToggleDienstwunschCommand(
                command.PeriodeId,
                command.UserId,
                command.Datum,
                zielTyp),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return ToggleMitarbeiterWunschResult.Fehler(
                result.ErrorMessage ?? "Der Wunsch konnte nicht gespeichert werden.");
        }

        return ToggleMitarbeiterWunschResult.Erfolg();
    }

    private async Task<Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart> ErmittleBeschaeftigungsartAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var result = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Unbekannt;
        }

        var profil = result.Profile.FirstOrDefault(x =>
            string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

        return profil?.Beschaeftigungsart
            ?? Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Unbekannt;
    }
}