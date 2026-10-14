# Debug Handoff

## Current Issue / Debug Request

Aktuell das selbe verhalten

- 2026-10-13, subsequent request: stopping gradually removes the UI instead of clearing it at once, and the status does not switch to Stop. This is a separate runtime issue from the approved sidebar file-name change.

## Tried Approaches

- 2026-10-13, Stop investigation: inspected PipeCommands.Stop, BookRuntime.StopAll/ReleaseResources, Core.StatusText/PipeHeartbeat, RuntimeWatchdog.Stop, the extension Stop dispatcher, incoming runtime signal parser, and notifyRuntimeSignal. No source corrections were made because the approved implementation handoff explicitly excludes runtime behavior and pipe communication changes.
- Confirmed Stop executes BookRuntime.StopAll on a background task. ReleaseResources stops the watchdog, invokes runtime/page Destroy, calls ItemDestroyHandler.DestroyAll, and then removes controls individually before resetting dictionaries. No enclosing UI-clear batching is visible in that method; actual repaint behavior inside Remove/Destroy was not verified.
- Confirmed Status:Stop is assigned only after ReleaseResources returns successfully. ReleaseResources failures set Alert:Stop and rethrow; PipeCommands.Stop catches the exception and writes only Debug output. Core publishes its current status on a two-second heartbeat. The extension recognizes Status:Stop and updates its status text to Stopped upon receiving it.
- Confirmed validation=true and the approved root process task, executable, and local compiler prerequisites. Invoked RunWorker(command=Build, parameter=VScodePlugin/amium-qbook) in this Stop investigation. Actual response: Success=true; embedded result has exactly seven properties, success=true, exitCode=0, empty stdoutTail/stderrTail, false truncation flags, and report=.ai-flow/BuildReport.txt. This proves extension compilation only, not runtime Stop correctness. No Clean, runtime launch, tests, packaging, installation, or direct-process validation was performed.
- 2026-10-13: Inspected the approved display-name function, compiled workspace output, latest VSIX package, installed extension, and webview rendering. All inspected treeService implementations already remove only the terminal .cs extension and exact page prefix while preserving casing. The webview renders file.displayName without aliasing.
- No additional source correction was justified; existing unrelated workspace modifications were preserved. The approved root Build task already matches the handoff, and Node plus the local TypeScript compiler are present.
- Invoked RunWorker with command Build and parameter VScodePlugin/amium-qbook. The response has Success=true and exactly seven embedded result properties: success=true, exitCode=0, empty stdoutTail/stderrTail, false truncation flags, report=.ai-flow/BuildReport.txt. No Clean, packaging, installation, launch, or additional tests were performed.

## Invalidated Hypotheses

- The inspected workspace source and compiled output still contain the old display aliases: contradicted by their current contents.
- The latest inspected VSIX and installed amium.amium-qbook-2026.1013.1815 still contain the old display aliases: contradicted by their current contents. This does not prove which code is loaded by the running extension host.

## Open Hypotheses

- Stop request: serial control removal and/or page/item Destroy operations may allow intermediate UI repainting. Inspect their implementations and the UI thread/rendering boundary under a newly approved runtime scope before choosing an atomic visual-clear correction.
- Stop request: slow or blocked teardown delays Status:Stop; an exception prevents it entirely and leaves Alert:Stop. A successfully completed teardown still relies on the next heartbeat. Runtime logs, teardown completion, and received pipe messages are needed to distinguish these cases; the observed running process was not inspected.
- Stop request: assumed the request concerns the runtime Stop action and rendered book UI. If it instead concerns debugger Stop or another status display, identify that exact UI before correction.
- A new PLAN must authorize the runtime/pipe source files and a suitable complete-project validation target for qbook. The current approved extension Build does not compile the C# Stop implementation.
- A running extension host or existing webview may retain older code/data; a manual VS Code window reload and renewed panel display would distinguish this hypothesis.
- The affected filename, page prefix, or displayed value may differ from the approved examples. Obtain one exact filename/path and its current sidebar label before making further corrections.
- The active VS Code profile, remote extension host, or development host may load another extension copy. The inspected local installation alone does not identify the running host.

## Do-Not-Retry Guidance

- Do not modify BookRuntime.cs, PipeCommands.cs, Core.cs, the extension dispatcher, or status presentation under the current sidebar-only implementation handoff. Do not substitute a qbookStudio Build for the explicitly selected extension target without a new PLAN.
- Do not reintroduce aliases or rewrite the already-correct generic display function without concrete contradictory input/output evidence.
- Do not package, install, change versions, alter metadata/files, or modify other source files under this handoff.

## Evidence / References

- Source: VScodePlugin/amium-qbook/src/services/book/treeService.ts, getDisplayName.
- Compiled output: VScodePlugin/amium-qbook/out/services/book/treeService.js.
- Package inspected read-only: VScodePlugin/amium-qbook/Setup/amium-qbook-2026.1013.1815.vsix, extension/out/services/book/treeService.js.
- Local installation inspected read-only: C:/Users/StefanFunk/.vscode/extensions/amium.amium-qbook-2026.1013.1815/out/services/book/treeService.js.
- Renderer: VScodePlugin/amium-qbook/src/webview/panelHtml.ts, file.displayName rendering.
- Active workitem: validation=true; status=failed; progress records an earlier implementation transport error. No prior build success was inferred from that metadata.
- Invocation-local Worker Build response on 2026-10-13 establishes compiler success only; the reported unchanged visible behavior remains unverified.
