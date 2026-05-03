using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Application.Planung;

namespace ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Write;

public sealed record SaveWachleiterTagesplanungCommand(
    Guid PeriodeId,
    DateOnly Datum,
    string? ArztUserId,
    string? Notfallsanitaeter1UserId,
    string? Notfallsanitaeter2UserId,
    bool PlanungLeeren,
    string BearbeitetVonUserId);

public sealed class SaveWachleiterTagesplanungResult
{
    private SaveWachleiterTagesplanungResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SaveWachleiterTagesplanungResult Erfolg()
        => new(true, null);

    public static SaveWachleiterTagesplanungResult Fehler(string message)
        => new(false, message);
}

public sealed class SaveWachleiterTagesplanungService
{
    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly IMitarbeiterUrlaubszeitraumRepository _mitarbeiterUrlaubszeitraumRepository;
    private readonly SaveGeplanterDienstTagService _saveGeplanterDienstTagService;

    public SaveWachleiterTagesplanungService(
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        IMitarbeiterUrlaubszeitraumRepository mitarbeiterUrlaubszeitraumRepository,
        SaveGeplanterDienstTagService saveGeplanterDienstTagService)
    {
        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService
            ?? throw new ArgumentNullException(nameof(readItwMitarbeiterprofileService));

        _mitarbeiterUrlaubszeitraumRepository = mitarbeiterUrlaubszeitraumRepository
            ?? throw new ArgumentNullException(nameof(mitarbeiterUrlaubszeitraumRepository));

        _saveGeplanterDienstTagService = saveGeplanterDienstTagService
            ?? throw new ArgumentNullException(nameof(saveGeplanterDienstTagService));
    }

    public async Task<SaveWachleiterTagesplanungResult> ExecuteAsync(
        SaveWachleiterTagesplanungCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PeriodeId == Guid.Empty)
        {
            return SaveWachleiterTagesplanungResult.Fehler("Die ausgewählte Dienstplanperiode ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.BearbeitetVonUserId))
        {
            return SaveWachleiterTagesplanungResult.Fehler("Der Bearbeiter ist ungültig.");
        }

        var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);

        if (!mitarbeiterResult.IsSuccess)
        {
            return SaveWachleiterTagesplanungResult.Fehler(
                mitarbeiterResult.ErrorMessage ?? "Die Mitarbeiterdaten konnten nicht geladen werden.");
        }

        var aktiveProfile = mitarbeiterResult.Profile
            .Where(x => !x.IstGesperrt && x.HatProfil)
            .ToArray();

        var festangestellteImUrlaubIds = await ErmittleFestangestellteImUrlaubUserIdsAsync(
            aktiveProfile,
            command.Datum,
            cancellationToken);

        var arztIds = aktiveProfile
            .Where(IstArzt)
            .Select(x => x.UserId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var nfsIds = aktiveProfile
            .Where(IstNotfallsanitaeter)
            .Select(x => x.UserId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var arztUserId = command.PlanungLeeren ? null : NormalisiereUserId(command.ArztUserId);
        var notfallsanitaeter1UserId = command.PlanungLeeren ? null : NormalisiereUserId(command.Notfallsanitaeter1UserId);
        var notfallsanitaeter2UserId = command.PlanungLeeren ? null : NormalisiereUserId(command.Notfallsanitaeter2UserId);

        if (!command.PlanungLeeren)
        {
            if (!string.IsNullOrWhiteSpace(arztUserId) && !arztIds.Contains(arztUserId))
            {
                return SaveWachleiterTagesplanungResult.Fehler("Der ausgewählte Arzt ist für die Tagesplanung ungültig.");
            }

            if (!string.IsNullOrWhiteSpace(notfallsanitaeter1UserId) && !nfsIds.Contains(notfallsanitaeter1UserId))
            {
                return SaveWachleiterTagesplanungResult.Fehler("Der ausgewählte Notfallsanitäter für Slot 1 ist ungültig.");
            }

            if (!string.IsNullOrWhiteSpace(notfallsanitaeter2UserId) && !nfsIds.Contains(notfallsanitaeter2UserId))
            {
                return SaveWachleiterTagesplanungResult.Fehler("Der ausgewählte Notfallsanitäter für Slot 2 ist ungültig.");
            }

            if (!string.IsNullOrWhiteSpace(arztUserId) && festangestellteImUrlaubIds.Contains(arztUserId))
            {
                return SaveWachleiterTagesplanungResult.Fehler("Ein festangestellter Mitarbeiter mit hinterlegtem Urlaub kann nicht regulär als Arzt eingeplant werden.");
            }

            if (!string.IsNullOrWhiteSpace(notfallsanitaeter1UserId) && festangestellteImUrlaubIds.Contains(notfallsanitaeter1UserId))
            {
                return SaveWachleiterTagesplanungResult.Fehler("Ein festangestellter Mitarbeiter mit hinterlegtem Urlaub kann nicht regulär als NFS 1 eingeplant werden.");
            }

            if (!string.IsNullOrWhiteSpace(notfallsanitaeter2UserId) && festangestellteImUrlaubIds.Contains(notfallsanitaeter2UserId))
            {
                return SaveWachleiterTagesplanungResult.Fehler("Ein festangestellter Mitarbeiter mit hinterlegtem Urlaub kann nicht regulär als NFS 2 eingeplant werden.");
            }

            var doppelteAuswahl = new[]
                {
                    arztUserId,
                    notfallsanitaeter1UserId,
                    notfallsanitaeter2UserId
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .GroupBy(x => x!, StringComparer.OrdinalIgnoreCase)
                .Any(x => x.Count() > 1);

            if (doppelteAuswahl)
            {
                return SaveWachleiterTagesplanungResult.Fehler("Ein Mitarbeiter darf an einem Tag nicht mehrfach derselben Besetzung zugeordnet werden.");
            }
        }

        var result = await _saveGeplanterDienstTagService.ExecuteAsync(
            new SaveGeplanterDienstTagCommand(
                command.PeriodeId,
                command.Datum,
                arztUserId,
                notfallsanitaeter1UserId,
                notfallsanitaeter2UserId,
                command.BearbeitetVonUserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return SaveWachleiterTagesplanungResult.Fehler(
                result.ErrorMessage ?? "Die Tagesplanung konnte nicht gespeichert werden.");
        }

        return SaveWachleiterTagesplanungResult.Erfolg();
    }

    private async Task<HashSet<string>> ErmittleFestangestellteImUrlaubUserIdsAsync(
        IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> aktiveProfile,
        DateOnly datum,
        CancellationToken cancellationToken)
    {
        var userIdsMitHinterlegtemUrlaub = await _mitarbeiterUrlaubszeitraumRepository.GetAktiveUserIdsFuerDatumAsync(
            datum,
            cancellationToken);

        var festangestellteIds = aktiveProfile
            .Where(x => x.Beschaeftigungsart == Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt)
            .Select(x => x.UserId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return userIdsMitHinterlegtemUrlaub
            .Where(x => festangestellteIds.Contains(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool IstArzt(ItwMitarbeiterprofilUebersichtDto profil)
        => string.Equals(profil.Hauptqualifikation, "Arzt", StringComparison.OrdinalIgnoreCase);

    private static bool IstNotfallsanitaeter(ItwMitarbeiterprofilUebersichtDto profil)
        => string.Equals(profil.Hauptqualifikation, "Notfallsanitäter", StringComparison.OrdinalIgnoreCase)
           || string.Equals(profil.Hauptqualifikation, "Notfallsanitaeter", StringComparison.OrdinalIgnoreCase);

    private static string? NormalisiereUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return userId.Trim();
    }
}