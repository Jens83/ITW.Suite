using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Shared;

internal static class DienstplanPlanungsHelper
{
    public static bool IstArzt(ItwMitarbeiterprofilUebersichtDto profil)
        => string.Equals(profil.Hauptqualifikation, "Arzt", StringComparison.OrdinalIgnoreCase);

    public static bool IstNotfallsanitaeter(ItwMitarbeiterprofilUebersichtDto profil)
        => string.Equals(profil.Hauptqualifikation, "Notfallsanitäter", StringComparison.OrdinalIgnoreCase)
           || string.Equals(profil.Hauptqualifikation, "Notfallsanitaeter", StringComparison.OrdinalIgnoreCase);

    public static (string AnzeigeName, string Hauptqualifikation) ErmittleGeplantenMitarbeiter(
        IReadOnlyDictionary<string, ItwMitarbeiterprofilUebersichtDto> profilLookup,
        string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ("Noch nicht gesetzt", string.Empty);
        }

        if (profilLookup.TryGetValue(userId, out var profil))
        {
            return (profil.AnzeigeName, profil.Hauptqualifikation);
        }

        return (userId, "Unbekannt");
    }

    public static string? ErmittleGeplantenUserIdFuerSlot(
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

    public static IReadOnlyList<string> ErmittleAndereGeplanteUserIds(
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

    public static async Task<HashSet<string>> ErmittleFestangestellteImUrlaubUserIdsAsync(
        IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> aktiveProfile,
        DateOnly datum,
        IMitarbeiterUrlaubszeitraumRepository urlaubszeitraumRepository,
        CancellationToken cancellationToken)
    {
        var userIdsMitHinterlegtemUrlaub = await urlaubszeitraumRepository.GetAktiveUserIdsFuerDatumAsync(
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
}