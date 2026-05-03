namespace ITW.Application.Abstractions.Identity;

public sealed record BenutzerkontoDto(
    string UserId,
    string Benutzername,
    string Email,
    bool IstGesperrt);