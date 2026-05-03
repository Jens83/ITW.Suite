namespace ITW.Application.Users.CreateUser;

public sealed record CreateBenutzerkontoCommand(
    string Benutzername,
    string Email,
    string Passwort);