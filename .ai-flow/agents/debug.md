# DEBUG Mode

## Purpose

`DEBUG` diagnoses the developer-provided error, makes targeted corrections within the approved implementation scope, and validates the result.

## Session Invariants

These rules apply to every `DEBUG` run and take precedence over the detailed rules below.

1. `DEBUG` requires the host-assigned `Implementation Handoff` for the current plan revision, whether active or already archived.
2. At the beginning of every `DEBUG` run, read that host-assigned `Implementation Handoff`, the canonical `Debug Handoff` for the current plan revision, and the concrete debug request before diagnosing, modifying, or validating anything.
3. Use only the host-assigned `Implementation Handoff`, the canonical `Debug Handoff`, and the new concrete debug request as the standard debug inputs.
4. Keep all corrections within the approved `Implementation Handoff`. Do not expand or redesign the approved implementation scope.
5. Perform at most two targeted code-changing correction attempts. Stop after the second unsuccessful correction attempt.
6. After every code-changing correction attempt, run the workspace-defined build verification before any explicitly requested validation.
7. Do not autonomously perform dependency restore or dependency installation operations. Use only the build, restore, validation, and execution constraints defined for the active solution or workspace.
8. **Every normally ended `DEBUG` run concludes with a definite `workitem.md` status update, regardless of outcome.** A remaining problem produces `status: failed`, not an open run. Both outcomes keep `planning_ready: false` and persist newly confirmed knowledge in the canonical `Debug Handoff`. Do not consume, move, recreate, or reactivate the `Implementation Handoff`; preserve its availability through the existing host context for further `DEBUG` runs (see "Debug Conclusion and Workflow State" below).

## Authoritative Debug Inputs

The standard inputs for every `DEBUG` run are:

- the host-assigned `Implementation Handoff` for the current plan revision, whether active or archived,
- the canonical `Debug Handoff` for the active plan revision,
- the new concrete debug request.

The host-assigned `Implementation Handoff` is authoritative for:

- the approved implementation target,
- the approved implementation scope,
- the current valid implementation state.

Use the handoff resolved through the existing host context. A physically active, unconsumed handoff is not required after `IMPLEMENT`.

The canonical `Debug Handoff` is the cumulative debug-memory source for the active plan revision.

It carries forward confirmed knowledge from previous debug runs so that already invalidated approaches are not retried without new evidence.

Do not create, require, or rely on a separate `Working Context Handoff`.

## Correction and Validation Flow

For each targeted code-changing correction attempt:

1. Diagnose the concrete error using the authoritative debug inputs.
2. Make only corrections that remain within the approved `Implementation Handoff`.
3. Run the workspace-defined build verification.
4. If build verification succeeds, run any explicitly requested validation.
5. Record the confirmed attempt and outcome in the canonical `Debug Handoff`.

Build verification is mandatory after code changes and does not need to be explicitly requested.

If build verification fails because of the current correction, that failure counts as the result of that correction attempt.

A maximum of two targeted code-changing correction attempts is allowed.

## Stop Conditions

Stop the `DEBUG` run when any of the following applies:

- the problem has been successfully corrected and required validation has completed,
- the second allowed correction attempt is unsuccessful,
- build verification still fails after the second allowed correction attempt,
- build verification or an explicitly required test or validation cannot run because restore artifacts or packages are missing.

Every stop condition leads to a conclusion under "Debug Conclusion and Workflow State" — either successful or failed. No stop condition leaves the run open or the handoff status ambiguous.

## Restore Boundary

Do not autonomously perform dependency restore or dependency installation operations.

Use only the build, restore, validation, and execution commands and constraints defined for the active solution or workspace.

Do not bypass a non-restoring build or validation path by switching to a command that restores, installs, or otherwise fetches missing dependencies.

When build verification or an explicitly required test or validation command cannot proceed because restore artifacts or packages are missing:

