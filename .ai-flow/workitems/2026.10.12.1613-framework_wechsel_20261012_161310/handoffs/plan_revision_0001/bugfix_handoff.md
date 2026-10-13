# Debug Handoff

## Generated .csproj correction (2026-10-12)

### Original issue and confirmed causes

- Reported external build diagnostic: NETSDK1136 with Windows Forms enabled and `net10.0` without the Windows suffix; reported legacy Framework/GAC references and duplicates. The exact failing project path/build log was not supplied.
- All three current source generators already specified `net10.0-windows` on entry. Repository HEAD's BookBuilder explicitly specified `net8.0` in its default and save/load calls. The nested `DebugProject/GO83P05 mShedAK/mSHED_AK GO83P05.data/mSHED_AK GO83P05.code/mSHED_AK GO83P05.csproj` still contains `net8.0`, Framework/GAC references and duplicates; the authoritative top-level project specifies `net10.0-windows`. These establish multiple project generations, but do not prove which file the earlier build consumed.
- BookBuilder exported the current Roslyn project's metadata references. Framework detection compared exact runtime paths only, so copied/legacy framework paths were not recognized by assembly name. MainForm had no duplicate filtering; CsprojGenerator deduplicated paths only. Both absolute-path generators excluded all Microsoft.CodeAnalysis references without checking their use.
- The new-book call passed the project file path as the second positional argument (`targetFramework`), leaving the output file at its default path.

### Correction measures

- Modified only `qbook/CsprojGenerator.cs`, `qbook/Studio/BookBuilder.cs`, `qbook/Controls/MainForm.cs`, and `qbook/RoslynServices/RoslynService.cs` for source corrections.
- All three generators now use `CsprojGenerator.BookTargetFramework`, fixed to `net10.0-windows`. Removed BookBuilder's variable framework argument and made output-file arguments explicit, including the defective new-book call. Existing output types, assembly names, source selection, WinForms/WPF/x64 properties and debugging configuration are preserved.
- Added component-local project-reference filtering in RoslynService: absolute paths; duplicate suppression by managed assembly simple name; SDK framework exclusion by runtime framework assembly names as well as paths; native DLL and prior Book output exclusion. The runtime platform set now uses the actual Core/WindowsDesktop framework directories, preserving external third-party assemblies as application references.
- Non-SDK legacy Microsoft-signed references from Framework/GAC locations stop export with an explicit migration error instead of silently omitting an unsupported API dependency. Publisher tokens distinguish framework candidates from third-party names such as System.Data.SQLite. Modern package references such as System.IO.Ports remain when absent from the desktop framework.
- BookBuilder includes the host's managed references even before an in-memory book project exists, while retaining additional current project references. Removed the blanket Microsoft.CodeAnalysis exclusions. MainForm now XML-escapes reference names and paths. No protected files, operational data, nested copies, launch/tasks configuration or packages were modified by this correction.

### Validation results

- Active workitem `validation: true`; approved selection remains `Worker Command: Build`, `Worker Target: qbookStudio.sln`, Clean false. Inspected the existing task, configured Visual Studio MSBuild executable and both application restore asset files before execution. No restore was run.
- Invoked RunWorker Build twice: the initial correction passed; after refining legacy publisher detection to preserve third-party GAC references, the final Build passed again. The last invocation-local response has `Success: true` and exactly seven embedded properties: `success: true`, integer `exitCode: 0`, empty stdout/stderr tails, false truncation flags, and `report: .ai-flow/BuildReport.txt`. Warnings remain in the full report.
- Generated-project `dotnet build <generated.csproj> --no-incremental` was not executed: the hosted rules authorize only the existing solution Worker Build and prohibit a direct substitute build or autonomous restore. No generated-project Worker task is scoped. A new PLAN must select and scope that validation if required in the hosted workflow.
- No running application export, in-memory Roslyn book compilation or debugger session was exercised. Solution Build proves source compilation, not those runtime outcomes or successful external book compilation. Existing VS Code launch configuration was inspected and preserved.

### Remaining hypotheses, assumptions and do-not-retry guidance

- A stale running binary or a different nested/generated project could explain the reported `net8.0`/`net10.0` discrepancy; confirm with the exact external build path and fresh export from the rebuilt application. Do not rewrite `.data`/backup copies to disguise stale generation.
- Absolute references support compilation with QBook stopped provided the exported host/third-party DLLs remain at those paths and the .NET 10 Windows SDK/restore assets are available. This is an implementation property, not an executed external-build result.
- Assembly simple names are treated as reference identities, consistent with the existing Roslyn reference collector; the first candidate wins, with host runtime/deployment references preceding additional project references.
- Do not restore Framework/GAC SDK references, use broad `System.*` or Roslyn-prefix deletion, reintroduce a variable non-Windows target for these desktop exports, change the compiler architecture, or install/restore dependencies autonomously. Unsupported legacy API dependencies require a concrete compatible package/API decision.

## Current Issue / Debug Request

Latest request: `System.Configuration.ConfigurationErrorsException: Configuration system failed to initialize` while reading `qbook.Properties.Settings.UpgradeRequired` in `MainForm` construction. Inner exception: `Unrecognized configuration section system.data`, generated `qbook/bin/x64/Release/qbookStudio.dll.config`, original line 30.

