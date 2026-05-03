# Zuständigkeitsmatrix der ITW-Suite

## Zweck des Dokuments

Dieses Dokument beschreibt, welcher Bereich und welche Rolle für welche Aufgaben zuständig ist.

Die Matrix dient dazu:

- organisatorische Zuständigkeiten sauber zu trennen
- spätere Berechtigungen abzuleiten
- Sichtbarkeiten nachvollziehbar zu machen
- Modulsteuerung und operative Nutzung auseinanderzuhalten

---

## 1. Grundregel

In der ITW-Suite gilt:

- organisatorische Zuständigkeit
- fachliche Modulverfügbarkeit
- technische Administration

sind drei unterschiedliche Dinge.

Daraus folgt:

- Ein Benutzer kann organisatorisch zu einem Bereich gehören, ohne jedes Modul dieses Bereichs zu sehen.
- Ein Modul kann für eine Rolle freigegeben sein, ohne dass dadurch andere Fachzuständigkeiten entstehen.
- Die Geschäftsführung steuert zentral die Modulzuweisung, nutzt aber nicht automatisch alle operativen Oberflächen.

---

## 2. Begriffe und technische Benennung

Fachlich sprechen die Docs von:

- Intensivtransport
- Verwaltung
- Geschäftsführung

Im aktuellen technischen Stand gilt zusätzlich:

- Die Geschäftsführung entspricht technisch dem Bereichscode `Vorstand`.
- Die Rolle `Vorstandsverwaltung` ist die verwaltungsnahe Führungsrolle im Bereich Verwaltung.

Diese technische Benennung ist bei der Umsetzung zu beachten.

---

## 3. Bereiche

Die ITW-Suite kennt organisatorisch folgende Bereiche:

- Intensivtransport
- Verwaltung
- Geschäftsführung

Jeder Bereich besitzt eigene Zuständigkeiten, eigene Navigation und eigene Rollen.

---

## 4. Typische Rollen

### Intensivtransport

- Mitarbeiter
- Wachleiter

### Verwaltung

- Verwaltungsmitarbeiter
- Vorstandsverwaltung

### Geschäftsführung

- Vorstand

---

## 5. Grundsatz je Bereich

### Intensivtransport
Zuständig für operative und planungsnahe Themen des Intensivtransports.

### Verwaltung
Zuständig für kaufmännische, organisatorische und verwaltungsnahe Themen.

### Geschäftsführung
Zuständig für zentrale Steuerung, Freigaben und Modulzuweisungen.

---

## 6. Anwendungsfall-Matrix

Legende:

- **Ja** = organisatorisch zuständig
- **Nein** = nicht organisatorisch zuständig

| Anwendungsfall                         | Mitarbeiter | Wachleiter | Verwaltungsmitarbeiter | Vorstandsverwaltung | Vorstand | Technische Administration |
|----------------------------------------|-------------|------------|------------------------|---------------------|----------|---------------------------|
| ITW-Benutzer lesen                     | Nein        | Ja         | Nein                   | Nein                | Nein     | Nein                      |
| ITW-Benutzer bearbeiten                | Nein        | Ja         | Nein                   | Nein                | Nein     | Nein                      |
| Verwaltungsbenutzer lesen              | Nein        | Nein       | Nein                   | Ja                  | Nein     | Nein                      |
| Verwaltungsbenutzer bearbeiten         | Nein        | Nein       | Nein                   | Ja                  | Nein     | Nein                      |
| Geschäftsführungsbenutzer lesen        | Nein        | Nein       | Nein                   | Nein                | Ja       | Nein                      |
| Geschäftsführungsbenutzer bearbeiten   | Nein        | Nein       | Nein                   | Nein                | Ja       | Nein                      |
| Bereichsrollen vergeben                | Nein        | Ja         | Nein                   | Ja                  | Ja       | Nein                      |
| Module zentral zuweisen                | Nein        | Nein       | Nein                   | Nein                | Ja       | Nein                      |
| Dienstwünsche abgeben                  | Ja          | Nein       | Nein                   | Nein                | Nein     | Nein                      |
| ITW-Dienstplan führen                  | Nein        | Ja         | Nein                   | Nein                | Nein     | Nein                      |
| Autoplan ausführen                     | Nein        | Ja         | Nein                   | Nein                | Nein     | Nein                      |
| Veröffentlichten ITW-Plan sehen        | Ja          | Ja         | Nein                   | Nein                | Nein     | Nein                      |
| Technische Plattform verwalten         | Nein        | Nein       | Nein                   | Nein                | Nein     | Ja                        |

Wichtig:

- Diese Matrix beschreibt organisatorische Zuständigkeit.
- Ob eine Funktion tatsächlich nutzbar ist, hängt zusätzlich von der Modulfreigabe ab.

---

## 7. Dienstplan – fachliche Zuständigkeiten

### ITW-Mitarbeiter
Ist zuständig für:

