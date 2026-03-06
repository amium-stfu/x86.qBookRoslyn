# Book Services

## Verantwortung
`book` enthält alle qBook-spezifischen Datei- und Struktur-Workflows:
- Pages anlegen/importieren/umbenennen/löschen
- Subnodes anlegen/umbenennen/löschen
- `Book.json`, `oPage.json` und `Program.cs` pflegen
- Tree-Daten für die Webview aufbauen
- Verifikationsläufe für C#-Dateien orchestrieren

## Typische Einstiegspunkte
- `pageCommandService.ts`
  - Page-Commands aus der UI
- `subnodeCommandService.ts`
  - Subnode-Commands aus der UI
- `pageMetadataCommandService.ts`
  - Titel/Format/Hidden-Änderungen

## Wichtige Helfer
- `bookFileService.ts`: Zugriff auf `Book.json`
- `pageMetadataFileService.ts`: Zugriff auf `oPage.json`
- `programClassService.ts`: Erzeugung/Update `Program.cs`
- `treeService.ts`: Aufbau der Tree-Payload
- `verificationService.ts` + `verifyWorkflowService.ts`: Verifikationslogik

## Konvention
- Validierung und Normalisierung früh (siehe `validation.ts`).
- Dateisystemoperationen in dedizierten Workflow-/File-Services.
- UI-Meldungen möglichst im Command-Layer, nicht tief in Hilfsfunktionen.
