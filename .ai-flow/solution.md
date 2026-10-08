## Build and Validation

- Use the solution-defined build and test commands.
- Build verification for this legacy .NET Framework solution must use Visual Studio MSBuild, not `dotnet build`.
- Build verification must use `msbuild qbookStudio.sln /t:Build /p:Configuration=Release /p:Platform=x86`.
- Do not use `dotnet build` for build verification, because its .NET Core MSBuild path can select an incompatible AnyCPU target and does not support all legacy MSBuild tasks.
- A failure in legacy MSBuild tasks, such as `ResolveComReference` or `LC`, when using a non-Visual-Studio build path is an environment limitation and is not by itself a project failure.
- Test verification must use `dotnet test` with `--no-restore`.
- Do not replace `--no-restore` with `--no-build`.
- Do not run `dotnet restore` autonomously.
- Do not retry a failed `--no-restore` test by switching to a restore-enabled command.
- If test execution cannot proceed because NuGet packages or restore artifacts are missing, stop and report that `dotnet restore` is required.
- Restore is a user- or host-controlled operation and is outside the normal autonomous agent execution path.
