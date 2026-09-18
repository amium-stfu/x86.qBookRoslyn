# IMPLEMENT Mode

## Purpose

`IMPLEMENT` executes the approved implementation handoff, transports the resulting changes back to the source workspace, and reports objective implementation and validation facts.

## Session Invariants

These rules apply to every `IMPLEMENT` run and take precedence over the detailed rules below.

1. `IMPLEMENT` may run only when an active implementation handoff exists.
2. Implement only the approved implementation handoff. Do not expand its scope or make new planning or architecture decisions.
3. After implementing the approved handoff, always run the workspace-defined build verification before any explicitly requested validation.
4. Execute only validation explicitly required by the approved implementation handoff. Do not infer validation requirements from existing test projects, test suites, UI tests, or other validation infrastructure.
5. Across the entire `IMPLEMENT` run, make at most one targeted correction when a required build or validation failure is caused by the current changes and the correction remains within the approved scope.
6. Do not autonomously perform dependency restore or dependency installation operations. Use only the build, restore, validation, and execution constraints defined for the active solution or workspace.
7. **Every normally ended `IMPLEMENT` run concludes with a definite `workitem.md` status update and consumption and archival of the active implementation handoff through the existing host lifecycle, regardless of outcome.** An unresolved build, validation, or dependency failure produces `status: failed`, not an open run. Only `status` depends on the outcome; both outcomes reset `planning_ready` to `false` and leave no active implementation handoff (see "Handoff Consumption and Workflow State" below).

## Preconditions and Authoritative Input

The active implementation handoff is the authoritative implementation target for the run.

If no active implementation handoff exists, do not implement.

Apply the global scope and authority constraints from `core.md`.

Do not create additional implementation work merely because repository inspection, tests, references, or discovered files suggest related improvements.

## Implementation Scope

Implement only what the approved implementation handoff requires.

Tests, validation code, or supporting implementation artifacts may change only when the approved implementation handoff explicitly requires them.

Do not use existing validation infrastructure as justification for additional implementation or validation work.

Do not create an Apply, completion, Bugfix, or other follow-up implementation handoff.

## Implementation and Validation Flow

1. Implement the approved handoff.
2. Run the workspace-defined build verification.
3. If build verification succeeds, run the explicitly required validation from the approved implementation handoff, if any.
4. If a required build or validation command fails because of the current changes and a correction is possible within the approved scope, make at most one targeted correction across the entire `IMPLEMENT` run.
5. Rerun only the failed command once after the allowed correction.
6. Continue only with any remaining validation explicitly required by the approved implementation handoff.
7. Conclude the run per "Handoff Consumption and Workflow State" — either as a successful conclusion or as a failed conclusion. Both are normal endings of the run.

Build verification is mandatory and does not need to be specified by the approved implementation handoff.

Run no automated tests when the approved implementation handoff does not explicitly request them.

Do not add broad test suites or UI tests as substitutes for the validation requested by the approved implementation handoff.

## Correction Boundary

Only one targeted correction is allowed across the entire `IMPLEMENT` run.

A correction is allowed only when:

- a required build or validation command failed,
- the failure is caused by the current changes,
- the correction remains within the approved implementation scope.

After the allowed correction, rerun only the failed command once.

If that command still fails, do not perform another correction. This is a failed conclusion of the run, not a blocked or incomplete one — conclude it per "Handoff Consumption and Workflow State".

## Dependency Restore Boundary

Do not autonomously perform dependency restore or dependency installation operations.

Use only the build, restore, validation, and execution commands and constraints defined for the active solution or workspace.

Do not bypass a non-restoring build or validation path by switching to a command that restores, installs, or otherwise fetches missing dependencies.

When build verification or an explicitly required test or validation command cannot proceed because required dependency artifacts or packages are missing:

- stop further execution and proceed to the conclusion response,
- do not start alternative validation loops or broad fallback suites,
- do not use the allowed correction attempt to change restore behavior,
- include the required dependency restore or installation notice according to the active solution or workspace rules in the short conclusion summary,
- conclude the run as a failed conclusion per "Handoff Consumption and Workflow State" — a missing dependency is a failure outcome, not a reason to leave the run unconcluded.

## Stop Conditions

End the `IMPLEMENT` run when any of the following applies:

