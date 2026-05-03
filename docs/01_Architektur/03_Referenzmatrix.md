# Referenzmatrix der ITW-Suite

## Zweck dieses Dokuments

Dieses Dokument legt verbindlich fest, welche Projektabhängigkeiten in der ITW-Suite erlaubt sind und welche ausdrücklich verboten sind.

Ziel ist eine stabile, nachvollziehbare und wartbare Abhängigkeitsrichtung.

Wichtig:

- Die Referenzmatrix beschreibt den **aktuellen technischen Stand** der Solution.
- Sie ersetzt keine fachliche Ownership.
- Eine erlaubte Referenz bedeutet nicht automatisch, dass dort auch die fachliche Verantwortung liegt.

---

## 1. Berücksichtigte Projekte

Die Matrix bezieht sich auf die produktiven Hauptprojekte:

- `ITW.Web`
- `ITW.Application`
- `ITW.Domain`
- `ITW.Infrastructure`
- `ITW.Dienstplan`
- `ITW.Einsatz`

Testprojekte sind hiervon ausgenommen.

---

## 2. Grundregeln

Für Referenzen gelten folgende Regeln:

### 2.1 Nur so viele Referenzen wie nötig
Jedes Projekt soll nur die Abhängigkeiten besitzen, die es wirklich braucht.

### 2.2 Keine Rückwärtskopplung in Web-Richtung
Fach- und Infrastrukturprojekte dürfen `ITW.Web` nicht kennen.

### 2.3 Domain bleibt am stabilsten
`ITW.Domain` ist der gemeinsame fachliche Kern und kennt keine anderen Produktivprojekte.

### 2.4 Fachliche Ownership bleibt trotz technischer Referenz erhalten
Auch wenn `ITW.Infrastructure` aktuell `ITW.Dienstplan` referenziert, bleibt die fachliche Ownership des Dienstplans im Projekt `ITW.Dienstplan`.

---

## 3. Erlaubte Referenzen je Projekt

## ITW.Web
`ITW.Web` darf referenzieren:

- `ITW.Application`
- `ITW.Dienstplan`
- `ITW.Einsatz`
- `ITW.Infrastructure`

### Begründung
`ITW.Web` hostet die Oberfläche, bindet Fachmodule ein und nutzt die Infrastruktur über Dependency Injection.

---

## ITW.Application
`ITW.Application` darf referenzieren:

- `ITW.Domain`

### Begründung
Zentrale Anwendungsfälle dürfen gemeinsame fachliche Kernbegriffe kennen, sollen aber nicht direkt von Web oder Infrastruktur abhängen.

---

## ITW.Domain
`ITW.Domain` darf referenzieren:

- keine Produktivprojekte der Solution

### Begründung
`ITW.Domain` ist der stabile fachliche Kern und darf nicht von anderen Projekten abhängig werden.

---

## ITW.Infrastructure
`ITW.Infrastructure` darf referenzieren:

- `ITW.Application`
- `ITW.Domain`
- `ITW.Dienstplan`

### Begründung
Die technische Umsetzung der Plattform muss zentrale Anwendungsabstraktionen und gemeinsame Kernbegriffe kennen.  
Im aktuellen Stand liegt zusätzlich technische Persistenzanbindung für Dienstplandaten in `ITW.Infrastructure`.

---

## ITW.Dienstplan
`ITW.Dienstplan` darf referenzieren:

- `ITW.Domain`

### Begründung
Das Dienstplanmodul nutzt gemeinsame fachliche Kernbegriffe, bleibt aber fachlich eigenständig.

---

## ITW.Einsatz
`ITW.Einsatz` darf referenzieren:

- `ITW.Domain`

### Begründung
Das Einsatzmodul nutzt gemeinsame fachliche Kernbegriffe, bleibt aber fachlich eigenständig.

---

## 4. Verbotene Referenzen je Projekt

## ITW.Web
`ITW.Web` darf nicht von anderen Produktivprojekten referenziert werden.

