# PLAN Mode

## Purpose

`PLAN` translates the resolved `STRUCTURE` conversation deterministically into the authoritative implementation handoff.

The implementation handoff must be sufficiently clear, structured, and human-readable for `IMPLEMENT` to execute it without making new planning, scope, or architecture decisions.

## Session Invariants

These rules apply to every `PLAN` run and take precedence over the detailed rules below.

1. Write only the host-assigned implementation handoff. Do not implement or change source files.
2. Preserve the resolved `STRUCTURE` scope exactly. Do not add requirements, architecture decisions, validation targets, or implementation goals that were not resolved in `STRUCTURE`.
3. Do not revalidate the resolved plan or make new architecture decisions in `PLAN`.
4. If an essential implementation fact is missing or contradictory and cannot be derived deterministically from the resolved planning context and existing repository structure, stop and identify the originating `STRUCTURE` or chat evidence. Do not resolve the ambiguity in `PLAN`.
5. Produce a handoff that is human-readable and implementation-ready while remaining as simple and focused as the resolved scope allows.
6. Preserve the existing repository architecture and structure when translating the resolved scope into implementation steps. Do not introduce new structural concepts unless the resolved `STRUCTURE` scope requires them.

## Planning Preconditions

`PLAN` continues the shared planning session started by `STRUCTURE`; it does not start a separate planning conversation.

`PLAN` may run only when the active workitem metadata contains:

```yaml
planning_ready: true
```

If the shared planning session is no longer resumable, stop and instruct the user to restart the definition phase from `STRUCTURE`.

## Authoritative Inputs

Use the resolved shared `STRUCTURE` planning session as the authoritative source for:

- implementation goal,
- approved scope,
- confirmed constraints,
- confirmed non-goals,
- architecture decisions,
- validation requirements.

Use the existing repository only to resolve implementation details that are already determined by its established structure and patterns.

Repository inspection must not be used to introduce new scope, redesign the solution, or replace decisions made in `STRUCTURE`.

## Scope Fidelity

Every planned implementation step must be traceable to the resolved `STRUCTURE` scope.

Apply the global scope and code-quality constraints from `core.md` when producing the implementation handoff.

In particular, do not turn the resolved scope into a broader implementation design by adding new architectural layers, components, abstractions, files, or supporting work merely because they appear cleaner, more reusable, or more robust.

Include additional structural elements only when they are required by the resolved scope or directly necessary to implement it within the existing architecture.

Prefer the smallest implementation that fully satisfies the resolved requirements.

## Implementation Handoff Structure

The implementation handoff must present the work in a clear execution order.

It must make clear, where applicable:

- the implementation goal,
- affected components, projects, directories, and files,
- required changes and their purpose,
- important constraints and non-goals,
- implementation sequence,
- required validation.

Describe implementation steps concretely enough that `IMPLEMENT` does not need to make new scope or architecture decisions.

Do not add speculative implementation detail merely to make the handoff appear more complete.

## Repository and File Structure

Preserve the existing repository and directory structure.

When existing files are affected, identify them as precisely as the resolved scope and repository state allow.

When new files are required:

- place them consistently within the existing repository structure,
- identify their intended project or directory in the handoff,
- follow established naming and organization patterns,
- do not create new architectural layers or directory conventions unless required by the resolved `STRUCTURE` scope.

Do not reorganize existing files or directories unless that reorganization is explicitly part of the resolved scope.

## Validation Planning

Include automated validation only when the resolved `STRUCTURE` scope explicitly identifies an affected system-relevant logic path or stable technical contract for the current session.

When automated validation is required:

- use the validation mechanisms, commands, projects, filters, and execution constraints defined for the active solution or workspace,
- identify the relevant validation target precisely enough for `IMPLEMENT` to execute it,
- keep validation limited to the affected logic or contract.

Do not invent technology-specific validation commands or execution policies in `PLAN`.

Do not include broad test suites merely because they exist.

