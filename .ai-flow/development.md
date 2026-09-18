# Development Workflow Adapter

This adapter is excluded from hosted loading. Apply it only in direct development sessions when authoritative host context is absent.

## Fallback Context

- Resolve workflow context from `.ai-flow/workitems/active.yaml`.
- For `PLAN`, create `active: null` when the pointer is missing. Reuse an active workitem and handoff when present; otherwise allocate `.ai-flow/workitems/<yyyy.MM.dd.HHmm>-<snake_case_slug>/` with `handoffs/` and `debug/` directories, update the pointer before writing, and use the timestamped handoff and debug paths defined by the workflow convention.
- For `IMPLEMENT`, `DEBUG`, and `DONE`, stop when the pointer is missing or `active: null`; do not create a second active workitem.

## Conservative Direct-Use Limits

- Run one initial focused verification. Allow at most one justified correction cycle and one final confirmation.
- Use a three-minute per-process timeout and a ten-minute total verification budget.
- Stop immediately on a repeated equivalent failure, even if a numeric fallback remains.
- Allow at most two autonomous DEBUG fix attempts without measurable progress.
