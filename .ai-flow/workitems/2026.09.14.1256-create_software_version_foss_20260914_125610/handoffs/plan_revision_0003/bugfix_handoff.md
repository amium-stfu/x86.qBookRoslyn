# Debug Handoff

## Current Issue / Debug Request

Der Buildvorgang wurde am 18.09.2026 09:35:55 gestartet.

     1>Projekt "B:\x86.qBookRoslyn\qbookStudio.sln" auf Knoten "1", Rebuild Ziel(e).
     1>B:\x86.qBookRoslyn\qbookStudio.sln.metaproj : error MSB4126: Die angegebene Projektmappenkonfiguration "Release|
       AnyCPU" ist ungültig. Geben Sie mithilfe der Konfigurations- und Plattformeigenschaften eine gültige Projektmapp
       enkonfiguration an (z. B. MSBuild.exe Solution.sln /p:Configuration=Debug /p:Platform="Any CPU"), oder lassen Si
       e diese Eigenschaften leer, sodass die Standardprojektmappenkonfiguration verwendet wird. [B:\x86.qBookRoslyn\qb
       ookStudio.sln]
     1>Die Erstellung des Projekts "B:\x86.qBookRoslyn\qbookStudio.sln" ist abgeschlossen, Rebuild Ziel(e) -- FEHLER.

Fehler beim Buildvorgang.

       "B:\x86.qBookRoslyn\qbookStudio.sln" (Rebuild Ziel) (1) ->
       (ValidateSolutionConfiguration Ziel) ->
         B:\x86.qBookRoslyn\qbookStudio.sln.metaproj : error MSB4126: Die angegebene Projektmappenkonfiguration "Releas
       e|AnyCPU" ist ungültig. Geben Sie mithilfe der Konfigurations- und Plattformeigenschaften eine gültige Projektma
       ppenkonfiguration an (z. B. MSBuild.exe Solution.sln /p:Configuration=Debug /p:Platform="Any CPU"), oder lassen
       Sie diese Eigenschaften leer, sodass die Standardprojektmappenkonfiguration verwendet wird. [B:\x86.qBookRoslyn\
       qbookStudio.sln]

    0 Warnung(en)
    1 Fehler

Verstrichene Zeit 00:00:00.05
qbook Release build failed.
In B:\x86.qBookRoslyn\qbook\release.ps1:24 Zeichen:28
+ if ($LASTEXITCODE -ne 0) { throw "qbook Release build failed." }
+                            ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : OperationStopped: (qbook Release build failed.:String) [], RuntimeException
    + FullyQualifiedErrorId : qbook Release build failed.

## Tried Approaches

- Attempt 1: Changed `qbook/release.ps1` to pass the existing solution platform as `Any CPU` instead of `AnyCPU`. `dotnet build qbookStudio.sln --no-restore --configuration Release -p:Platform=Any CPU -p:BuildRevision=1` no longer reported MSB4126, but failed because CefSharp rejected the `AnyCPU` platform target in `qbookCsScript`.
- Attempt 2: Changed `qbook/release.ps1` to invoke the existing `Release|x86` solution configuration. `dotnet build qbookStudio.sln --no-restore --configuration Release -p:Platform=x86 -p:BuildRevision=1` again did not report MSB4126, but `qbookCsScript` still supplied `PlatformTarget=AnyCPU` to CefSharp.

## Invalidated Hypotheses

- Passing `AnyCPU` without the space was the cause of the reported invalid solution configuration.
- Selecting the solution `x86` platform alone does not set `qbookCsScript`'s CefSharp `PlatformTarget` to `x86`.

## Open Hypotheses

- `qbookCsScript.csproj` or an imported project property independently fixes `PlatformTarget` to `AnyCPU`; that project configuration must be aligned with the selected Release platform before the complete Release build can succeed.

## Do-Not-Retry Guidance

- Do not retry the Release build with `AnyCPU`; CefSharp explicitly rejects it.
- Do not retry only the solution platform switch to `x86`; the project-level `PlatformTarget` remains `AnyCPU`.

## Evidence / References

- `qbookStudio.sln` declares `Release|Any CPU` and `Release|x86`; it does not declare `Release|AnyCPU`.
- Both required no-restore build verifications reached project builds without MSB4126. Both failed with `CefSharp.Common.targets(373,5)` reporting `PlatformTarget` as `AnyCPU` for `qbookCsScript`.

## Debug Run 2026-09-18: Release x86 Compiler and Resource Settings

### Current Issue / Debug Request

The PowerShell Release build reached `qbook` compilation with `Release|x86` and failed with `CS8370` because nullable reference types require C# 8.0 or later. Visual Studio Debug builds succeed.

### Correction Attempts And Outcomes

