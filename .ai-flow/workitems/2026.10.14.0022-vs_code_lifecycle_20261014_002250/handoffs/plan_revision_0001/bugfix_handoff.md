# Debug Handoff

## Current Issue / Debug Request

Implement wurde abgebrochen. Bitte neu starten

The active request runs in DEBUG. Continue the archived implementation scope without changing Core-managed workflow state. Authoritative workitem validation remains true.

## Tried Approaches

- 2026-10-14, correction 1: inspected the interrupted workspace and retained existing unrelated edits. Preserved host book/model metadata during cleanup; published replacement book/model/facade together on the UI owner; retained configured directories and resolved lazy defaults from the owning book instance. Suppressed runtime rendering/input until initialization succeeds. Kept host frame drawing during teardown and Stop, without reenabling old native runtime controls. Made registry cleanup use locked snapshots and invoke callbacks outside registry locks. Aggregated cleanup failures, cleared runtime bindings and requested assembly unloading even on cleanup failure. Prevented replay of the old global Destroy through released-cycle state. Preserved client Save/Backup and host-only Run sequencing. Added underlying directory-access diagnostics while retaining facade null-return semantics.
- Worker Build for qbook/qbook.csproj succeeded with outer Success true, embedded success true, integer exitCode 0, report .ai-flow/BuildReport.txt, empty output tails and false truncation flags.
- Final correction review: retained distinct Cleanup/Load/Build stages in failure reporting; removed snapshot-owned module/signal registrations before callbacks to avoid adding duplicate destruction through side registries. Repeated the same approved Worker Build against the final source state. Terminal response again passed all canonical fields. No Clean, direct-process build, dependency restore, process termination, launch or runtime exercise was performed.
- Modified scoped source files: qbook/Studio/BookRuntime.cs, qbook/Studio/Core.cs, qbook/Studio/Book.cs, qbook/Studio/BookBuilder.cs, qbook/Net/PipeCommands.cs, qbook/Controls/PageControl.cs, qbookCsScript/Base.cs, qbookCsScript/Book.cs, qbookCsScript/Controls/ItemDestroyHandler.cs. Existing MainForm/Roslyn/extension changes were retained; no additional changes were made there.

## Invalidated Hypotheses

- Retaining the PageControl alone preserves header/footer: false by source inspection. OnPaint explicitly cleared the surface and returned during teardown, although PageAmium owns host frame drawing in that surface.
- Directory resets preserve Book.json settings: false by source inspection. FromFolder reads configured directory values, which OpenQbookAsync immediately discarded.
- A registry lock makes callback-driven live enumeration safe: false. Reentrant callbacks can unregister entries; calling UI-marshaled cleanup under a registry lock can also prevent progress.
- No evidence establishes that the collection and path exceptions have one common cause.

## Open Hypotheses

- The exact throwing enumeration for the reported collection exception remains unconfirmed without the runtime stack trace. Registry cleanup contained mutation-sensitive enumerations and callback-driven removal paths; these were corrected.
- FunkyTest passes QB.Book.DataDirectory as Path.Combine path1. The facade can return null after a missing active binding or swallowed getter error. Configuration loss and cross-book default resolution are established defects, but the precise observed null-producing branch remains unconfirmed.
- Runtime acceptance is unverified: Start followed by repeated Rebuild, Stop followed by Rebuild, retained header/footer/navigation/status through failure, and corrected failure recovery require authorized observations. Compilation does not establish these results.
- Cleanup failure is reported and bindings are cleared; disposal failure can still leave the underlying resource incompletely released. No hardware or external book exercise was authorized or performed.

## Do-Not-Retry Guidance

- Preserve the approved Worker Build target qbook/qbook.csproj and existing process task/script. Do not substitute dotnet build, direct MSBuild, Clean, dependency restore or automatic Start.
- Do not clear the host model or whole drawing surface to stop runtime content. Do not restore native controls from the removed runtime after teardown.
- Do not replay obsolete global Destroy on the replacement book or replace configured paths with arbitrary fallbacks.
- Do not treat the two successful compilation results as runtime acceptance or evidence of the original exact exception stack.
- Preserve protected files, external book sources/metadata, existing unrelated workspace edits and Core-managed workflow state.

## Evidence / References

- Archived implementation handoff: handoffs/archive/2026.10.14.003200-implementation-handoff.md.
- qbook/Studio/Core.cs: former host-model clearing and null directory assignments; replacement publication and stage tracking.
- qbook/Studio/Book.cs: former directory defaults referenced Core.ThisBook instead of the receiver.
- qbook/Studio/BookBuilder.cs: configured directory deserialization and premature facade publication.
- qbookCsScript/Base.cs and qbookCsScript/Controls/Control.cs: live registry reset callbacks and callback-driven control unregistration.
- qbookCsScript/Controls/Object.cs: item destroy registration and shared object/module/signal registry ownership, inspected read-only.
- qbook/Controls/PageControl.cs and qbook/oControls/PageAmium.cs: former teardown clear/return and host header/footer drawing.
- B:/Audi/CellTester_2026.08.26.code/Pages/FunkyLib/FunkyLib.testDevice.cs: Path.Combine uses QB.Book.DataDirectory in the constructor; inspected read-only.
- Final invocation-local RunWorker(Build, qbook/qbook.csproj): outer Success true; canonical embedded result success true, exitCode 0, stdoutTail [], stderrTail [], stdoutTruncated false, stderrTruncated false, report .ai-flow/BuildReport.txt.
- Worker-produced BuildResult.json and .ai-flow/BuildReport.txt are preserved. They are diagnostic artifacts, not replacements for invocation-local Worker evidence.