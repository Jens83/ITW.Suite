using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Qualifications;

namespace ITW.Application.Personnel.Profiles;

public sealed class SaveItwMitarbeiterprofilService
{
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IItwMitarbeiterprofilRepository _itwMitarbeiterprofilRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SaveItwMitarbeiterprofilService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IItwMitarbeiterprofilRepository itwMitarbeiterprofilRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _itwMitarbeiterprofilRepository = itwMitarbeiterprofilRepository
            ?? throw new ArgumentNullException(nameof(itwMitarbeiterprofilRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<SaveItwMitarbeiterprofilResult> ExecuteAsync(
        SaveItwMitarbeiterprofilCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return SaveItwMitarbeiterprofilResult.Fehler("Die UserId ist erforderlich.");
        }

        if (command.HauptqualifikationId == Guid.Empty)
        {
            return SaveItwMitarbeiterprofilResult.Fehler("Die Hauptqualifikation ist erforderlich.");
        }

        var zusatzqualifikationIds = command.ZusatzqualifikationIds
            .Where(x => x != Guid.Empty && x != command.HauptqualifikationId)
            .Distinct()
            .ToArray();

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            command.UserId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            return SaveItwMitarbeiterprofilResult.Fehler("Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        await _itwMitarbeiterprofilRepository.EnsureStandardqualifikationenAsync(cancellationToken);

        var verfuegbareQualifikationen = await _itwMitarbeiterprofilRepository.GetAktiveQualifikationenAsync(cancellationToken);
        var qualifikationLookup = verfuegbareQualifikationen.ToDictionary(x => x.Id);

        if (!qualifikationLookup.TryGetValue(command.HauptqualifikationId, out var hauptqualifikation))
        {
            return SaveItwMitarbeiterprofilResult.Fehler("Die ausgewählte Hauptqualifikation ist nicht verfügbar.");
        }

        if (!ItwQualifikationsCodes.IstHauptqualifikationCode(hauptqualifikation.Code))
        {
            return SaveItwMitarbeiterprofilResult.Fehler("Als Hauptqualifikation sind aktuell nur Arzt oder Notfallsanitäter zulässig.");
        }

        foreach (var zusatzqualifikationId in zusatzqualifikationIds)
        {
            if (!qualifikationLookup.TryGetValue(zusatzqualifikationId, out var zusatzqualifikation))
            {
                return SaveItwMitarbeiterprofilResult.Fehler("Mindestens eine ausgewählte Zusatzqualifikation ist nicht verfügbar.");
            }

            if (!ItwQualifikationsCodes.IstZusatzqualifikationCode(zusatzqualifikation.Code))
            {
                return SaveItwMitarbeiterprofilResult.Fehler("Arzt und Notfallsanitäter dürfen nicht als Zusatzqualifikation gespeichert werden.");
            }
        }

        var jetzt = _dateTimeProvider.UtcNow;

        await _itwMitarbeiterprofilRepository.UpsertQualifikationenAsync(
            command.UserId,
            command.HauptqualifikationId,
            zusatzqualifikationIds,
            jetzt,
            cancellationToken);

        await _itwMitarbeiterprofilRepository.SaveChangesAsync(cancellationToken);

        return SaveItwMitarbeiterprofilResult.Erfolg();
    }
}