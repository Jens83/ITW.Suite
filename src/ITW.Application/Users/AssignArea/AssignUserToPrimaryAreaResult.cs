namespace ITW.Application.Users.AssignArea;

public sealed record AssignUserToPrimaryAreaResult(
    bool IsSuccess,
    string? ErrorMessage,
    Guid? BereichszuordnungId)
{
    public static AssignUserToPrimaryAreaResult Erfolg(Guid bereichszuordnungId) =>
        new(true, null, bereichszuordnungId);

    public static AssignUserToPrimaryAreaResult Fehler(string errorMessage) =>
        new(false, errorMessage, null);
}