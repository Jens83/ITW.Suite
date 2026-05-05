using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Write;

public sealed record SaveWachleiterAusfallCommand(
    Guid PeriodeId,
    DateOnly Datum,
    string SlotCode,
    string? AusfallGrundCode,
    string? VertretungsUserId,
    string BearbeitetVonUserId);

public sealed class SaveWachleiterAusfallResult
{
    private SaveWachleiterAusfallResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SaveWachleiterAusfallResult Erfolg()
        => new(true, null);

    public static SaveWachleiterAusfallResult Fehler(string message)
        => new(false, message);
}

public sealed class SaveWachleiterAusfallService
{
    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly IDienstbesetzungsAusfallRepository _dienstbesetzungsAusfallRepository;
    private readonly IMitarbeiterUrlaubsanspruchRepository _mitarbeiterUrlaubsanspruchRepository;
    private readonly SaveDienstbesetzungsAusfallService _saveDienstbesetzungsAusfallService;

    public SaveWachleiterAusfallService(
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        IGeplanterDienstTagRepository geplanterDienstTagRepository,
        IDienstbesetzungsAusfallRepository dienstbesetzungsAusfallRepository,
        IMitarbeiterUrlaubsanspruchRepository mitarbeiterUrlaubsanspruchRepository,
        SaveDienstbesetzungsAusfallService saveDienstbesetzungsAusfallService)
    {
        ArgumentNullException.ThrowIfNull(readItwMitarbeiterprofileService);
        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService;

        ArgumentNullException.ThrowIfNull(geplanterDienstTagRepository);
        _geplanterDienstTagRepository = geplanterDienstTagRepository;

        ArgumentNullException.ThrowIfNull(dienstbesetzungsAusfallRepository);
        _dienstbesetzungsAusfallRepository = dienstbesetzungsAusfallRepository;

        ArgumentNullException.ThrowIfNull(mitarbeiterUrlaubsanspruchRepository);
        _mitarbeiterUrlaubsanspruchRepository = mitarbeiterUrlaubsanspruchRepository;

        ArgumentNullException.ThrowIfNull(saveDienstbesetzungsAusfallService);
        _saveDienstbesetzungsAusfallService = saveDienstbesetzungsAusfallService;
    }