Previous request (retained debug history):

Severity	Code	Description	Project	File	Line	Suppression State	Details
Error (active)	CS0117	'CefSharpSettings' does not contain a definition for 'WcfEnabled'	qbook	B:\x86.qBookRoslyn\qbook\oControls\cHtml.cs	29

## Tried Approaches

- Configuration issue, attempt 1 (2026-10-12): Removed the legacy `system.data/DbProviderFactories` section from `qbook/App.config` and `qbookCsScript/app.config`. Registered the same `System.Data.SQLite` and `System.Data.SQLite.EF6` invariant names and assembly-qualified factory types using `System.Data.Common.DbProviderFactories.RegisterFactory` at the beginning of `qbook/Program.cs` Main. Existing user settings and Entity Framework provider configuration were preserved. No generated configuration was hand-edited.
- Configuration issue validation: Invoked `mcp__aiflow_worker__RunWorker` with `command: Build`, `parameter: qbookStudio.sln`, unchanged Visual Studio MSBuild Release/x64 task, without Clean or restore. Invocation-local response `Success: true`; embedded result contains exactly seven properties: `success: true`, integer `exitCode: 0`, `stdoutTail: []`, `stderrTail: []`, `stdoutTruncated: false`, `stderrTruncated: false`, `report: .ai-flow/BuildReport.txt`. Read-only inspection of the generated application configuration confirms the offending section is absent. Application startup and SQLite factory resolution/database operations were not exercised; Build does not establish runtime success.

- Attempt 1 (2026-10-12): Removed `CefSharpSettings.WcfEnabled = false` from the static constructor in `qbook/oControls/cHtml.cs`. The CefSharp 141.0.110 source excludes this property under `NETCOREAPP`; the migrated project references CefSharp's .NET Core packages. No replacement setting is required to keep WCF disabled.
- Validation: Invoked `mcp__aiflow_worker__RunWorker` with `command: Build`, `parameter: qbookStudio.sln`, using the unchanged Visual Studio MSBuild Release/x64 process task. Worker response `Success: true`; embedded result `success: true`, `exitCode: 0`, `report: .ai-flow/BuildReport.txt`, empty output tails and false truncation flags. The complete solution Build passed after the correction.

## Invalidated Hypotheses

- The old Framework `system.data` section can remain unchanged in the migrated application configuration: invalidated by the reported inner exception and the matching source/output configuration. The outer exception at `UpgradeRequired` does not itself establish corrupt user settings.
- Treating `WcfEnabled` as an available setting in CefSharp's .NET Core build is invalid; the package source explicitly guards it with `#if !NETCOREAPP`.

## Open Hypotheses

- Configuration issue: removing the unsupported section addresses the reported parser failure. A fresh application start remains necessary to confirm runtime initialization; any subsequent error needs its own concrete exception evidence. Assumption: the existing SQLite factory names/types remain intended, so their registrations were preserved using the modern API. SQLite and broader migration acceptance checks remain unperformed.
- No remaining hypothesis for the reported CS0117 after the successful solution Build. Browser runtime behavior and the broader migration's book/camera/SQLite acceptance checks were not exercised in this targeted debug run.

## Do-Not-Retry Guidance

- Do not restore the legacy `system.data` section, suppress settings initialization errors, delete user settings/databases, or hand-edit the generated `qbookStudio.dll.config` to resolve this issue. Keep the modern startup factory registrations.
- Do not restore the `WcfEnabled` assignment or downgrade to Framework packages to resolve this error. Do not install or restore dependencies autonomously.

## Evidence / References

- Configuration issue: both source configs contained the same legacy SQLite factory section; the reported generated app config also contained it at line 30 before correction. Worker Build regenerated the app config without it.
- Modern provider registration API: https://learn.microsoft.com/en-us/dotnet/api/system.data.common.dbproviderfactories.registerfactory?view=net-10.0 . Modern runtime implementation: https://source.dot.net/System.Data.Common/System/Data/Common/DbProviderFactories.cs.html . Registration uses invariant names and assembly-qualified type strings and does not rely on the old config section.
- Original diagnostic: CS0117, `CefSharpSettings` has no definition for `WcfEnabled`, `qbook/oControls/cHtml.cs`, original line 29.
- `packages/CefSharp.Common.141.0.110/src/CefSharp/CefSharpSettings.cs`: `WcfEnabled` is enclosed in `#if !NETCOREAPP` and pertains to synchronous JavaScript object binding.
- `qbook/qbook.csproj`: references `CefSharp.Common.NETCore` and `CefSharp.WinForms.NETCore` 141.0.110 and explicitly compiles `oControls/cHtml.cs`.
- `cHtml` uses DevTools and Chromium message handling; no synchronous `JavascriptObjectRepository.Register` binding was found in this component. Assumption: the existing intent remains to operate without WCF; the removed `false` assignment needs no replacement on .NET Core.
- Active `workitem.md`: `validation: true`. Approved handoff selects Build for `qbookStudio.sln`, without Clean. Invocation-local Worker evidence confirms success; `.ai-flow/BuildReport.txt` is the authoritative diagnostic artifact.
