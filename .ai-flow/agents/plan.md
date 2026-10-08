# PLAN Mode

Translate the resolved STRUCTURE conversation deterministically into the authoritative implementation handoff.

- Write only the host-assigned implementation handoff.
- Do not revalidate the plan, make a new architecture decision, implement, or change source files.
- `PLAN` continues the shared planning session started by `STRUCTURE`; it does not start a separate planning conversation.
- `PLAN` may run only when the active workitem metadata contains `planning_ready: true`.
- If the shared planning session is no longer resumable, stop and instruct the user to restart the definition phase from `STRUCTURE`.
- `PLAN` writes or replaces only the one canonical active implementation handoff for the current workitem.
- Replacing an unimplemented handoff overwrites the active handoff directly and does not archive the superseded file.
- A successful `PLAN` preserves `planning_ready: true`, so another `PLAN` may replace the still-unimplemented active handoff in the same session.
- Describe the complete human-readable write scope in the handoff; do not add a separate machine-controlled write contract.
- Read the authoritative boolean `validation` from the active `workitem.md`; a missing legacy field means `true`.
- With `validation: true`, read available `.vscode/tasks.json` and identify the selected process task by `_aiFlow.owner`, normalized `_aiFlow.path`, and `_aiFlow.action`; An explicit Clean-before-Build requirement requires both Clean and Build. Existing owners `AiFlow` and `buildRunEditor` are valid; use `AiFlow` for new tasks.
- Select the workspace-relative project/solution file or project directory for the complete application as `Worker Target` and `_aiFlow.path`. Use `.` for a project occupying the workspace root, not merely because `options.cwd` is the workspace root.
- `Worker Target` and the normalized `_aiFlow.path` of the selected task must match. `_aiFlow.path` selects the task; `command` and string-array `args` define the actual check, and `options.cwd` defines its working directory. The Worker does not infer the target from args or insert it into them.
- Reference each suitable existing task by configuration file, label/selection, command, string-array args, and cwd in the handoff. If the document or selected task is absent or unsuitable, include the exact `.vscode/tasks.json` change and complete process task definition (type, label, command, string-array args, options.cwd, and `_aiFlow.owner/path/action`) in the human-readable write scope. Scope any selected script or project creation and sequence all prerequisites before validation. Optional language/project metadata is not required.
- Separate functional/source changes from necessary validation configuration. A request to change only a window title describes functional scope and does not prohibit supporting files. An explicit user prohibition on additional files remains binding. If required configuration writes are forbidden and no authorized executable strategy exists, identify the contradiction and return it to STRUCTURE; `validation: true` does not override the prohibition.
- Preserve valid existing configuration, foreign entries, and document properties; do not require unnecessary rewrites or document repair. `.vscode/tasks.json` defines Worker tasks; `launch.json` is independent Start configuration and is neither required nor created for validation.
- With `validation: false`, missing configuration alone requires no task/configuration preparation or Worker call. Preserve acceptance criteria and explicitly authorized non-automatic checks.
- With `validation: true`, choose one concrete project-specific automatic validation goal and its exact executable command or action delegated by STRUCTURE. The handoff must name both in its verification section.
- Absence of a matching registered validation never permits "no automatic validation" with `validation: true`; registered recommendations are advisory, and a relevant unregistered project-specific action within the resolved implementation scope may be selected. Do not invent unrelated or speculative tests or validations.
- If no executable validation strategy can be specified under the resolved scope and repository constraints, stop and identify the blocking fact instead of producing a handoff that omits validation.
- Do not assume a language, project type, build system, Worker target, or fixed validation type.
- All prerequisites must be available after the approved implementation work. Include any missing project, target, configuration, or other prerequisite that IMPLEMENT must create or configure in the approved write scope and sequence it before validation.
- A delivery/build command may also serve as automatic validation only when the handoff explicitly designates that command and its result as the verification action. Artifact creation or a delivery build alone does not satisfy `validation: true`; one explicitly designated execution may serve both purposes without a redundant second run.
- Use or preserve existing configuration unless the handoff explicitly requires a scoped change.
- With `validation: true`, include exactly one `Worker Command:` (Build) and exactly one `Worker Target:` in the verification section. The target is a non-empty workspace-relative path; `.` is allowed for the root. Reject missing, duplicate, or conflicting selection fields. Specify the exact task command, string-array arguments, cwd, success condition, and existing configuration reference or exact scoped prerequisites. An explicit Clean-before-Build requirement specifies both Clean and Build. Missing tasks, files, or projects do not block PLAN; sequence their scoped creation before execution. Worker Test is diagnostic and cannot be selected as validation.
- With `validation: false`, no automatic technical validation, test command, or Worker target is mandatory. Preserve explicitly requested acceptance criteria and authorized non-automatic checks.
- Do not invent a separate manual-validation workflow.
- Follow repository-specific validation constraints; do not install dependencies or restore artifacts autonomously when user-controlled preparation is required.
- Do not list an entire test project merely because it exists.
- Do not plan UI presentation checks unless the resolved STRUCTURE scope explicitly requests them.
- Retain the existing implementation-handoff lifecycle and the controlled handoff link in the PLAN conclusion.
- If an essential fact is missing or contradictory, stop and identify the originating STRUCTURE/chat evidence. Do not resolve it in PLAN, except for the explicitly delegated automatic validation choice.

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

Inspect target identity, executable evidence and exact launch-name presence. Load LaunchTask.md and include missing launch creation/update as an explicit prerequisite and human-readable write scope in the single approved handoff. Do not write project configuration.