    public async Task<SaveWachleiterAusfallResult> ExecuteAsync(
        SaveWachleiterAusfallCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.PeriodeId == Guid.Empty)
        {
            return SaveWachleiterAusfallResult.Fehler("Die ausgewählte Dienstplanperiode ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.BearbeitetVonUserId))
        {
            return SaveWachleiterAusfallResult.Fehler("Der Bearbeiter ist ungültig.");
        }

        if (!Enum.TryParse<DienstbesetzungsSlotCode>(command.SlotCode, ignoreCase: true, out var besetzungsSlotCode))
        {
            return SaveWachleiterAusfallResult.Fehler("Der ausgewählte Besetzungsslot ist ungültig.");
        }

        DienstausfallGrundCode? grundCode = null;

        if (!string.IsNullOrWhiteSpace(command.AusfallGrundCode))
        {
            if (!Enum.TryParse<DienstausfallGrundCode>(command.AusfallGrundCode, ignoreCase: true, out var parsedGrundCode))
            {
                return SaveWachleiterAusfallResult.Fehler("Der ausgewählte Ausfallgrund ist ungültig.");
            }

            grundCode = parsedGrundCode;
        }

        var vertretungsUserId = NormalisiereUserId(command.VertretungsUserId);

        var geplanterDienstTag = await _geplanterDienstTagRepository.GetAsync(
            command.PeriodeId,
            command.Datum,
            cancellationToken);

        var tagesAusfaelle = await _dienstbesetzungsAusfallRepository.GetAlleFuerTagAsync(
            command.PeriodeId,
            command.Datum,
            cancellationToken);

        var bestehenderAusfall = tagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == besetzungsSlotCode);

        var urspruenglichGeplanteUserId = bestehenderAusfall?.UrspruenglichGeplanterUserId
            ?? ErmittleGeplantenUserIdFuerSlot(geplanterDienstTag, besetzungsSlotCode);

        if (!string.IsNullOrWhiteSpace(vertretungsUserId) || grundCode == DienstausfallGrundCode.Urlaub)
        {
            var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);

            if (!mitarbeiterResult.IsSuccess)
            {
                return SaveWachleiterAusfallResult.Fehler(
                    mitarbeiterResult.ErrorMessage ?? "Die Mitarbeiterdaten konnten nicht geladen werden.");
            }

            var aktiveProfile = mitarbeiterResult.Profile
                .Where(x => !x.IstGesperrt && x.HatProfil)
                .ToDictionary(x => x.UserId, StringComparer.OrdinalIgnoreCase);

            if (grundCode == DienstausfallGrundCode.Urlaub && !string.IsNullOrWhiteSpace(urspruenglichGeplanteUserId))
            {
                if (aktiveProfile.TryGetValue(urspruenglichGeplanteUserId, out var urspruenglichGeplanter))
                {
                    if (urspruenglichGeplanter.Beschaeftigungsart == Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Honorarkraft)
                    {
                        return SaveWachleiterAusfallResult.Fehler("Honorarkräfte haben keinen Urlaubsanspruch und können nicht auf Urlaub gesetzt werden.");
                    }

                    var istFreelancer = urspruenglichGeplanter.Beschaeftigungsart
                        == Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer;

                    if (istFreelancer)
                    {
                        var hinterlegterAnspruch = await _mitarbeiterUrlaubsanspruchRepository.GetAsync(
                            urspruenglichGeplanteUserId,
                            command.Datum.Year,
                            cancellationToken);

                        var maximalErlaubteUrlaubstage = hinterlegterAnspruch?.Anspruchstage
                            ?? ErmittleStandardUrlaubsanspruch(urspruenglichGeplanter.Beschaeftigungsart);

                        var bereitsGenommeneUrlaubstage = await _dienstbesetzungsAusfallRepository.GetUrlaubstageFuerBenutzerUndJahrAsync(
                            urspruenglichGeplanteUserId,
                            command.Datum.Year,
                            cancellationToken);

                        var aktuellerTagIstSchonUrlaub = bereitsGenommeneUrlaubstage.Contains(command.Datum);
                        var anzahlUrlaubstage = bereitsGenommeneUrlaubstage.Count;

                        if (!aktuellerTagIstSchonUrlaub && anzahlUrlaubstage >= maximalErlaubteUrlaubstage)
                        {
                            return SaveWachleiterAusfallResult.Fehler(
                                $"Für Freelancer sind im Jahr {command.Datum.Year} bereits alle hinterlegten Urlaubstage ({maximalErlaubteUrlaubstage}) aufgebraucht.");
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(vertretungsUserId))
            {
                if (!aktiveProfile.TryGetValue(vertretungsUserId, out var vertretung))
                {
                    return SaveWachleiterAusfallResult.Fehler("Die ausgewählte Vertretung ist ungültig.");
                }

                var vertretungIstZulaessig =
                    besetzungsSlotCode == DienstbesetzungsSlotCode.Arzt
                        ? IstArzt(vertretung)
                        : IstNotfallsanitaeter(vertretung);

                if (!vertretungIstZulaessig)
                {
                    return SaveWachleiterAusfallResult.Fehler("Die ausgewählte Vertretung passt fachlich nicht zum ausgewählten Besetzungsslot.");
                }

                if (!string.IsNullOrWhiteSpace(urspruenglichGeplanteUserId) &&
                    string.Equals(vertretungsUserId, urspruenglichGeplanteUserId, StringComparison.OrdinalIgnoreCase))
                {
                    return SaveWachleiterAusfallResult.Fehler("Die ursprünglich geplante Person kann nicht als eigene Vertretung gesetzt werden.");
                }

                var andereSlotUserIds = ErmittleAndereGeplanteUserIds(geplanterDienstTag, besetzungsSlotCode);

                if (andereSlotUserIds.Contains(vertretungsUserId, StringComparer.OrdinalIgnoreCase))
                {
                    return SaveWachleiterAusfallResult.Fehler("Die ausgewählte Vertretung ist bereits in einem anderen Slot dieses Tages eingeplant.");
                }
            }
        }

        var result = await _saveDienstbesetzungsAusfallService.ExecuteAsync(
            new SaveDienstbesetzungsAusfallCommand(
                command.PeriodeId,
                command.Datum,
                besetzungsSlotCode,
                grundCode,
                vertretungsUserId,
                command.BearbeitetVonUserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return SaveWachleiterAusfallResult.Fehler(
                result.ErrorMessage ?? "Der Ausfall konnte nicht gespeichert werden.");
        }

        return SaveWachleiterAusfallResult.Erfolg();
    }

    private static string? ErmittleGeplantenUserIdFuerSlot(
        GeplanterDienstTag? geplanterDienstTag,
        DienstbesetzungsSlotCode slotCode)
    {
        if (geplanterDienstTag is null)
        {
            return null;
        }

        return slotCode switch
        {
            DienstbesetzungsSlotCode.Arzt => geplanterDienstTag.ArztUserId,
            DienstbesetzungsSlotCode.Notfallsanitaeter1 => geplanterDienstTag.Notfallsanitaeter1UserId,
            DienstbesetzungsSlotCode.Notfallsanitaeter2 => geplanterDienstTag.Notfallsanitaeter2UserId,
            _ => null
        };
    }

    private static IReadOnlyList<string> ErmittleAndereGeplanteUserIds(
        GeplanterDienstTag? geplanterDienstTag,
        DienstbesetzungsSlotCode aktuellerSlot)
    {
        if (geplanterDienstTag is null)
        {
            return Array.Empty<string>();
        }

        return new[]
            {
                aktuellerSlot == DienstbesetzungsSlotCode.Arzt ? null : geplanterDienstTag.ArztUserId,
                aktuellerSlot == DienstbesetzungsSlotCode.Notfallsanitaeter1 ? null : geplanterDienstTag.Notfallsanitaeter1UserId,
                aktuellerSlot == DienstbesetzungsSlotCode.Notfallsanitaeter2 ? null : geplanterDienstTag.Notfallsanitaeter2UserId
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToArray();
    }

    private static int ErmittleStandardUrlaubsanspruch(
        Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart beschaeftigungsart)
    {
        return beschaeftigungsart switch
        {
            Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt => 30,
            Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer => 3,
            Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Honorarkraft => 0,
            _ => 0
        };
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