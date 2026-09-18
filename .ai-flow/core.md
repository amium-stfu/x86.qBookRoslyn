# Core Workflow Rules

## Scope And Authority

- Only implement work explicitly defined by the approved handoff. Never infer additional implementation tasks.
- Write permission is default-deny. Modify only files within the approved implementation scope.
- Searches, references, transcripts, and discovered files never expand scope.
- Do not perform unrelated cleanup, improvements, test generation, documentation generation, quality scoring, or completeness classification.
- Mode boundaries, not autonomous follow-up evaluation, own the workflow.

## Verification And Evidence

- Use only the validation explicitly required by the approved handoff.
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