## ITW.Application
`ITW.Application` darf nicht referenzieren:

- `ITW.Web`
- `ITW.Infrastructure`
- `ITW.Dienstplan`
- `ITW.Einsatz`

## ITW.Domain
`ITW.Domain` darf keine Produktivprojekte referenzieren.

## ITW.Infrastructure
`ITW.Infrastructure` darf nicht referenzieren:

- `ITW.Web`
- `ITW.Einsatz`

## ITW.Dienstplan
`ITW.Dienstplan` darf nicht referenzieren:

- `ITW.Web`
- `ITW.Application`
- `ITW.Infrastructure`
- `ITW.Einsatz`

## ITW.Einsatz
`ITW.Einsatz` darf nicht referenzieren:

- `ITW.Web`
- `ITW.Application`
- `ITW.Infrastructure`
- `ITW.Dienstplan`

---

## 5. Matrix

Legende:

- **Ja** = direkte Referenz ist erlaubt
- **Nein** = direkte Referenz ist nicht erlaubt
- **-** = Projekt selbst

| Von \\ Nach         | ITW.Web | ITW.Application | ITW.Domain | ITW.Infrastructure | ITW.Dienstplan | ITW.Einsatz |
|---------------------|---------|-----------------|------------|--------------------|----------------|-------------|
| ITW.Web             | -       | Ja              | Nein       | Ja                 | Ja             | Ja          |
| ITW.Application     | Nein    | -               | Ja         | Nein               | Nein           | Nein        |
| ITW.Domain          | Nein    | Nein            | -          | Nein               | Nein           | Nein        |
| ITW.Infrastructure  | Nein    | Ja              | Ja         | -                  | Ja             | Nein        |
| ITW.Dienstplan      | Nein    | Nein            | Ja         | Nein               | -              | Nein        |
| ITW.Einsatz         | Nein    | Nein            | Ja         | Nein               | Nein           | -           |

---

## 6. Aktueller technischer Stand

Der aktuelle technische Stand der Solution entspricht dieser Richtung:

- `ITW.Web` referenziert `ITW.Application`, `ITW.Dienstplan`, `ITW.Einsatz` und `ITW.Infrastructure`
- `ITW.Application` referenziert `ITW.Domain`
- `ITW.Infrastructure` referenziert `ITW.Application`, `ITW.Domain` und `ITW.Dienstplan`
- `ITW.Dienstplan` referenziert `ITW.Domain`
- `ITW.Einsatz` referenziert `ITW.Domain`

Diese Matrix dokumentiert also den Ist-Stand und macht ihn verbindlich nachvollziehbar.

---

## 7. Bedeutung für spätere Umbauten

Wenn künftig weitere gemeinsame fachliche oder technische Bausteine benötigt werden, gilt:

- nicht vorschnell neue Projekte einführen
- keine Referenzregeln aufweichen, nur um kurzfristig schneller zu sein
- zuerst prüfen, ob die Funktion in ein bestehendes Projekt gehört
- fachliche Ownership immer getrennt von technischer Referenz betrachten

Das ist besonders wichtig bei:

- Benutzer- und Organisationslogik
- Personaldaten
- Dienstplan-Persistenz
- späteren Einsatzerweiterungen

---

## 8. Zusammenfassung

Die Referenzmatrix schützt die Lösung vor unkontrollierter Kopplung.

Die wesentlichen Regeln sind:

- `ITW.Web` bleibt die Oberfläche
- `ITW.Application` bleibt zentrale Anwendungslogik
- `ITW.Domain` bleibt der gemeinsame Kern
- `ITW.Infrastructure` bleibt technische Umsetzung
- `ITW.Dienstplan` und `ITW.Einsatz` bleiben fachliche Module
- `ITW.Infrastructure -> ITW.Dienstplan` ist im aktuellen Stand erlaubt und dokumentiert
- keine Produktivkomponente darf `ITW.Web` kennen