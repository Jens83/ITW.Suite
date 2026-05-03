namespace ITW.Application.Organisation.SetModulZuweisungStatus;

public sealed record SetModulZuweisungStatusResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static SetModulZuweisungStatusResult Erfolg()
        => new(true, null);

    public static SetModulZuweisungStatusResult Fehler(string errorMessage)
        => new(false, errorMessage);
}