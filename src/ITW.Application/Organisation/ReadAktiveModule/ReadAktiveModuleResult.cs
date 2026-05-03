using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Organisation.ReadAktiveModule;

public sealed record ReadAktiveModuleResult(
    bool IsSuccess,
    IReadOnlyList<ModulCode> Module,
    string? ErrorMessage)
{
    public static ReadAktiveModuleResult Erfolg(IReadOnlyList<ModulCode> module)
        => new(true, module, null);

    public static ReadAktiveModuleResult Fehler(string errorMessage)
        => new(false, Array.Empty<ModulCode>(), errorMessage);
}