- eigene Dienstwünsche
- eigene Wunschabgabe
- Sicht auf den veröffentlichten Plan

Ist nicht zuständig für:

- Periodenverwaltung
- Planung anderer Mitarbeiter
- Autoplan
- Freigabe und Veröffentlichung

### Wachleiter
Ist zuständig für:

- Dienstplanführung im Bereich Intensivtransport
- Periodensteuerung
- Planungskontext der Wünsche
- Freigabe und Veröffentlichung
- Autoplan
- bereichsbezogene Mitarbeiterverwaltung im ITW-Bereich, sofern `Personal` freigegeben ist

Ist nicht zuständig für:

- Verwaltungsmodule der Verwaltung
- zentrale Modulzuweisung
- bereichsübergreifende operative Benutzerverwaltung

---

## 8. Personal – fachliche Zuständigkeiten

### Wachleiter im Intensivtransport
Kann zuständig sein für:

- ITW-Benutzerlisten
- ITW-bezogene Rollenänderungen
- ITW-bezogene Personalfunktionen

Wichtig:

- Diese Zuständigkeit gilt nur im Bereich Intensivtransport.
- Sie setzt fachlich die passende Freigabe des Moduls `Personal` voraus.

### Vorstandsverwaltung in der Verwaltung
Kann zuständig sein für:

- Verwaltungsbenutzer
- verwaltungsbezogene Rollenänderungen
- verwaltungsbezogene Personalfunktionen

Wichtig:

- Diese Zuständigkeit gilt nur im Bereich Verwaltung.
- Sie setzt fachlich die passende Freigabe des Moduls `Personal` voraus.

### Vorstand in der Geschäftsführung
Kann zuständig sein für:

- Benutzerlisten im Bereich Geschäftsführung
- Rollenänderungen im Bereich Geschäftsführung
- Personalfunktionen im Bereich Geschäftsführung

Wichtig:

- Diese Zuständigkeit ist im aktuellen Stand auf den Bereich Geschäftsführung begrenzt.
- Daraus entsteht keine automatische operative Vollsicht auf Benutzer anderer Bereiche.

---

## 9. Modulsteuerung in der Geschäftsführung

Die Geschäftsführung ist zuständig für:

- zentrale Steuerung der Modulverfügbarkeit
- Zuweisung von Modulen an Bereich und Rolle
- Freigaben auf Suite-Ebene
- übergreifende Steuerungssichten

Wichtig:

- Die Modulsteuerung ist eine zentrale Führungsfunktion.
- Sie ersetzt keine operative Personalzuständigkeit in anderen Bereichen.

Beispiel:

- Die Geschäftsführung kann `Dienstplan` für `Intensivtransport / Mitarbeiter` freischalten.
- Die Geschäftsführung wird dadurch nicht automatisch operativer ITW-Planer.

---

## 10. Zuständigkeit in der Geschäftsführung

Die Geschäftsführung ist zuständig für:

- Führungsebene im Bereich Geschäftsführung
- zentrale Steuerungsfunktionen
- Freigaben
- zentrale Modulzuweisungen
- Auswertungen auf freigegebenen Oberflächen

Nicht automatisch enthalten sind:

- operative Vollsicht in alle Benutzer anderer Bereiche
- fachliche Detailzuständigkeit für Dienstplan oder Verwaltung
- technische Vollzugriffe

---

## 11. Technische Administration

Technische Administration ist kein fachlicher Bereich und keine Fachrolle.

Technische Administration ist zuständig für:

- Hosting
- Deployment
- technische Infrastruktur
- Backups
- technische Betriebsaufgaben

Technische Administration ist nicht automatisch zuständig für:

- Fachentscheidungen
- Modulfreigaben
- Dienstplanung
- operative Personalarbeit

---

## 12. Ableitung für die Umsetzung

Für die technische Umsetzung bedeutet diese Matrix:

- Bereich und Rolle definieren den organisatorischen Rahmen.
- Modulzuweisungen definieren die fachliche Nutzbarkeit.
- Navigation darf Funktionen nur anzeigen, wenn sie organisatorisch und fachlich passen.
- Controllerzugriffe müssen serverseitig denselben Rahmen prüfen.
- Bereichsbezogene Benutzerlisten bleiben auf den jeweiligen Bereich begrenzt.

---

## 13. Zusammenfassung

Die ITW-Suite trennt bewusst:

- organisatorische Zuständigkeit
- fachliche Modulverfügbarkeit
- technische Administration

Die Geschäftsführung verwaltet die Modulzuweisungen zentral.  
Die eigentliche operative Nutzung bleibt in den zuständigen Bereichen und Rollen.

Für den Dienstplan bleibt besonders wichtig:

- Mitarbeiterfluss bleibt Mitarbeiterfluss
- Wachleiterfluss bleibt Wachleiterfluss
- `Dienstplan` und `Personal` bleiben getrennte Module