using ITW.Application.Abstractions.Identity;

namespace ITW.Application.Users.ReadNichtZugeordneteBenutzerkonten;

public sealed record ReadNichtZugeordneteBenutzerkontenResult(
    bool IsSuccess,
    IReadOnlyList<BenutzerkontoDto> Benutzerkonten,
    string? ErrorMessage)
{
    public static ReadNichtZugeordneteBenutzerkontenResult Erfolg(
        IReadOnlyList<BenutzerkontoDto> benutzerkonten)
        => new(true, benutzerkonten, null);

    public static ReadNichtZugeordneteBenutzerkontenResult Fehler(string errorMessage)
        => new(false, Array.Empty<BenutzerkontoDto>(), errorMessage);
}