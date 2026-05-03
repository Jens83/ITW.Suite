# Projektgrenzen der ITW-Suite

## Zweck dieses Dokuments

Dieses Dokument beschreibt die fachlichen und technischen Grenzen der einzelnen Projekte innerhalb der ITW-Suite.

Ziel ist, sauber festzulegen:

- welches Projekt wofür zuständig ist,
- welche Verantwortung dort liegt,
- was dort ausdrücklich **nicht** hinein gehört,
- welche Abhängigkeiten erlaubt sind,
- wie Web, Plattform und Fachmodule praktisch zusammenspielen.

---

## 1. Gesamtübersicht

Die Lösung besteht aus folgenden Projekten:

- `ITW.Web`
- `ITW.Application`
- `ITW.Domain`
- `ITW.Infrastructure`
- `ITW.Dienstplan`
- `ITW.Einsatz`
- `ITW.Fahrzeugmanagement`

Diese Aufteilung bleibt verbindlich.  
Neue Projekte oder zusätzliche Layer werden nicht leichtfertig eingeführt.

---

## 2. ITW.Web

## Aufgabe

`ITW.Web` ist der Web-Host und die Oberfläche der Anwendung.

## Zuständig für

- ASP.NET Core MVC Host
- Areas
- Controller
- Views
- ViewModels
- Layouts
- Navigation
- menüabhängige Sichtbarkeit
- UI-Komponenten
- zentrales UI-Designsystem
- gemeinsame Shell- und Resource-Partials
- Theme-, Shell-, Komponenten- und Modulstruktur im Frontend
- web-spezifische Integration
- Anzeige von Benachrichtigungen
- web-spezifische Fehlerbehandlung
- Web-spezifische Dependency-Injection-Struktur
- Web-spezifische Startup-/Bootstrap-Verdrahtung
- browserbasierte Tablet-Tracking-Seite
- Wachleiter-Oberflächen für Tracking-Geräte und Live-Standort
- zentrale JavaScript-Initialisierung für Karten- und Tracking-UI
- zentrale CSS-Nutzung über vorhandene App-/Modulklassen

## Gehört ausdrücklich hierhin

- alles, was Darstellung und Bedienung betrifft
- area-spezifische Oberflächen
- Dashboard-Seiten
- Formular- und Listenansichten
- bereichsspezifische Navigation
- gemeinsame App-Shell für die Areas
- zentrale Head-/Script-Ressourcen im Web-Projekt
- Pflege der zentralen CSS-/UI-Dateien
- schlanke Controller
- Web-Orchestrierungsservices, die:
  - Fachservices aus `ITW.Application`, `ITW.Dienstplan`, `ITW.Einsatz` oder `ITW.Fahrzeugmanagement` konsumieren,
  - ViewModels zusammensetzen,
  - Redirect-/Modal-/Ansichtslogik koordinieren,
  - aber keine fachliche Kernlogik besitzen

## Gehört ausdrücklich nicht hierhin

- fachliche Kernlogik von Dienstplan
- fachliche Kernlogik von Einsatz
- fachliche Kernlogik von Fahrzeugmanagement
- dauerhafte Besitzverantwortung für Benutzer- und Organisationsdaten
- Datenbanklogik
- technische Infrastrukturimplementierungen
- fachliche Regeln, die im Modul `ITW.Dienstplan`, `ITW.Einsatz` oder `ITW.Fahrzeugmanagement` zuhause sind
- GPS-Speicherlogik
- API-Key-Prüfung für Tracking-Geräte
- Berechnung von RouteSessions
- fachliche Entscheidung, wann Historienpunkte geschrieben werden

## Wichtige aktuelle Festlegung

`ITW.Web` darf zur Entlastung großer Controller eigene Web-Orchestrierungsservices enthalten.

Diese sind zulässig, wenn sie:

- nur Web-spezifische Koordination übernehmen,
- bestehende Fachservices zusammensetzen,
- ViewModels oder Redirect-Flows vorbereiten,
- keine fachliche Kernverantwortung aus den Fachmodulen übernehmen,
- keine Datenbankverantwortung erhalten.

Beispiele aus dem aktuellen Stand:

- `ReadDienstplanIndexViewModelService`
- `ReadSichtbarerDienstplanViewModelService`
- `ReadMonatsauswertungViewModelService`
- `ReadWachleiterPlanungsModalService`
- `SaveWachleiterTagesplanungService`
- `SaveWachleiterAusfallService`
- `ToggleMitarbeiterWunschService`
- `SaveFreelancerMonatswunschMitarbeiterService`

## Frontend-Regel

Für das Web-Projekt gilt:

- Views enthalten Struktur und Daten.
- Views enthalten keine eigenen Inline-Styles.
- Views enthalten keine eigenen Inline-Scripts.
- Gemeinsame Styles liegen zentral im Web-Projekt.
- Gemeinsame Scripts liegen zentral im Web-Projekt.
- Area-Layouts laden zentrale Resource-Partials.
- Modulbezogene Oberflächen verwenden die vorhandenen App-Klassen, Komponenten und Layoutmuster.

