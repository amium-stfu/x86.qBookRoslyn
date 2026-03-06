# Debug Services

## Verantwortung
`debug` kapselt den kompletten Debug-Lebenszyklus:
- Attach-Konfiguration ermitteln
- Debug-Start und Debug-Stop ausführen
- Session-Lifecycle überwachen
- Debug-Landschaft und Session-Ereignisse protokollieren

## Typische Einstiegspunkte
- `debugCommandService.ts`
  - Command-Handler für Start/Stop
- `attachService.ts`
  - Auswahl und Start über Launch-Konfiguration
- `sessionLifecycleService.ts`
  - Reaktion auf Start/Terminate/Active-Session-Events

## Unterstützende Module
- `helpers.ts`
  - Session-Erkennung, JSON-Helfer, Config-Auswahl
- `launchConfigService.ts`
  - Lesen/Schreiben von `.vscode/launch.json`
- `logging.ts`
  - Strukturierte Runtime-/Debug-Logs

## Konvention
- Debug-Status immer in die Webview zurückspiegeln.
- Attach-Fehler klar loggen und als UI-Status melden.
- Session-Erkennung zentral halten (keine duplizierte Heuristik).