- Attempt 1: Changed `qbook/qbook.csproj` so that `Release|x86` uses `LangVersion` `9.0` instead of `7.3`. Changed the `qbookCsScript` `Release|AnyCPU` block to set `PlatformTarget` to `x86`, matching the selected Release architecture and CefSharp requirement. `dotnet build qbookStudio.sln --no-restore --configuration Release -p:Platform=x86 -p:BuildRevision=1` no longer reported `CS8370` or the CefSharp `AnyCPU` error, but failed with `MSB3823` for non-string `qbookCsScript` resources.
- Attempt 2: Added `GenerateResourceUsePreserializedResources` with value `true` to the same `qbookCsScript` Release block. The project already references `System.Resources.Extensions`, so this required no dependency change. The required no-restore build no longer reported `MSB3823`, but failed with `MSB4803`: the .NET Core MSBuild used by `dotnet build` cannot execute the legacy `LC` task.

### Confirmed Findings

- `Release|x86` in `qbook.csproj` previously overwrote the project-level C# 9.0 setting with C# 7.3, directly causing the reported nullable-reference compilation errors.
- The qbook Release path now supplies C# 9.0 and an explicit x86 target to the affected Release project configurations. Debug configurations were not changed.
- The current required `dotnet build --no-restore` verification is blocked by the legacy license-compiler task in `qbookCsScript`, not by the corrected C# version, CefSharp target, or resource serialization setting.

### Do-Not-Retry Guidance

- Do not repeat the identical `dotnet build --no-restore` validation expecting `LC` to succeed; .NET Core MSBuild does not support that task.
- Do not remove or disable the license compilation task solely to make the .NET Core validation pass; this would exceed the approved Release-workflow scope.

### Assumption And Next-Best Hypothesis

- `qbook/release.ps1` resolves and invokes the .NET Framework MSBuild supplied by Visual Studio Build Tools, which is the toolchain required by `LC`; that invocation was not run in this Debug session because the workspace mandates `dotnet build --no-restore` as build verification and exposes no host validation action.

## Debug Run 2026-09-18: Release Version Automation Explanation

### Current Debug Request

The user confirmed that the Release build now runs and asked whether the build version can be automated, and what the final `0` in `v26.1.9.0` represents.

### Confirmed Findings

- The checked generated assembly metadata is `AssemblyVersion("26.1.9.0")`, `AssemblyFileVersion("26.1.9.0")`, and `AssemblyInformationalVersion("26.01.00009+ce179ff")`.
- For Release builds, `updateAssemblyInfo.cs.ps1` generates the numeric version as `yy.FossRevision.BuildRevision.0`.
- `26` is the UTC two-digit year, `1` is the approved `FossRevision` from `compliance/foss/approved-state.json`, and `9` is `BuildRevision`.
- The GitHub workflow passes `${{ github.run_number }}` as `BuildRevision`; therefore the third component is already automated and rises with each workflow run.
- The final `0` is deliberately a constant fourth .NET assembly-version component. It is not computed from a date, a Git commit, or a local counter. The separate informational version carries the padded release identity and the short commit ID.

### Measures And Validation

- No code change and no build verification were performed for this explanatory request.
- The user reported that the Release build now runs; that runtime result was not independently executed in this Debug session.

### Remaining Problems And Assumptions

- The prior `dotnet build --no-restore` verification limitation remains documented: the .NET Core MSBuild cannot execute the legacy `LC` task. No new validation was run here.
- The explanation assumes the observed build was started by the GitHub Release workflow, whose `github.run_number` becomes `BuildRevision`.

## Debug Run 2026-09-18: Local BuildRevision Prompt and Requested Timestamp Revision

### Current Debug Request

The user reported that a direct invocation of `release.ps1` prompted for `BuildRevision` and asked whether the fixed final `0` of the assembly version could be replaced with a local `MMddHHmm` timestamp.

### Confirmed Findings

- `qbook/release.ps1` intentionally declares `BuildRevision` as mandatory. A direct local invocation without `-BuildRevision` therefore prompts before the script runs.
- `.github/workflows/release-qbook.yml` calls `release.ps1 -BuildRevision "${{ github.run_number }}"`; the official manually dispatched Release workflow does not prompt and already automates the third version component.
- The approved implementation contract requires the numeric assembly/file version `yy.FossRevision.BuildRevision.0` and expressly prohibits deriving a revision from local time.
- A decimal `MMddHHmm` value can exceed the .NET assembly-version component limit of `65535`; it cannot safely replace the fourth component as requested.

### Measures And Validation

- Inspected `qbook/release.ps1`, `qbook/updateAssemblyInfo.cs.ps1`, and `.github/workflows/release-qbook.yml`.
- No code was changed and no build verification was run, because the requested timestamp-based versioning conflicts with the approved handoff.

### Remaining Problems And Assumptions

- A local direct Release build still requires an explicit numeric `BuildRevision`; that behavior is intentional under the approved release workflow.
- A timestamp-based revision would require a revised approved handoff and a version format that satisfies .NET component limits.
- The existing `dotnet build --no-restore` limitation remains: the .NET Core MSBuild cannot execute the legacy `LC` task in `qbookCsScript`.

