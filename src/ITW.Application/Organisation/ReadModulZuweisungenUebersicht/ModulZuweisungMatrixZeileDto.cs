using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Organisation.ReadModulZuweisungenUebersicht;

public sealed record ModulZuweisungMatrixZeileDto(
    ModulCode Modul,
    string Anzeigename,
    IReadOnlyList<ModulZuweisungMatrixZelleDto> Zellen);