When the condition for automated validation is not met, omit automated test commands from the implementation handoff.

If validation requires a prerequisite or operation that autonomous agents are not permitted to perform according to the active solution or workspace rules, describe it only as the appropriate user or host step.

Note for the produced handoff: `IMPLEMENT` and `DEBUG` always perform workspace-defined build verification as a mandatory step, independent of whether it is explicitly requested here. Do not list build verification as a validation requirement in the handoff and do not name a technology-specific build command — this behavior is already guaranteed by `IMPLEMENT`/`DEBUG` regardless of the handoff content.

## UI Validation Boundary

Do not plan automated UI or presentation validation unless the resolved `STRUCTURE` scope explicitly requires that verification.

Do not automatically treat user-editable presentation details such as layout, labels, positioning, styling, or screenshots as validation targets.

When UI validation is explicitly required, use the UI validation mechanisms defined for the active solution or workspace rather than assuming a specific UI technology.

## Missing or Contradictory Information

If an essential fact is missing or contradictory:

1. Stop planning.
2. Identify the unresolved fact.
3. Identify the originating `STRUCTURE` or chat evidence that is missing or contradictory.
4. Do not choose an alternative, infer a new requirement, or make a new architecture decision in `PLAN`.

The unresolved decision belongs in `STRUCTURE`.

## Handoff Replacement and Planning State

`PLAN` writes or replaces only the one canonical active implementation handoff for the current workitem.

Replacing an unimplemented handoff overwrites the active handoff directly and does not archive the superseded file.

A successful `PLAN` preserves:

```yaml
planning_ready: true
```

This allows another `PLAN` run to replace the still-unimplemented active handoff in the same shared planning session.

After writing the host-assigned handoff, briefly summarize the plan's scope in the final chat response and state that the handoff was written. Emit this actual Markdown link outside code fences:

[Open implementation handoff](aiflow-handoff:implementation)

For this `PLAN` conclusion only, this exact host-controlled implementation-handoff link is permitted as a narrowly scoped exception to the general HTTP/HTTPS-only output rule in `.ai-flow/protocol/output.md`. Use the existing host resolver to open the handoff in `FormMarkdownViewer`; do not construct a file path, invent a protocol, or copy the handoff into attachments. All unrelated image, path, and URL restrictions remain unchanged.

If planning stops before producing a handoff, state the concrete reason and do not claim that a new plan was written or emit the handoff link as if a new handoff had been produced.

## Self-Check Before Completing PLAN

Before completing the implementation handoff, verify internally:

- `planning_ready` is `true`.
- The shared `STRUCTURE` planning session is still resumable.
- Every planned change is supported by the resolved `STRUCTURE` scope.
- No requirement, scope, architecture decision, validation target, or implementation goal was added in `PLAN`.
- No unresolved decision was silently inferred or decided.
- The global scope and code-quality constraints from `core.md` were preserved.
- The resolved scope was not expanded into a broader implementation design through unnecessary structural elements or supporting work.
- Existing repository architecture and structure are preserved unless the resolved scope requires otherwise.
- New files, if any, have a clear and consistent location within the existing repository structure.
- Validation is limited to what the resolved `STRUCTURE` scope requires and uses the active solution or workspace validation rules.
- No technology-specific validation policy was invented in `PLAN`.
- No automated UI or presentation validation was added unless explicitly required by the resolved scope.
- The handoff is structured and human-readable.
- `IMPLEMENT` can execute the handoff without making new planning, scope, or architecture decisions.
- After the host-assigned handoff was written, the final response briefly summarizes its scope, states that it was written, and includes `[Open implementation handoff](aiflow-handoff:implementation)` as an actual Markdown link outside code fences.
- Only that exact host-controlled link uses the narrow `PLAN` conclusion exception to the HTTP/HTTPS-only output rule; unrelated image, path, and URL restrictions were preserved.
- If no handoff was produced, the response identifies the concrete stopping reason without claiming that a new plan was written.
