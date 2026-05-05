using ITW.Application.Personnel.ProfileQueries;
using ITW.Dienstplan.Application.Wunschphase;

namespace ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Write;

public sealed record SaveFreelancerMonatswunschMitarbeiterCommand(
    Guid PeriodeId,
    string UserId,
    int GewuenschteDienste);

public sealed class SaveFreelancerMonatswunschMitarbeiterResult
{
    private SaveFreelancerMonatswunschMitarbeiterResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SaveFreelancerMonatswunschMitarbeiterResult Erfolg()
        => new(true, null);

    public static SaveFreelancerMonatswunschMitarbeiterResult Fehler(string message)
        => new(false, message);
}

public sealed class SaveFreelancerMonatswunschMitarbeiterService
{
    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly SaveFreelancerMonatswunschService _saveFreelancerMonatswunschService;

    public SaveFreelancerMonatswunschMitarbeiterService(
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        SaveFreelancerMonatswunschService saveFreelancerMonatswunschService)
    {
        ArgumentNullException.ThrowIfNull(readItwMitarbeiterprofileService);
        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService;

        ArgumentNullException.ThrowIfNull(saveFreelancerMonatswunschService);
        _saveFreelancerMonatswunschService = saveFreelancerMonatswunschService;
    }

    public async Task<SaveFreelancerMonatswunschMitarbeiterResult> ExecuteAsync(
        SaveFreelancerMonatswunschMitarbeiterCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PeriodeId == Guid.Empty)
        {
            return SaveFreelancerMonatswunschMitarbeiterResult.Fehler("Die aktive Dienstplanperiode ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return SaveFreelancerMonatswunschMitarbeiterResult.Fehler("Der aktuelle Benutzer ist ungültig.");
        }

        var beschaeftigungsart = await ErmittleBeschaeftigungsartAsync(
            command.UserId,
            cancellationToken);

        if (beschaeftigungsart != Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer)
        {
            return SaveFreelancerMonatswunschMitarbeiterResult.Fehler(
                "Die gewünschte Monatsanzahl kann nur für Freelancer gespeichert werden.");
        }

        var result = await _saveFreelancerMonatswunschService.ExecuteAsync(
            new SaveFreelancerMonatswunschCommand(
                command.PeriodeId,
                command.UserId,
                command.GewuenschteDienste),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return SaveFreelancerMonatswunschMitarbeiterResult.Fehler(
                result.ErrorMessage ?? "Die gewünschte Monatsanzahl konnte nicht gespeichert werden.");
        }

        return SaveFreelancerMonatswunschMitarbeiterResult.Erfolg();
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