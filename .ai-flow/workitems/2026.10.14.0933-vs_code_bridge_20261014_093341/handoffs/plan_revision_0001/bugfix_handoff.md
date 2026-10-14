# Debug Handoff

## Current Issue / Debug Request

Run wird bereits als Fehler beendet. Allerdings sendet ExecuteLifecycle über Alert:Run den vollständigen ex.ToString()-Text, den das Plugin direkt im Status anzeigt.

Die Korrektur ist definiert:

Vorbedingung: Run ohne erfolgreiche Initialisierung früh abweisen, ohne Program.Run() auszuführen oder den Runtime-Zustand zu verändern.
Status: kurze Meldung wie Run unavailable: Initialize first.; kein Stacktrace.
Log: vollständige Exception-Details weiterhin protokollieren. Auch andere Lifecycle-Fehler erhalten kurze Statusmeldungen.
Abnahme: Nach der Ablehnung endet der Fortschrittsstatus; Initialize → Run bleibt möglich.
Abgrenzung: Button-Zuordnung, Host-Automatik und Timeout-Verhalten bleiben unverändert.
status: ready und planning_ready: true sind gesetzt. validation: true bleibt erhalten; die genaue Build-Verifikation wird an PLAN delegiert. Build- und Launch-Konfiguration sind vorhanden.

Keine offenen materiellen Entscheidungen. Nur Workflow-Metadaten wurden geändert.

## Tried Approaches

- Attempt 1 (2026-10-14): Updated `qbook/Net/PipeCommands.cs`. ExecuteLifecycle validates Run before calling RunAll, without changing BookRuntime state. Missing initialization returns `Run unavailable: Initialize first.`; missing assembly/build and repeated Run also receive concise rejection messages. Other lifecycle exceptions return an action-specific short failure message; full exceptions remain in LogError and LogRichException.
- Existing extension handling clears runtimeInFlight and ends progress on lifecycle Alert. No plugin, host automation, button mapping, or timeout changes were needed. Runtime behavior was not exercised; this conclusion is based on source inspection and the existing sequential pipe processing contract.
- Authorized RunWorker Build for `qbook/qbook.csproj` returned Success=false, embedded success=false, exitCode=1, report=`.ai-flow/BuildReport.txt`. MSB3027/MSB3021: qbookCsScript.dll is locked by qbook (39620). The sequential plugin build did not run. Build validation remains unsuccessful.

## Invalidated Hypotheses

- None recorded yet.

## Open Hypotheses

- After the running host releases qbookCsScript.dll, repeat the authorized full Worker Build. No runtime acceptance proof is available from this attempt.

## Do-Not-Retry Guidance

- Do not repeat Build while the output DLL remains locked. Do not kill the host, change build tools, restore dependencies, or alter configuration to bypass this environment issue.

## Evidence / References

- `qbook/Net/PipeCommands.cs`: ExecuteLifecycle and Run.
- `VScodePlugin/amium-qbook/src/extension.ts`: notifyRuntimeSignal handles terminal lifecycle alerts.
- Invocation-local RunWorker response and `.ai-flow/BuildReport.txt`, 2026-10-14 11:13:47, exit code 1.
