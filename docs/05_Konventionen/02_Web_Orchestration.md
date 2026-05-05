# 02 – Web-Orchestrierungsregeln

> Stand: 2026-05-05 | Verantwortlich: Umbauplan v2, Phase 15

---

## Kontext

`ITW.Web` ist die Präsentationsschicht (ASP.NET Core MVC). Sie koordiniert Requests,
wandelt Ergebnisse in ViewModels um und rendert Views. Sie darf **keine Fachlogik** enthalten
und **keine persistente Infrastruktur** direkt ansprechen.

---

## Was darf die Web-Schicht

| Erlaubt | Beispiel |
|---|---|
| Application-Services aufrufen | `_readFahrzeugUebersichtService.ExecuteAsync(...)` |
| Commands/Queries an Application-Services übergeben | `new CreateFahrzeugCommand(...)` |
| ViewModels aufbauen (aus Application-Ergebnissen) | `new FahrzeugIndexItemViewModel { ... }` |
| TempData / Flash-Keys setzen | `TempData[FlashKeys.Success] = "..."` |
| IDateTimeProvider, ICurrentUserContextAccessor injizieren | Systemdienste |
| `ITW.Domain.*` Enums und Value-Objects verwenden | `FahrzeugStatus.Aktiv` |

---

## Was die Web-Schicht **nicht** darf

| Verboten | Begründung |
|---|---|
| `DbContext` direkt referenzieren | Infrastrukturdetail, verletzt Schichtentrennung |
| Konkreten `ITW.Infrastructure.*` Typ verwenden | Implementierungsdetail der Infrastruktur |
| Domain-Entitäten (`*.Entities.*`) als Action-Return oder ViewModel-Property | ViewModels sind die Schnittstelle; Entities enthalten EF-Abhängigkeiten |
| Fachlogik in Controllern oder Web-Services implementieren | Gehört in Application-Services |

---

## Web-Services (`ITW.Web.Areas.*.Services`)

Web-Services sind Hilfsklassen für komplexe ViewModel-Zusammenstellung, die zu groß
für einen Controller wären. Es gelten dieselben Regeln wie für Controller:

- ✅ Application-Services injizieren (z. B. `ReadDienstplanMonatsauswertungService`)
- ✅ Repository-**Interfaces** aus `ITW.Application.*` oder `ITW.Dienstplan.Application.*`
  injizieren, solange kein dedizierter Application-Service existiert (Übergangszustand)
- ❌ DbContext nicht verwenden
- ❌ `ITW.Infrastructure.*` nicht referenzieren
- ❌ Fachliche Berechnungen (die in die Domäne gehören) nicht selbst durchführen

> **Hinweis (Übergangszustand):** Einige Web-Services unter
> `ITW.Web.Areas.Intensivtransport.Services.Dienstplan.*` injizieren noch direkt
> Repository-Interfaces, weil kein dedizierter Application-Service existiert.
> Dies ist zu migrieren, sobald die betroffenen Dienstplan-Services refaktoriert werden.

---

## Architektur-Test (NetArchTest)

Die Regeln werden durch `ArchitekturTests.cs` in `ITW.Web.Test` automatisch geprüft:

```
✅ Web.Areas.* darf keinen DbContext referenzieren
✅ Web.Areas.* darf kein ITW.Infrastructure.* referenzieren
✅ Controller in Web.Areas.* dürfen keine ITW.Domain.*.Entities verwenden
✅ Controller in Web.Areas.* dürfen keine ITW.Dienstplan.Domain.Entities verwenden
```

Der Build schlägt fehl, wenn eine dieser Regeln verletzt wird.

---

## Beispiel-Refactoring: Repository → Application-Service

```csharp
// ❌ Vorher: Web-Service greift direkt auf Repository zu
public sealed class ReadDienstplanViewModelService(
    IGeplanterDienstTagRepository tagRepository)  // Repository-Abhängigkeit
{ ... }

// ✅ Nachher: Web-Service verwendet Application-Service
public sealed class ReadDienstplanViewModelService(
    ReadSichtbarenDienstplanService sichtbarerDienstplanService)  // Application-Service
{ ... }
```