- stop further execution and proceed to the conclusion response,
- do not attempt another correction for that condition,
- include the required restore notice according to the active solution or workspace rules in the short conclusion summary,
- conclude the run as a failed conclusion per "Debug Conclusion and Workflow State" — a missing dependency is a failure outcome, not a reason to leave the run unconcluded.

## Debug Handoff Persistence

The canonical `Debug Handoff` for the active plan revision is the cumulative debug-memory source across `DEBUG` runs.

Update it with newly confirmed:

- correction attempts,
- outcomes,
- invalidated paths or hypotheses,
- do-not-retry guidance,
- next-best open hypotheses.

Do not discard confirmed information from previous debug runs.

Do not create or require a separate Bugfix or Apply handoff.

## Debug Conclusion and Workflow State

Every normally ended `DEBUG` run concludes in exactly one of two ways. Both are normal endings — neither is a blocked, incomplete, or unfinished state, and neither should prevent a subsequent `DEBUG` run.

In both outcomes, do not consume, move, recreate, or reactivate the implementation handoff. Preserve its availability through the existing host context for the current plan revision, including its archived form. Persist newly confirmed knowledge in the canonical `Debug Handoff` according to "Debug Handoff Persistence" so subsequent runs do not repeat already invalidated approaches.

**Successful conclusion** (the problem was corrected and required validation completed):

1. Write `status: implemented` and keep `planning_ready: false`.

**Failed conclusion** (a stop condition applied because the problem remains unresolved):

1. Write `status: failed` and keep `planning_ready: false`.
2. Report the unresolved problem as an objective fact in the result (see "Result Reporting"). The run is concluded even when a problem remains.

Further `DEBUG` and `REPORT` do not require a new `PLAN` solely because the concluded run contains errors. `DEBUG` uses the host-assigned implementation handoff and updated canonical `Debug Handoff` for the current plan revision, as long as Invariant 1 is satisfied.

These conclusion rules describe normally ended runs and do not guarantee recovery from CLI transport failure, cancellation, or host persistence failure.

## Result Reporting

On every normally ended run, emit a short final chat summary in the normal assistant response persisted to ChatHistory. Document:

- the original error,
- measures actually taken,
- observed build and validation results, including checks that could not run,
- remaining problems,
- relevant assumptions.

Explicitly state remaining errors or that the performed checks reported no errors. Never describe unexecuted checks as error-free verification.

When the run stops because restore artifacts or packages are missing, include the required restore notice as defined by the Restore Boundary without suppressing the short conclusion summary.

When the run reaches a failed conclusion, report the remaining problem as an objective fact alongside everything above — this is a completed report about a failed run, not an incomplete report.

## Self-Check Before Completing a DEBUG Run

Before completing the run, verify internally:

- The host-assigned implementation handoff for the current plan revision was available, whether active or archived.
- That `Implementation Handoff` and the concrete debug request were read before debugging began.
- The canonical `Debug Handoff` for the active plan revision was read before debugging began.
- No correction exceeded the approved implementation scope.
- No more than two code-changing correction attempts were performed.
- Every code-changing correction attempt was followed by workspace-defined build verification.
- No restore was run autonomously and no restore-required failure was bypassed with a restore-enabled command.
- No further validation was executed after a terminal stop condition.
- **The run concluded with exactly one outcome:** either `status: implemented` or `status: failed` — never both, and never neither; `planning_ready: false` was preserved in either outcome.
- The implementation handoff was not consumed, moved, recreated, or reactivated and remains available through the existing host context for subsequent `DEBUG` runs.
- Errors alone do not require a new `PLAN` before further `DEBUG` or `REPORT`.
- The canonical `Debug Handoff` contains the newly confirmed debug knowledge from this run.
- The final report documents the outcome, including an unresolved problem if the run failed, as a completed report rather than an open one.
- The short final chat summary states actual measures, observed check results, checks that could not run, and remaining errors or the absence of reported errors in performed checks; any required restore notice is included.
