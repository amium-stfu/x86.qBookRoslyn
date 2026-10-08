# Debug Handoff

## Current Issue / Debug Request

Release state update failed.
In B:\x86.qBookRoslyn\qbook\release.ps1:11 Zeichen:28
+ if ($LASTEXITCODE -ne 0) { throw "Release state update failed." }
+                            ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : OperationStopped: (Release state update failed.:String) [], RuntimeException
    + FullyQualifiedErrorId : Release state update failed.

## Tried Approaches

- Corrected package-folder extraction in `qbook/updateFossState.ps1`. The previous `Split("\\\\")` looked for two adjacent backslashes, so the lookup key contained the remaining path (for example `AForge.2.2.5\\lib\\AForge.dll`) and could not match a declared package.
- Corrected `qbook/release.ps1` to use `$?` after invoking PowerShell scripts. `$LASTEXITCODE` is only reliable for native executables and was null after a successful script invocation, causing the false `Release state update failed.` error.

## Invalidated Hypotheses

- Missing restored packages were not the cause: the referenced AForge package was present and declared by both projects.
- A failed FOSS snapshot was not the cause: the state-update script completed and persisted the updated revision state before the release wrapper failed.

## Open Hypotheses

- None.

## Correction Attempt 1: Release Archive and Indented State

- Changed `qbook/updateFossState.ps1` to persist `NugetRevision.json` with indented JSON instead of compressed JSON. The canonical manifest remains compressed for deterministic fingerprint calculation.
- Added `qbook/packageRelease.ps1`. After a successful Release build it copies `NugetRevision.json` and the generated `BuildInfo.md` to `Setup`, and uses the root `7z.exe`/`7z.dll` to create `Setup/qbook_<Major.Feature.Foss.yyDDD>.zip` from `Setup/Release/bin/qbook`.
- Updated `qbook/release.ps1` to invoke packaging after `BuildInfo.md` generation and added the script to `qbook/qbook.csproj`.

## Validation Results

- Visual Studio MSBuild verification completed successfully with `0 Error(s)` and `575 Warning(s)` using `qbookStudio.sln /t:Build /p:Configuration=Release /p:Platform=x86`.
- `qbook/packageRelease.ps1` passed PowerShell parser validation.
- `NugetRevision.json` parsed successfully and begins with an opening brace on its own line, confirming indented JSON output.
- No full `qbook/release.ps1` run was performed because it increments `BuildId`; the approved workflow reserves this release execution and output inspection for the user.

## Assumptions

- `7z.exe` and `7z.dll` in the repository root are the intended 7-Zip command-line pair; these are the actual root file names.
- The archive contains the contents of `Setup/Release/bin/qbook`, including its `BuildInfo.md`. The additional copies of `NugetRevision.json` and `BuildInfo.md` are placed directly in `Setup` as requested.

## Do-Not-Retry Guidance

- Do not use `$LASTEXITCODE` to evaluate the result of an invoked PowerShell script; use `$?` for script invocation status.

## Evidence / References

- Initial isolated state update: `Referenced package file does not belong to a declared package: ../packages/AForge.2.2.5/lib/AForge.dll`.
- Both mandatory Visual Studio MSBuild verifications completed with exit code 0 and 0 errors (575 existing warnings).
- The final `qbook/release.ps1` execution completed with exit code 0. It updated state to `FossRevision 2` / `BuildId 4`, generated Release version `1.1.2.26266` in both qbook assemblies, and created `Setup\\Release\\bin\\qbook\\BuildInfo.md`.
- Confirmed release behavior: `qbook/release.ps1` invokes `updateFossState.ps1` before every Release build. The script reconstructs and hashes the FOSS component snapshot on each invocation; `FossRevision` changes only when the resulting fingerprint differs, while `BuildId` increments on every successful state-update invocation.
