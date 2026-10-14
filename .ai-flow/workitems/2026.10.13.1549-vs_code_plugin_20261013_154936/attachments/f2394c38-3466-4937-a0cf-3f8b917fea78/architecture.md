# amium-qbook – Architekturüberblick

## Ziel
Die Extension bündelt qBook-Workflows in klar getrennten Domänen, damit Änderungen lokal bleiben und das Verhalten leichter nachvollziehbar ist.

## Modulstruktur
- `src/extension.ts`
  - Aktivierung der Extension
  - Initialisierung von View Provider und Pipe Bridge
  - Orchestrierung zentraler Zustände
- `src/services/book`
  - Page-/Subnode-Workflows
  - Book-/Metadata-Dateien lesen und schreiben
  - Verifikation und Tree-Aufbereitung
- `src/services/debug`
  - Start/Stop von Debug-Sessions
  - Attach-Konfiguration und Session-Lifecycle
  - Debug-spezifisches Logging
- `src/services/ui`
  - Webview-Message-Dispatch
  - Status-Rückmeldungen an die Webview
  - Book-Status-Aufbereitung (Selection, Errors, Form-Daten)
- `src/pipes`
  - Pipe-Kommunikation mit Runtime
  - Empfang/Parsing von Runtime-Signalen
  - Versand von Runtime-Commands
- `src/types`
  - Geteilte Typen für Nachrichten, Payloads und Metadaten

## Hauptfluss (Webview -> Runtime)
1. Webview sendet eine Nachricht.
2. `webviewMessageDispatcher` ordnet die Nachricht einem Command-Handler zu.
3. Der passende Domain-Service führt die Aktion aus (`book`, `debug`, `pipes`).
4. Ergebnis/Status wird über `webviewStatusService` an die Webview zurückgemeldet.

## Hauptfluss (Runtime -> Webview)
1. Pipe-Nachricht wird über `namedPipeClients` empfangen.
2. `pipeCommands` interpretiert Signal/Log-Level.
3. View Provider aktualisiert Runtime-Button-Status und Status-Text.
4. UI wird per Webview-Message aktualisiert.

## Zentrale Zustände im View Provider
- `lastBookRoot`: aktuelles Projekt-Root für qBook-Operationen
- `lastPayload`: letzter Baumzustand für UI-Aktualisierung
- `selectedPath`: aktuell selektierte Datei im Tree
- `errorPaths`: Pfade mit aktuellen C#-Fehlern
- `pipeStatus`: Verbindungsstatus der Runtime-Pipe

## Erweiterungsprinzip
- Neue Webview-Commands zuerst im Dispatcher verdrahten.
- Business-Logik in den passenden Domain-Service auslagern.
- Typen in `src/types/extensionTypes.ts` zentral ergänzen.
- UI-Feedback nur über die UI-Status-Services senden.
