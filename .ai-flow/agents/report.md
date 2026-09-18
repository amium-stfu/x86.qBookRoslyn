# REPORT Mode

## Purpose

`REPORT` creates a concise, human-readable summary of the selected session for later documentation use.

## Session Invariants

1. `REPORT` is read-only. Do not modify source files, workflow state, handoffs, or other session artifacts.
2. Use only evidence from the selected session.
3. Do not introduce new technical, scope, architecture, or workflow decisions.
4. Do not classify the implementation as complete, incomplete, blocked, or requiring further work.

## Report Content

The report must contain exactly these primary sections:

### Objective

Summarize the resolved goal of the session.

Describe what the session was intended to achieve, not the conversation history or individual planning steps.

### Implementation Result

Summarize what was actually implemented or changed during the session.

Include only information relevant to understanding the resulting state, such as:

- implemented functional changes,
- relevant structural or architectural changes,
- important constraints that affected the result,
- validation results when they materially qualify the implementation result,
- known objective issues that remain at the end of the session.

Do not reproduce implementation steps, debug history, raw validation logs, or internal workflow mechanics unless they are necessary to understand the final result.

## Documentation Quality

Write the report so that it can later be combined with reports from other sessions into higher-level project documentation.

- Prefer concise, durable descriptions over transient implementation details.
- Use human-readable language.
- Describe the resulting behavior or structure rather than the AI workflow that produced it.
- Avoid references such as "the assistant", "this chat", or "the previous response".
- Preserve relevant technical names when they are necessary to identify components, files, interfaces, or behavior.

## Evidence Boundary

Base the report only on confirmed evidence from the selected session.

Do not infer implementation results that were not confirmed.

If an intended change was not implemented, do not describe it as an implementation result.

## Self-Check Before Completing REPORT

Before completing the report, verify internally:

- The objective reflects the resolved session goal.
- The implementation result reflects only confirmed changes.
- No new technical or workflow decision was introduced.
- No completeness classification was made.
- Internal workflow history was omitted unless required to understand the result.
- The report can be understood later without reading the original session.