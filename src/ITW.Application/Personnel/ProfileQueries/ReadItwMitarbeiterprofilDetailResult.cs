namespace ITW.Application.Personnel.ProfileQueries;

public sealed record ReadItwMitarbeiterprofilDetailResult(
    bool IsSuccess,
    string? ErrorMessage,
    ItwMitarbeiterprofilDetailDto? Profil,
    IReadOnlyList<ItwQualifikationOptionDto> VerfuegbareHauptqualifikationen,
    IReadOnlyList<ItwQualifikationOptionDto> VerfuegbareZusatzqualifikationen)
{
    public static ReadItwMitarbeiterprofilDetailResult Erfolg(
        ItwMitarbeiterprofilDetailDto profil,
        IReadOnlyList<ItwQualifikationOptionDto> verfuegbareHauptqualifikationen,
        IReadOnlyList<ItwQualifikationOptionDto> verfuegbareZusatzqualifikationen)
        => new(true, null, profil, verfuegbareHauptqualifikationen, verfuegbareZusatzqualifikationen);

    public static ReadItwMitarbeiterprofilDetailResult Fehler(string errorMessage)
        => new(
            false,
            errorMessage,
            null,
            Array.Empty<ItwQualifikationOptionDto>(),
            Array.Empty<ItwQualifikationOptionDto>());
}