- the approved implementation and required validation have completed successfully,
- the allowed targeted correction has been used and the failed command still fails,
- build verification still fails after the allowed correction,
- required build or validation cannot proceed because dependency artifacts or packages are missing,
- another condition prevents continuing within the approved implementation scope.

Do not start speculative fallback validation or additional correction loops after a stop condition applies.

Every stop condition leads to a conclusion under "Handoff Consumption and Workflow State" — either successful or failed. No stop condition leaves the run open or the handoff status ambiguous.

## Handoff Consumption and Workflow State

Every normally ended `IMPLEMENT` run concludes in exactly one of two ways. Both are normal endings — neither is a blocked, incomplete, or unfinished state.

In both outcomes, consume and archive the active implementation handoff through the existing host-owned artifact lifecycle and leave no active implementation handoff. Use the host-assigned artifacts; do not invent paths or perform an additional archive operation alongside host handling.

**Successful conclusion** (implementation and all required validation completed without an unresolved failure):

1. Write `status: implemented` and reset `planning_ready` to `false`.

**Failed conclusion** (a stop condition applied due to an unresolved build, validation, or dependency failure):

1. Write `status: failed` and reset `planning_ready` to `false`.
2. Report the failure as an objective fact in the result (see "Result Reporting"). Do not treat the failed conclusion as requiring the run or handoff to stay active, retried, or unresolved in `workitem.md`.

In both cases, no active implementation handoff remains until a new `PLAN` writes one; another `IMPLEMENT` requires that new `PLAN`. `DEBUG` and `REPORT` are follow-up actions under the existing host lifecycle and do not require a new `PLAN` solely because the concluded run contains errors. `DEBUG` uses the implementation handoff supplied by the host for the current plan revision, including its archived form.

These conclusion rules describe normally ended runs. They do not guarantee recovery from CLI transport failure, cancellation, or host persistence failure and do not define a separate recovery mechanism.

The next user-sent `STRUCTURE` message resumes the same shared planning chat for a fresh definition cycle. Selecting `STRUCTURE` alone does not send a transport request.

## Result Reporting

On every normally ended run, emit a short final chat summary of the changes actually performed, observed build and validation results, and checks that could not run. Explicitly state remaining errors or that the performed checks reported no errors. Never describe unexecuted checks as error-free verification.

Do not emit an Apply or completion handoff.

Do not classify the result as:

- `Complete`,
- `Incomplete`,
- `Blocked`,
- `Implementation incomplete`,
- `Further work is required`.

Report only objective facts about:

- implemented changes,
- transport results,
- build and validation results,
- known issues.

When the run reaches a failed conclusion, report the failure as an objective fact alongside everything above — this is a completed report about a failed run, not an incomplete report.

When dependency restore or installation is required, include that requirement as defined by the Dependency Restore Boundary without suppressing the short conclusion summary.

## Self-Check Before Completing IMPLEMENT

Before completing the run, verify internally:

- An active implementation handoff existed before implementation began.
- All implementation changes remained within the approved handoff.
- No new planning, architecture, or scope decisions were introduced.
- The global scope and authority constraints from `core.md` were preserved.
- Workspace-defined build verification was run after implementing the approved handoff and before explicitly requested validation.
- Only validation explicitly required by the approved implementation handoff was executed.
- Existing test projects, test suites, UI tests, or validation infrastructure were not treated as implicit validation requirements.
- No more than one targeted correction was performed across the entire `IMPLEMENT` run.
- Any correction was made only for a failure caused by the current changes and remained within scope.
- After the allowed correction, only the failed command was rerun once.
- No dependency restore or installation operation was performed autonomously.
- No non-restoring build or validation failure was bypassed with a dependency-restoring command.
- No further validation was executed after a terminal stop condition.
- **The run concluded with exactly one outcome:** either `status: implemented` or `status: failed` — never both, and never neither.
- In either outcome, the handoff is consumed and archived through the existing host lifecycle, with no active handoff left and no duplicate archive operation or invented artifact path.
- `DEBUG` can use the host-supplied handoff for the current revision, including its archived form; errors alone do not require a new `PLAN` before `DEBUG` or `REPORT`.
- `planning_ready` was reset to `false` in both outcomes.
- The final report contains only objective implementation, transport, validation, and known-issue facts, including the failure itself if the run failed.
- The short final chat summary states actual changes, observed check results, checks that could not run, and remaining errors or the absence of reported errors in performed checks; any required restore notice is included.
