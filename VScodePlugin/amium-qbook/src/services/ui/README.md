# UI Services

## Verantwortung
`ui` ist die Schicht zwischen Webview und Fachlogik:
- Webview-Nachrichten dispatchen
- Statusmeldungen standardisiert zurücksenden
- Book-Status für Selection/Errors/Form aufbereiten

## Typische Einstiegspunkte
- `webviewMessageDispatcher.ts`
  - Routing aller eingehenden Webview-Messages
- `webviewStatusService.ts`
  - Einheitliche Outbound-Messages (`statusText`, `pipeStatus`, `runtimeState`, `debugState`)
- `bookStatusService.ts`
  - Zusammensetzen von `bookStatus` inkl. Form-Zustand

## Konvention
- Business-Logik nicht im Dispatcher halten, sondern in Domain-Services.
- Message-Formate konsistent über `src/types/extensionTypes.ts` halten.
- UI-Status zentral posten, nicht ad hoc in mehreren Modulen.
