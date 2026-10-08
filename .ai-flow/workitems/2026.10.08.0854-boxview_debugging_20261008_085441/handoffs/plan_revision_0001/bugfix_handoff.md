# Debug Handoff

## Current Issue / Debug Request

The user reports that the click failure remains after the previous implementation: after switching to the ScriptEditor view, a rendered BoxStringSignal shows a hand cursor but does not open its editing dialog.

## Tried Approaches

- Previous implementation present at DEBUG entry: initialize BoxStringSignal visibility/clickability, keep its frame passive and its value area clickable only when visible and unlocked, and exclude effectively hidden controls in GetControlUnderCursor. The user reports that these measures alone did not resolve the runtime issue.
- DEBUG correction attempt 1 (2026-10-08): add Read.Directory = Page immediately after Frame.Directory = Page in BoxStringSignal.Create. This explicitly assigns the existing editable value area to its host page. Preserve the single existing callback, value binding, geometry, lock policy and existing unrelated edits, including the pre-existing edit log line.
- Required automatic validation: registered RunWorker invoked with command Build and parameter qbookStudio.sln. The invocation-local response has Success true; the embedded result has success true, integer exitCode 0, empty stdoutTail/stderrTail, false truncation flags and report .ai-flow/BuildReport.txt. Complete Release|x86 solution Build passed. No Clean, Start, runtime/UI check, test suite or direct-process validation was executed.
- Existing .vscode/tasks.json matches the approved process command, string-array arguments, cwd, owner, action and target; the specified MSBuild executable exists. Build and launch configuration were already present and were preserved.

## Invalidated Hypotheses

- Visibility/clickability fixes alone are not a sufficient explanation or remedy for the user's remaining issue. This does not invalidate their role in preventing hidden hits or unlocked-frame clicks.

## Open Hypotheses

- Confirmed source-level gap: BoxStringSignal previously set only Frame.Directory explicitly. Item.Directory is a plain property initialized from the caller's Definition namespace, and Root.GetControlsByDirectory filters each registered control by exact Directory equality. Box.Render renders its children directly, independently of that directory filter. Thus a value area created inside DefinitionFunkyLib can render on the explicitly selected host page while remaining absent from that page's hit selection when the page differs from the inferred directory. The new assignment closes this gap; its role in the reported runtime instance remains unverified.
- Runtime assumptions still need confirmation if the issue persists: the application uses the newly built library; its Page differs from the inferred FunkyLib directory as suspected; the manual Clickable/ZOrder overrides from the reference example have been removed by the user; the click falls within the existing inner value area.
- Read-only observation for a future scoped investigation: PageControl.Page_MouseDown computes x1/y1 from the current event but calls GetControlUnderCursor with x2/y2 from mouse movement. No change was made; this path is outside the current approved write scope and has not been established as the cause.

## Do-Not-Retry Guidance

- Do not repeat the visibility-only correction as a new explanation, attach another edit callback, make the description/frame editable, change visible-control ordering, or modify the external example.
- A successful Build establishes compilation/integration only. Do not claim runtime acceptance or use prior result files as fresh Worker evidence.
- Changes to PageControl.cs or general directory/input infrastructure require a revised PLAN scope. Do not widen this DEBUG correction to those files.

## Evidence / References

- Approved archived Implementation Handoff: handoffs/archive/2026.10.08.0854-implementation-handoff.md. Active workitem.md has validation: true.
- qbookCsScript/Controls/BoxView.cs: BoxStringSignal.Create, SetVisible and BoxTable visibility propagation.
- qbookCsScript/Controls/Object.cs: Item.Directory and GetDirectoryFromCaller (read-only).
- qbookCsScript/Base.cs: Root.GetControlsByDirectory (read-only).
- qbookCsScript/Controls/Box.cs: child rendering and existing OnBoxClick dispatch (read-only).
- qbook/Controls/PageControl.cs: mouse-down selection and mouse-up dispatch (read-only).
- Reference attachment FunkyLib.view.cs defines FunkyView in DefinitionFunkyLib, accepts a separate page argument, and contains manual Clickable/ZOrder overrides in SwitchView(4). The attachment was not modified.
- Worker artifacts preserved: .ai-flow/BuildResult.json and .ai-flow/BuildReport.txt. No runtime reproduction evidence was obtained in this DEBUG invocation.
