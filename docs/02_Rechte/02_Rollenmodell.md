# Rollenmodell der ITW-Suite

## Zweck dieses Dokuments

Dieses Dokument definiert das Rollenmodell der ITW-Suite auf konzeptioneller Ebene.

Ziel ist eine saubere Trennung zwischen:

- globalen Rollen,
- Bereichsrollen,
- fachlichen Merkmalen.

---

## 1. Grundprinzip

Das Rollenmodell der ITW-Suite besteht aus drei Ebenen:

1. **Globale Rollen**
2. **Bereichsrollen**
3. **Fachliche Merkmale**

Diese Ebenen dürfen nicht vermischt werden.

---

## 2. Globale Rollen

Globale Rollen besitzen systemweite oder bereichsübergreifende Bedeutung.

Sie sollen bewusst sparsam eingesetzt werden.

## Beispiele
- Technischer Administrator
- Geschäftsführung / Executive
- Revisions- oder Auditrolle, falls später erforderlich

## Eigenschaften globaler Rollen
- systemweit relevant
- selten vergeben
- nicht für fachliche Detailsteuerung gedacht
- kein Ersatz für Bereichsrollen

---

## 3. Bereichsrollen

Bereichsrollen gelten innerhalb eines organisatorischen Bereichs.

Sie steuern Sichtbarkeit, Verwaltung und Zuständigkeit innerhalb des Bereichs.

## Beispiele im Bereich Intensivtransport
- Wachleiter
- ITW-Mitarbeiter

## Beispiele im Bereich Verwaltung
- Geschäftsführer Verwaltung
- Verwaltungsmitarbeiter

## Beispiele im Bereich Geschäftsführung
- Führungsebene
- freigegebene übergeordnete Verwaltung

## Eigenschaften von Bereichsrollen
- immer an einen Bereich gebunden
- regeln Sichtbarkeit innerhalb des Bereichs
- regeln Bearbeitungsrechte innerhalb des Bereichs
- ersetzen keine globalen technischen Rollen

---

## 4. Fachliche Merkmale

Fachliche Merkmale sind keine Berechtigungen.

Sie beschreiben fachliche Eigenschaften eines Mitarbeiters.

## Beispiele
- Arzt
- Notfallsanitäter
- spätere Zusatzqualifikationen

## Wichtige Regel
Ein fachliches Merkmal bedeutet **nicht automatisch**, dass der Benutzer Verwaltungsrechte besitzt.

Beispiel:
- Ein Arzt ist fachlich qualifiziert, aber nicht automatisch ein Bereichsleiter.
- Ein Wachleiter ist ein Bereichsleiter, aber seine Rolle ist keine fachliche Qualifikation.

---

## 5. Empfohlenes Rollenmodell für die erste Ausbaustufe

## Globale Rollen
- Technischer Administrator
- Geschäftsführung / Executive

## Bereichsrollen Intensivtransport
- Wachleiter
- ITW-Mitarbeiter

## Bereichsrollen Verwaltung
- Geschäftsführer Verwaltung
- Verwaltungsmitarbeiter

## Bereichsrollen Geschäftsführung
- Führungsebene

## Fachliche Merkmale Intensivtransport
- Arzt
- Notfallsanitäter

---

## 6. Rechtevergabe

Rechte werden nicht ausschließlich über starre Rollen modelliert.

Stattdessen gilt:

- Rollen gruppieren typische Zuständigkeiten,
- konkrete Aktionen werden über Berechtigungen / Policies gesteuert,
- Sichtbarkeit wird über Bereich und Scope ergänzt.

Dadurch bleibt das Modell langfristig flexibler.

---

## 7. Trennung von Rollen und Berechtigungen

### Rollen
beschreiben die typische Stellung eines Benutzers.

### Berechtigungen / Policies
beschreiben konkrete erlaubte Aktionen, z. B.:
- Benutzer lesen im eigenen Bereich
- Benutzer bearbeiten im eigenen Bereich
- Periode anlegen
- Wunschphase öffnen
- Dienstplan veröffentlichen

### Scopes
beschreiben, auf **welche Datenmenge** sich die Berechtigung beziehen darf, z. B.:
- nur Bereich Intensivtransport
- nur Bereich Verwaltung
- nur eigene Daten
- nur freigegebene Führungsebene

---

## 8. Beispielhafte Trennung

### Beispiel 1
Benutzer:
- Bereich: Intensivtransport
- Bereichsrolle: Wachleiter
- fachliches Merkmal: Notfallsanitäter

Bedeutung:
- darf ITW-Benutzer im eigenen Bereich verwalten,
- darf Planungsperioden steuern,
- ist fachlich Notfallsanitäter.

### Beispiel 2
Benutzer:
- Bereich: Intensivtransport
- Bereichsrolle: ITW-Mitarbeiter
- fachliches Merkmal: Arzt

Bedeutung:
- darf keine Benutzer verwalten,
- darf eigene Wünsche abgeben,
- ist fachlich Arzt.

---

## 9. Verbotene Modellierungsfehler

Folgende Fehler sind zu vermeiden:

- Arzt oder Notfallsanitäter als Verwaltungsrolle modellieren
- Bereichsleiter nur als globalen Admin modellieren
- fachliche Merkmale in globale Rollen überführen
- alle Rechte über wenige harte Rollen erschlagen
- Sichtbarkeit nur aus Menüs ableiten

---

## 10. Geschäftsführung und technische Administration

Die Geschäftsführung ist eine fachliche und organisatorische Rolle.  
Sie ist nicht automatisch technischer Administrator.

Die technische Administration ist eine eigene technische Verantwortung und bleibt getrennt.

---

## 11. Entwicklungsperspektive

Das Rollenmodell muss so aufgebaut werden, dass spätere Erweiterungen möglich sind, ohne alle bestehenden Rollen neu schneiden zu müssen.

Das bedeutet:

- globale Rollen bleiben sparsam,
- Bereichsrollen bleiben bereichsbezogen,
- fachliche Merkmale bleiben getrennt,
- neue Berechtigungen werden über Policies ergänzt.

---

## 12. Zusammenfassung

Das Rollenmodell der ITW-Suite trennt bewusst zwischen:

- globaler Stellung,
- bereichsbezogener Funktion,
- fachlicher Qualifikation.

Diese Trennung ist zentral für eine saubere Rechte- und Sichtbarkeitslogik.