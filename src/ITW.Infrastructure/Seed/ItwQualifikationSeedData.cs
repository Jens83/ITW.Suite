using ITW.Domain.Personnel.Qualifications;

namespace ITW.Infrastructure.Persistence.Seed;

internal static class ItwQualifikationSeedData
{
    public static readonly Guid ArztId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid NotfallsanitaeterId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid FacharztAnaesthesieId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DiviQualifizierungId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid PraxisanleiterId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid MedizinprodukteBeauftragterId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static IReadOnlyList<ItwQualifikation> GetStandardqualifikationen()
        => new[]
        {
            new ItwQualifikation(ArztId, ItwQualifikationsCodes.Arzt, "Arzt", 10),
            new ItwQualifikation(NotfallsanitaeterId, ItwQualifikationsCodes.Notfallsanitaeter, "Notfallsanitäter", 20),

            new ItwQualifikation(FacharztAnaesthesieId, ItwQualifikationsCodes.FacharztAnaesthesie, "Facharzt für Anästhesie", 100),
            new ItwQualifikation(DiviQualifizierungId, ItwQualifikationsCodes.DiviQualifizierung, "DIVI-Qualifizierung", 110),
            new ItwQualifikation(PraxisanleiterId, ItwQualifikationsCodes.Praxisanleiter, "Praxisanleiter", 120),
            new ItwQualifikation(MedizinprodukteBeauftragterId, ItwQualifikationsCodes.MedizinprodukteBeauftragter, "Medizinprodukte-Beauftragter", 130)
        };
}