## Debug Run 2026-09-18: Manual GitHub Release Process and Fourth Version Component

### Current Debug Request

The user asked how to create a Release correctly in connection with Git and whether the fixed final `0` in the numeric assembly version should instead be a consecutive number or be omitted when `github.run_number` is sufficient.

### Confirmed Findings

- `.github/workflows/release-qbook.yml` is manually triggered only, checks out the selected ref, restores legacy packages, invokes `qbook/release.ps1` with `github.run_number`, and uploads `Setup/Release/bin/qbook` only after success.
- The approved FOSS state must already be committed and merged before starting the workflow. The workflow itself has read-only repository permissions and cannot approve or modify FOSS state.
- The generated numeric assembly/file version is `yy.FossRevision.BuildRevision.0`. `github.run_number` is an automatically increasing workflow run number and is the intended consecutive build component.
- The fixed fourth `0` is required by the approved version format but does not need a separate counter. It is not a meaningful release identity because the first three components already identify the year, approved FOSS revision, and workflow run.

### Measures And Validation

- Inspected the committed Release script, generated-version script, workflow, and approved FOSS baseline.
- No code change and no build verification were performed; the request was explanatory and the user reported the Release build already runs.

### Remaining Problems And Assumptions

- An official Release remains blocked if the selected Git ref does not include a valid, merged FOSS baseline matching its dependencies.
- GitHub Actions run numbers are repository-wide and can have gaps; they are still monotonically increasing and acceptable as build identifiers.
- The existing `dotnet build --no-restore` limitation remains: the .NET Core MSBuild cannot execute the legacy `LC` task in `qbookCsScript`.

## Debug Run 2026-09-18: GitHub Actions MSBuild Discovery

### Current Issue / Debug Request

The manually dispatched GitHub Actions Release workflow failed in `qbook/release.ps1` before the build started. The script reported `MSBuild.exe was not found in Visual Studio Build Tools.`

### Correction Attempt And Outcome

- Replaced the `vswhere` query that required `Microsoft.Component.MSBuild` and searched with a wildcard with a query for the selected Visual Studio installation path. The script now resolves `MSBuild.exe` from the stable `MSBuild\\Current\\Bin` path, with `MSBuild\\17.0\\Bin` as a compatibility fallback.
- The correction remains limited to the approved Release entry script and continues to invoke Visual Studio's .NET Framework MSBuild. It does not alter FOSS validation, version generation, workflow permissions, or dependency restore behavior.

### Validation

- Ran the required `dotnet build qbookStudio.sln --no-restore --configuration Release -p:Platform=x86 -p:BuildRevision=1` verification.
- The build reached the Release projects and generated the Release assembly version information, but failed with `MSB4803`: .NET Core MSBuild cannot run the legacy `ResolveComReference` task. This is not the Visual Studio MSBuild that the Release script selects on the GitHub Windows runner.
- No external validation action was available, and no restore was run.

### Remaining Problems And Assumptions

- The corrected MSBuild discovery has not been executed on a GitHub-hosted Windows runner in this DEBUG run; the next manually dispatched workflow run is required to confirm it.
- The local `dotnet build --no-restore` path remains unsuitable for validating this legacy .NET Framework Release build because it cannot execute `ResolveComReference`.

## Debug Run 2026-09-18: Visual Studio Prerelease Discovery

### Current Issue / Debug Request

The manually dispatched GitHub Actions Release workflow failed in `qbook/release.ps1` before the build started. The `vswhere` query returned no installation path for the Visual Studio 18 prerelease installation on the Windows runner, and the script reported `A Visual Studio installation was not found.`

### Correction Attempt And Outcome

- Added `-prerelease` to the existing `vswhere` query in `qbook/release.ps1`.
- The query retains `-latest`, `-products *`, and the existing installation-path and MSBuild-path validation, while allowing the runner's prerelease Visual Studio installation to be selected.
- PowerShell parsing reported no syntax errors.

### Validation

- Ran the required `dotnet build qbookStudio.sln --no-restore --configuration Release -p:Platform=x86 -p:BuildRevision=1` verification.
- The build reached the Release projects and built `qbookCsScript`, but failed in `qbook` with `MSB4803`: .NET Core MSBuild cannot run the legacy `ResolveComReference` task.
- The failure is the previously documented local validation limitation and does not exercise the Visual Studio discovery performed by `release.ps1`.
- No trusted external validation action was available, and no restore was run.

### Remaining Problems And Assumptions

- The corrected prerelease discovery has not been executed on a GitHub-hosted Windows runner in this DEBUG run; the next manually dispatched workflow run is required to confirm that the installed Visual Studio 18 instance and its `MSBuild\Current\Bin\MSBuild.exe` are selected.
- The correction assumes the runner installation is registered with Visual Studio Installer and therefore discoverable by `vswhere -prerelease`.
- The local `dotnet build --no-restore` path remains unsuitable for validating this legacy .NET Framework Release build because it cannot execute `ResolveComReference`.
