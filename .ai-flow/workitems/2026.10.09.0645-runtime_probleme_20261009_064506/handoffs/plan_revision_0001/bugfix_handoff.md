# Debug Handoff

## Current Issue / Debug Request

Current request: CS1579 at qbook/MruFilesManager.cs:122. The foreach source expression items.Reverse() is reported as void. The preceding CS8370 request and its correction are retained below as cumulative history.

Severity	Code	Description	Project	File	Line	Suppression State	Details
Error (active)	CS8370	Feature 'nullable reference types' is not available in C# 7.3. Please use language version 8.0 or greater.	qbook	B:\x86.qBookRoslyn\qbook\CsprojGenerator.cs	33	Gerne aktuelle Language

## Tried Approaches

- Attempt 1: Set qbook/qbook.csproj language version to latest and remove the Debug and HTML5 C# 7.3 overrides. Release also explicitly uses latest. Preserve existing x64 migration changes and all application source files.
- RunWorker(command=Build, parameter=qbookStudio.sln) executed the approved Visual Studio MSBuild Release/x64 task. WorkerResponse.Success=false; embedded success=false; exitCode=1; report=.ai-flow/BuildReport.txt. The full report contains 573 warnings and one error, CS1579 at qbook/MruFilesManager.cs:122. No CS8370, MSB3270, CS8012, or MSB3245 appears.
- Validation did not succeed. No second correction or Build was attempted because the remaining source correction is outside the approved write scope. No runtime or PicoScope verification was performed.
- Follow-up CS1579 diagnosis: inspected SetMruByCsvString. csv.Split returns string[]; line 122 uses items.Reverse(), and Add inserts each entry at index zero. Explicit System.Linq.Enumerable.Reverse(items) is the proposed one-line correction to retain reverse enumeration and the existing MRU insertion order without reversing the array in place. This proposal has not been applied or compiled.
- The current archived Implementation Handoff still excludes application source edits, and the new request supplies no expanded PLAN scope. No additional correction attempt or Worker invocation was performed. Only this Debug Handoff was updated; existing source/configuration changes and diagnostic artifacts were preserved.

## Invalidated Hypotheses

- qbook already uses C# 9.0 in every configuration: false. Debug and HTML5 explicitly selected C# 7.3 before this correction.
- Changing only the language setting is sufficient for a successful latest-language solution Build: false; CS1579 remains.

## Open Hypotheses

- The latest compiler may bind items.Reverse() on the string array to a void-returning span extension instead of the intended LINQ enumerable Reverse. The exact binding has not been independently verified. A new PLAN should authorize a targeted MruFilesManager.cs correction that explicitly selects enumerable reversal, followed by the same full Worker Build.
- Local evidence supports the span-extension hypothesis: qbook references System.Memory.4.6.3, and that package documents System.MemoryExtensions.Reverse<T>(Span<T>). The diagnostic confirms a void-valued call, but does not identify the selected method. No independent semantic binding inspection was performed.
- latest means the latest stable language supported by the installed build compiler; no numeric language version or nullable analysis policy is forced. Debug and HTML5 were not separately built, as extra configurations are excluded.

## Do-Not-Retry Guidance

- Do not edit MruFilesManager.cs under the current handoff: application source edits are explicitly excluded. Obtain an expanded PLAN scope first. Do not repeat unchanged Builds, downgrade the requested latest language, restore packages, or run direct-process validation.

## Evidence / References

- qbook/CsprojGenerator.cs:33 uses IEnumerable<string>? and requires C# 8.0 or later.
- qbook/qbook.csproj:17 and its Release group select latest; Debug and HTML5 now inherit the general setting.
- .vscode/tasks.json already matches the approved process task, owner AiFlow, target qbookStudio.sln, Build action, exact executable, Release/x64 arguments, and workspace cwd; unchanged.
- .ai-flow/BuildReport.txt: MSBuild 18.9.1, Release|x64, qbook compiler argument /langversion:latest; sole error CS1579 at qbook/MruFilesManager.cs:122, foreach over items.Reverse() where the selected call returns void.
- workitem.md has validation: true. Existing user changes were preserved. Build report/result files were not manually rewritten.
- Follow-up inspection confirms that the existing BuildReport still reports 573 warnings and one CS1579 error; BuildResult.json records success=false and exitCode=1. These are prior-run diagnostic artifacts, not invocation-local validation proof. No CS8370, MSB3270, CS8012, or MSB3245 occurs in that report.
- Required next scope: authorize only the foreach enumerable expression change in qbook/MruFilesManager.cs, retain latest and the existing x64 configuration, then validate using RunWorker(command=Build, parameter=qbookStudio.sln), without Clean, restore, runtime tests, or PicoScope work.
