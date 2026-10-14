# Debug Handoff

## Current Issue / Debug Request

Beim Versuch VS Code zu starten aus qbook

## Tried Approaches

- 2026-10-14: Read-only diagnosis of the screenshot and editor launch path. No source correction attempted: the required launcher file is outside the approved implementation scope.

## Invalidated Hypotheses

- VS Code is not installed / the CLI is entirely missing: invalidated for the inspected environment. PATH resolves code.cmd, and both Code.exe and the versioned cli.js exist.

## Open Hypotheses

- Confirmed dependency: VsCodeLauncher.OpenFolder assumes resources/app/out/cli.js directly below the Code.exe installation directory. The installed code.cmd instead invokes 7baac39ec3/resources/app/out/cli.js. The fixed-path check returns false and MainForm displays the reported error.
- Return to STRUCTURE to scope qbook/VscodeLauncher.cs for installation-layout-aware CLI resolution while preserving a single dispatch and terminal exit/error handling. Do not hardcode the observed version directory. Runtime launch remains unverified.

## Do-Not-Retry Guidance

- Do not install/reinstall VS Code or alter PATH to compensate for this confirmed fixed-path assumption.
- Do not bypass the approved file scope by moving launcher resolution into MainForm.cs. No Build or editor launch was executed; no code-changing correction exists to validate.

## Evidence / References

- Screenshot: .ai-flow/attachments/9ff9e5639843/clipboard-20261014-083642.png. Title: Book editor launch failed. Message: VS Code CLI was not found. The book workspace was not opened.
- qbook/Controls/MainForm.cs: newEditorToolStripMenuItem_Click reports this message when VsCodeLauncher.OpenFolder returns false.
- qbook/VscodeLauncher.cs: OpenFolder checks the unversioned resources/app/out/cli.js path; TryFindVsCodeExe resolves the executable separately.
- Inspected installation: C:/Users/StefanFunk/AppData/Local/Programs/Microsoft VS Code. Code.exe exists; resources/app/out/cli.js does not exist; 7baac39ec3/resources/app/out/cli.js exists. bin/code.cmd explicitly uses that versioned CLI path.
- Active workitem validation is true. Approved Build: RunWorker command Build, parameter qbook/qbook.csproj, Clean false. Build was not invoked because no source correction was authorized/performed.
- Scope constraint: the archived implementation handoff does not list qbook/VscodeLauncher.cs and requires returning unlisted-file dependencies to STRUCTURE. Existing workspace modifications were preserved.
