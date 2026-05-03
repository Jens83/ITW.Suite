namespace ITW.Application.Personnel.ProfileQueries;

public sealed record ReadItwMitarbeiterDetailUebersichtResult(
    bool IsSuccess,
    string? ErrorMessage,
    ItwMitarbeiterDetailUebersichtDto? Mitarbeiter)
{
    public static ReadItwMitarbeiterDetailUebersichtResult Erfolg(ItwMitarbeiterDetailUebersichtDto mitarbeiter)
        => new(true, null, mitarbeiter);

    public static ReadItwMitarbeiterDetailUebersichtResult Fehler(string errorMessage)
        => new(false, errorMessage, null);
}