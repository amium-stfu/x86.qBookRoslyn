# Debug Handoff

## Current Issue / Debug Request

Beim manuellen Buildversuch bekomme ich:Severity	Code	Description	Project	File	Line	Suppression State	Details
Error		BuildRevision must be supplied for a Release build.	qbook	B:\x86.qBookRoslyn\qbook\qbook.csproj	1129

## Tried Approaches

- Verified the Release gate with `BuildRevision=1`. The original missing-property error is intentionally raised by `qbook/qbook.csproj` when no numeric release revision is supplied.
- Ran `dotnet build qbookStudio.sln --no-restore -c Release -p:Platform=x86 -p:BuildRevision=1`. The build passed the `BuildRevision` gate but failed in the existing `qbookCsScript` reference because CefSharp reports `PlatformTarget` `AnyCPU`.

## Invalidated Hypotheses

- The Release error is not caused by a missing default or a defect in the FOSS gate. Adding a local default would violate the approved requirement that a Release build require a pipeline-provided `BuildRevision`.

## Open Hypotheses

- The solution configuration maps `qbookCsScript` to `Release|Any CPU` while its CefSharp targets require `x86` or `x64`. This is outside the approved FOSS-versioning scope.

## Do-Not-Retry Guidance

- Do not add a timestamp, counter, or local default for `BuildRevision`; Release must continue to reject an absent value.

## Evidence / References

- `qbook/qbook.csproj`, target `ValidateFossState`, explicitly rejects an empty `$(BuildRevision)` for Release.
- The build verification with `BuildRevision=1` no longer emitted the original error. It instead failed with `CefSharp.Common is unable to proceeed as your current PlatformTarget is 'AnyCPU'` for `qbookCsScript.csproj`.

## Debug Run 2026-09-14: Visual Studio Release Revision Request

### Requested change

- Run the Release FOSS preflight through the Visual Studio pre-build event and generate `BuildRevision` automatically for manual Visual Studio builds.

### Confirmed analysis

- `ValidateFossState` runs before `GenerateAssemblyVersionInfo`, and `GenerateAssemblyVersionInfo` runs before `CoreCompile`. This is required because the generated assembly version source must be available before compilation.
- A Visual Studio `PreBuildEvent` runs after compilation in the MSBuild build sequence and therefore cannot provide the generated assembly version source for the current build.
- The implementation handoff explicitly requires a pipeline-provided numeric `BuildRevision` and explicitly lists an absent `BuildRevision` as a Release failure. Automatic local generation would change that approved Release-versioning contract.

### Outcome

- No source change was made and no build verification was run because the requested behavior conflicts with the approved implementation handoff.

### Next-best path

- A new planning decision is required to define an approved Visual Studio revision source and its uniqueness, persistence, concurrency, reproducibility, and Release-pipeline behavior. The FOSS preflight can remain a `BeforeTargets="CoreCompile"` MSBuild target rather than a `PreBuildEvent`.
