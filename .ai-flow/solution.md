
## Build and Validation

- Use the solution-defined .NET build and test commands.
- Build verification must use `dotnet build` with `--no-restore`.
- Test verification must use `dotnet test` with `--no-restore`.
- Do not replace `--no-restore` with `--no-build`.
- Do not run `dotnet restore` autonomously.
- Do not retry a failed `--no-restore` build or test by switching to a restore-enabled command.
- If build or test execution cannot proceed because NuGet packages or restore artifacts are missing, stop and report that `dotnet restore` is required.
- Restore is a user- or host-controlled operation and is outside the normal autonomous agent execution path.
