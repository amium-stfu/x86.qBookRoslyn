# Pipe Layer

## Verantwortung
`pipes` verwaltet die Runtime-Kommunikation:
- Pipe-Verbindung aufbauen und überwachen
- Runtime-Commands senden
- Eingehende Runtime-Signale und Runtime-Logs interpretieren

## Wichtige Dateien
- `namedPipeClients.ts`
  - Technische Pipe-Bridge und Eventing
- `pipeCommands.ts`
  - Parsing/Normalisierung von Runtime-Signalen
  - Versand von Runtime-Commands mit Fehlerbehandlung

## Signal- und Command-Idee
- Inbound: Runtime -> Signal (z. B. `status`/`alert` für Buttons)
- Outbound: UI-Action -> Pipe-Command (`Run`, `Destroy`, `Rebuild`, ...)

## Konvention
- Parsing zentral in `pipeCommands.ts` bündeln.
- Fehler beim Senden immer als UI-Fehler und Statuswechsel behandeln.
- Log-Nachrichten getrennt von UI-Signal-Parsing verarbeiten.