## Merksatz

`ITW.Web` zeigt Fachlogik an, koordiniert Web-Flows und besitzt das komplette UI-Rahmensystem, aber nicht die fachliche Kernlogik selbst.

---

## 3. ITW.Application

## Aufgabe

`ITW.Application` enthält zentrale Anwendungslogik und zentrale Anwendungsfälle für plattformweite Themen.

## Zuständig für

- Benutzeranwendungsfälle
- Bereichszuordnung
- Bereichsrollen
- Führungsverantwortung
- serverseitige Sichtbarkeit und Scopes
- plattformweite Policies
- Audit-Anwendungsfälle
- Benachrichtigungsanbindung
- gemeinsame Anwendungsabstraktionen

## Gehört ausdrücklich hierhin

- Anwendungsfälle der zentralen Benutzer-/Organisationslogik
- zentrale Scope-Prüfungen
- gemeinsame Verträge und Abstraktionen
- zentrale Policy-nahe Logik
- Ergebnis- und Validierungsstrukturen, soweit wirklich zentral

## Gehört ausdrücklich nicht hierhin

- Web-spezifische Controller- oder ViewLogik
- Dienstplan-spezifische Vollverantwortung
- Einsatz-spezifische Vollverantwortung
- Fahrzeugmanagement-spezifische Vollverantwortung
- EF-Core-Konfigurationen
- Identity-Implementierungsdetails

## Merksatz

`ITW.Application` koordiniert zentrale Anwendungsfälle, ist aber nicht der Sammelplatz für jede Fachlogik.

---

## 4. ITW.Domain

## Aufgabe

`ITW.Domain` enthält zentrale fachliche Kernbegriffe, die projektübergreifend relevant sind.

## Zuständig für

- Organisationsbereich
- Bereichszuordnung
- Bereichsrolle
- Führungsverantwortung
- zentrale Mitarbeiterprofil-Grundbegriffe
- zentrale Qualifikationsgrundbegriffe
- gemeinsame Value Objects und Regeln

## Gehört ausdrücklich hierhin

- wirklich gemeinsame Kernkonzepte
- fachliche Grundbegriffe, die nicht an Web oder Infrastruktur gebunden sind
- zentrale Value Objects
- zentrale Regeln, wenn sie nicht modulspezifisch sind

## Gehört ausdrücklich nicht hierhin

- Dienstplanperioden
- Dienstwünsche
- geplante Dienste
- Einsatzdaten
- Fahrzeugmanagement-Entities
- Tracking-Geräte
- MVC-spezifische Modelle
- Infrastrukturthemen

## Merksatz

`ITW.Domain` ist klein und sauber. Es enthält nur echte Kernbegriffe, nicht pauschal alles Fachliche.

---

## 5. ITW.Dienstplan

## Aufgabe

`ITW.Dienstplan` enthält die fachliche Kernlogik des Dienstplans.

## Zuständig für

- Dienstplanperioden
- Wunschphasen
- Dienstwünsche
- gewünschte monatliche Dienstanzahl
- automatische Planung
- manuelle Planung
- Veröffentlichungen
- Auswertungslogik, soweit fachlich dienstplanbezogen
- Dienstplanregeln
- Autoplan-Fachlogik
- Lernereignisse im Dienstplankontext

## Gehört ausdrücklich hierhin

- Dienstplan-Entities
- Dienstplan-Services
- Dienstplan-Regeln
- Dienstplan-Repository-Contracts
- Tests der Dienstplanfachlogik

## Gehört ausdrücklich nicht hierhin

- MVC-Controller
- Razor-Views
- Web-ViewModels
- Navigation
- CSS
- JavaScript
- Identity-Implementierung
- Infrastrukturimplementierung

## Merksatz

`ITW.Dienstplan` besitzt die Dienstplanfachlogik, aber keine Web-Oberfläche.

---

## 6. ITW.Einsatz

## Aufgabe

`ITW.Einsatz` enthält die fachliche Kernlogik der Einsatzverwaltung.

## Zuständig für

- Einsätze
- Einsatzstatus
- Einsatzdaten
- Besetzungen im Einsatzkontext
- spätere Einsatzdokumentation
- fachliche Einsatzprozesse

## Gehört ausdrücklich hierhin

- Einsatz-Entities
- Einsatz-Services
- Einsatz-Regeln
- Einsatz-Repository-Contracts
- Tests der Einsatzfachlogik

## Gehört ausdrücklich nicht hierhin

- MVC-Controller
- Razor-Views
- Web-ViewModels
- Navigation
- CSS
- JavaScript
- Fahrzeugmanagement-Trackinglogik
- Tablet-GPS-Speicherlogik

## Merksatz

`ITW.Einsatz` besitzt die Einsatzfachlogik, liest später ggf. Liveinformationen mit, besitzt aber nicht die GPS-Tracking-Kernlogik.

---

