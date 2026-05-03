namespace ITW.Application.Organisation.ReadModulZuweisungenUebersicht;

public sealed record ReadModulZuweisungenUebersichtResult(
    bool IsSuccess,
    IReadOnlyList<ModulEmpfaengerDefinitionDto> Empfaenger,
    IReadOnlyList<ModulZuweisungMatrixZeileDto> Module,
    string? ErrorMessage)
{
    public static ReadModulZuweisungenUebersichtResult Erfolg(
        IReadOnlyList<ModulEmpfaengerDefinitionDto> empfaenger,
        IReadOnlyList<ModulZuweisungMatrixZeileDto> module)
        => new(true, empfaenger, module, null);

    public static ReadModulZuweisungenUebersichtResult Fehler(string errorMessage)
        => new(false, Array.Empty<ModulEmpfaengerDefinitionDto>(), Array.Empty<ModulZuweisungMatrixZeileDto>(), errorMessage);
}