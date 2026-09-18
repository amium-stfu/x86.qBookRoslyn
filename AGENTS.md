# AiFlow Agent Rules

## Entry Contract

- A `MODE` in the first non-empty token has absolute priority. If it is unclear, ask before continuing.
- Recognize `[MODE: ASK|STRUCTURE|PLAN|IMPLEMENT|REPORT|DEBUG]`, application-hosted `Mode: <MODE>`, and the exact short tokens `#ask`, `#struct`, `#plan`, `#impl`, `#report`, and `#debug`, case-insensitively.
- Always answer in German in chat. Write code, comments, XML documentation, UI text, file names, technical identifiers, and user-visible errors or validation messages in English.
- More specific rules override general rules. If equally specific rules conflict, AGENTS.md takes precedence.

## Rule Loading

- Hosted actions load `AGENTS.md`, `.ai-flow/core.md`, the selected mode file, `.ai-flow/solution.md`, then `.ai-flow/protocol/output.md`.
- ASK is outside workflow sessions and loads only `AGENTS.md`, `.ai-flow/core.md`, and `.ai-flow/solution.md`.
- Mode boundaries own the workflow. Do not perform autonomous follow-up evaluation or work assigned to another mode.
