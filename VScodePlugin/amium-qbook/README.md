## amium qBook – VS Code Bridge

Dieses VS Code-Plugin zeigt ein qBook-Baumview im Explorer an und verbindet den Editor mit der laufenden qBook-Runtime über Named Pipes.

### Funktionen

- Seitenstruktur aus der `Book.json` als Baum anzeigen
- Page-Metadaten (Titel, Format, Hidden) direkt in VS Code bearbeiten
- Pages anlegen, importieren, umbenennen und löschen (`Pages/<PageName>`-Ordner, `Book.json`, `Program.cs` werden aktualisiert)
- Subcode-Dateien pro Page anlegen, umbenennen und löschen (inkl. Pflege der `oPage.json`/`CodeOrder`/`Includes`)
- Drag & Drop zur Neuordnung der Pages (`PageOrder` in `Book.json`)
- Runtime-Steuerung (Run/Stop/Rebuild) über benutzerdefinierte Buttons und Named Pipes

### Voraussetzungen

- Installiertes qBook-Projekt mit `Book.json` und `Pages`-Ordner
- Konfigurierte Pipe-Namen in den VS Code Settings (`amium-qbook.pipe.*`) oder in der Projektdatei `pipes.json`

### Aktivierung

Die Erweiterung wird aktiviert, wenn der qBook-View im Explorer geöffnet oder der Befehl „Open qBook Panel“ ausgeführt wird.

### Dokumentation

- Architekturüberblick: [docs/architecture.md](docs/architecture.md)
- Book Services: [src/services/book/README.md](src/services/book/README.md)
- Debug Services: [src/services/debug/README.md](src/services/debug/README.md)
- UI Services: [src/services/ui/README.md](src/services/ui/README.md)
- Pipe Layer: [src/pipes/README.md](src/pipes/README.md)

### Contributing (Doku-Regeln)

- Architekturänderung (neue Schicht, neuer Hauptfluss, neue zentrale Zustände): `docs/architecture.md` aktualisieren.
- Änderungen innerhalb einer Domäne (`book`, `debug`, `ui`, `pipes`): passendes Bereichs-README aktualisieren.
- Neue zentrale Typen/Nachrichten: betroffene Abschnittsbeschreibung in Architektur- oder Bereichs-Doku ergänzen.
- Kleine Refactorings ohne Verhaltensänderung: nur Doku anpassen, wenn sich Verantwortlichkeiten/Dateigrenzen geändert haben.
- Ziel: Doku kurz halten, aber immer den aktuellen Einstieg und die Zuständigkeiten korrekt abbilden.
