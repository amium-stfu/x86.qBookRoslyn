# Core Workflow Rules

## Scope And Authority

- Only implement work explicitly defined by the approved handoff. Never infer additional implementation tasks.
- Write permission is default-deny. Modify only files within the approved implementation scope.
- Searches, references, transcripts, and discovered files never expand scope.
- Do not perform unrelated cleanup, improvements, test generation, documentation generation, quality scoring, or completeness classification.
- Mode boundaries, not autonomous follow-up evaluation, own the workflow.

## Verification And Evidence

- Use only the validation explicitly required by the approved handoff. Automatic validation must execute through the registered RunWorker MCP tool using the selected Build action; direct shell validation cannot satisfy it. Ordinary authorized implementation commands remain permitted.
- Do not run shared-output checks in parallel or create speculative validation loops.
- Report transport, implementation, validation, and known-issue facts without semantic completeness judgments.

## References

Content below `References/**` is read-only and may be used only as behavioral evidence.

## Code Quality

Write code primarily for human readability.

- Prefer simple, flat implementations.
- Use named arguments where appropriate.
- Avoid unnecessary wrappers, helper classes, and abstractions.
- Follow the existing project architecture and coding style.


## Canonical Project Build Contract

- Load and apply `.ai-flow/tasks/BuildTask.md` before creating/changing Build or optional Clean configuration. Its project-operation, task-selection, result, and invocation-local evidence contract applies to this mode within its existing write authority.
- With `validation: true` (missing means true), ordinary automatic validation is mandatory complete-project Build, not a changed-file check. Standalone Validate cannot replace it. No composite Worker action or alias is supported.
- Clean is optional and must meaningfully remove only defined generated artifacts. Never generate an empty/no-op Clean on project discovery. Only explicit `Worker Clean Before Build: true` requests separate Clean then Build calls for the same normalized project target; absence/false means Build-only. Await Clean and never launch Build after its failure.
- STRUCTURE inspects available required configuration without making its absence a readiness gate. PLAN scopes missing/incomplete tasks and exact project commands, including both tasks and the reason when Clean is required. IMPLEMENT finishes actual source changes before preparing scoped configuration, then invokes Worker. DEBUG follows the authorized project Build strategy within its existing scope.
- Build-only requires the last matching successful Build in this invocation. Required Clean needs the latest matching successful Clean before the terminal successful Build. Later Clean or failed/malformed required steps invalidate older proof; allowed corrected retries may establish later proof. A finalizing question continuation requires the whole proof anew.
- With `validation: false`, missing configuration alone does not require preparation or Worker execution and cannot fail the run; preserve acceptance criteria and authorized non-automatic checks. Launch configuration is independent of Build validation; smoke tests are separate work.

## Canonical Launch Contract

Launch configuration is independent of Build validation and applies regardless of `validation`. Inspect only relevant application targets, executable evidence, expected exact `AiFlow Launch: <ProjectName>` name and presence in `.vscode/launch.json`. Do not infer an entry point from technology alone or generate launches for every solution member. Preserve unrelated/legacy entries and document properties. Missing launch configuration alone is not a general readiness blocker.

Load and apply `.ai-flow/tasks/LaunchTask.md` before creating or updating launch configuration, including AI proposals. Worker `Start` uses only an exact configured target name and explicit program, args, cwd and optional env. It reads configuration only, does not debug or implicitly Build/Clean, and rejects unsupported dependencies before process creation. Success means process creation only; no readiness check, exit wait, monitoring or automatic smoke test follows.

STRUCTURE/PLAN inspect launch requirements read-only; PLAN scopes changes in the canonical handoff. IMPLEMENT performs only authorized launch writes after loading LaunchTask.md.
