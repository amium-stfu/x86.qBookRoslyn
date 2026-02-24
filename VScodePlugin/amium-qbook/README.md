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