## 7. ITW.Fahrzeugmanagement

## Aufgabe

`ITW.Fahrzeugmanagement` enthält die fachliche Kernlogik des Fahrzeugmanagements.

## Zuständig für

- Fahrzeugstammdaten
- digitale Fahrzeugakte
- Fahrzeugdokumente
- Fahrzeugverträge
- Fahrerzuordnungen
- Wartungs- und Terminlogik
- Kostenmanagement
- Compliance-Themen
- Fahrtenbuch
- Registrierung von Tracking-Geräten
- Verarbeitung von Tablet-GPS-Daten
- aktueller Tablet-Standort
- Tablet-Standort-Historie
- RouteSession-Logik
- Entscheidung, ob ein Historienpunkt geschrieben wird

## Gehört ausdrücklich hierhin

- Entity-Modelle des Fahrzeugmanagements
- fachliche Services für Tracking-Geräte
- `RegisterTrackingGeraetService`
- `SaveLocationUpdateService`
- `ReadTabletLiveStandortOverviewService`
- Repository-Contracts des Moduls
- Tests der fachlichen Trackinglogik

## Gehört ausdrücklich nicht hierhin

- MVC-Controller
- Razor-Views
- Web-ViewModels
- Navigation
- CSS
- JavaScript
- Layouts
- Web-spezifische Redirect- oder TempData-Logik
- EF-Core-Implementierung
- SQL-Bootstrapper

## Wichtige fachliche Festlegung

Das GPS-Tracking ist tabletbasiert.

Es gilt:

- Das mobile Tablet ist die GPS-Quelle.
- Standortdaten werden dem Tracking-Gerät zugeordnet.
- Das Fahrzeug ist für das Live-Tracking nicht führend.
- Eine aktive Tablet-Fahrzeug-Zuordnung ist für das GPS-Tracking nicht erforderlich.
- Der Wachleiter sieht die Tablet-Position als operativen Standort des aktuell eingesetzten ITW.

## Merksatz

`ITW.Fahrzeugmanagement` besitzt die fachliche Trackinglogik, aber keine Web-Oberfläche.

---

## 8. ITW.Infrastructure

## Aufgabe

`ITW.Infrastructure` enthält technische Implementierungen und Datenzugriff.

## Zuständig für

- EF-Core DbContexts
- EF-Core Konfigurationen
- Repository-Implementierungen
- SQL-Bootstrapper
- technische Integrationen
- Dateispeicher
- externe Schnittstellen, soweit technisch
- technische Persistenzdetails

## Gehört ausdrücklich hierhin

- `PlatformDbContext`
- EF-Konfigurationen
- Repository-Implementierungen
- Datenbank-Bootstrapper
- technische Implementierung der Persistence
- technische Adapter

## Gehört ausdrücklich nicht hierhin

- MVC-Controller
- Razor-Views
- Web-ViewModels
- Navigation
- CSS
- JavaScript
- Web-Layouts
- fachliche UI-Koordination
- Abhängigkeit auf `ITW.Web`

## Wichtige Regel

`ITW.Infrastructure` darf `ITW.Web` nicht kennen.

## Merksatz

`ITW.Infrastructure` speichert und lädt Daten, besitzt aber keine Web- oder UI-Verantwortung.

---

## 9. Referenzrichtung

Die Projekte dürfen nur in der vorgesehenen Richtung voneinander abhängen.

Grundregel:

- Web kennt Fachmodule und Application.
- Fachmodule kennen ihre eigenen Contracts und Entities.
- Infrastructure implementiert technische Details.
- Infrastructure darf Web nicht kennen.
- Fachmodule dürfen Web nicht kennen.
- Web darf Fachservices konsumieren, aber keine Fachlogik ersetzen.

---

## 10. Aktuelle Sonderfestlegung: Web-Orchestrierungsservices

Im aktuellen Stand existieren im Web-Projekt gezielt Web-Orchestrierungsservices.

Diese sind erlaubt, wenn sie:

- Controller entlasten,
- ViewModels zusammensetzen,
- Web-Flows koordinieren,
- TempData, Redirects oder Modale vorbereiten,
- Fachservices konsumieren,
- keine Fachlogik besitzen.

Sie sind nicht als neuer allgemeiner Layer zu verstehen, sondern als konkrete Web-Koordinationslogik innerhalb von `ITW.Web`.

---

## 11. Zusammenfassung

Die ITW-Suite bleibt ein modularer Monolith mit klaren Projektgrenzen.

Wichtigste Regeln:

- Fachlogik bleibt in den Fachmodulen.
- Web zeigt und koordiniert.
- Infrastructure speichert und implementiert technische Details.
- Domain bleibt klein und zentral.
- Keine Schichtvermischung.
- Keine unnötigen neuen Projekte.
- Keine Mandantenlogik für Modulsichtbarkeit.
- Tablet-Tracking gehört fachlich zum Fahrzeugmanagement.
- Browserbasierte Tablet-Seiten gehören ins Web.