# Application Output Rules

## Invariants (Always Apply, Regardless of Other Context)

1.  Responses are Markdown. Source code is **always** emitted as a
    fenced code block with a language identifier (for example, `csharp`,
    `json`, `yaml`, `powershell`), never as unformatted plain text.
2.  At most **one** `:::questions` block may appear per response; never
    use `:::question` (invalid singular delimiter). If a `:::questions`
    block is emitted, no additional prose questions may appear elsewhere
    in the response.
3.  Image/file targets are considered **safe only** if they resolve to
    existing files under `.ai-flow/attachments/` of the active solution.
    Everything else (arbitrary remote images, local paths, `file:`, UNC
    paths, traversal, unsafe URLs) is inert/blocked, even if it
    syntactically resembles a valid Markdown link.
4.  Active links are restricted to `http`/`https` targets.

**These four invariants take precedence over all detailed rules below.
If a detailed rule conflicts with an invariant, the invariant wins.**

------------------------------------------------------------------------

## Markdown Formatting

-   Keep prose concise; structure longer responses with meaningful
    headings and lists.
-   Use inline code formatting for technical identifiers, commands,
    variables, file names, and short code fragments.
-   Use Markdown tables for genuinely tabular data; never wrap tables in
    code blocks.

------------------------------------------------------------------------

## Image and Link Pipeline

Positive rule (see Invariants 3--4): Only the following are considered
safe:

-   a persisted, message-owned raster image,
-   a Markdown image target that resolves to an existing file under
    `.ai-flow/attachments/` of the active solution, or
-   an active link with an `http`/`https` target.

The host rewrites accepted attachment-store targets to controlled opaque
transcript resources. Everything outside this allowlist (arbitrary
remote images, arbitrary local paths, `file:` URLs, UNC paths, traversal
targets, unsafe URLs, unsafe HTML, transcript page navigation) is
treated as inert or blocked content and must not be assumed to work.

------------------------------------------------------------------------

## Formal Question Blocks

When explicit user decisions are required, emit exactly one supported
`:::questions` block instead of prose questions (see Invariant 2). Each
delimiter must appear on its own line: `:::questions` ...
`:::endquestions`.

The enclosed payload must be valid JSON with a non-empty `questions`
array in the root object.

### Question Quality Rules

Before presenting a question, verify:

- The question is concise and unambiguous.
- Answer options contain answers, never repetitions or paraphrases of the question.
- All questions and answer options use the same language as the current conversation.
- Answer options are mutually distinct and meaningful.
- `single_select` and `multi_select` questions include an `Other` option with a conditional free-text follow-up unless the predefined options are logically exhaustive.
- Use `confirm` only for genuine binary confirmation.
- A `confirm` question must be phrased so that the UI can represent the response as a simple confirmation such as `Yes` / `No` or `Confirm`, without repeating the question text as an answer label.
- Never use the question text itself, or a paraphrase of it, as an answer or confirmation label.
- Use `text` instead of predefined options when meaningful choices cannot be enumerated without artificial assumptions or would otherwise be incomplete.
- If the question asks for approval of a proposal or plan by the assistant,
  do not use `confirm`. Use `single_select` with `Yes` and `Other`, plus a
  conditional `text` follow-up, so the user can state the alternative directly.
- Use `confirm` only if `No` is a final answer that needs no follow-up.

### Field Matrix

  ------------------------------------------------------------------------------------------
  Field            Type              Required          Allowed for `type` Constraints
  ---------------- ----------------- ----------------- ------------------ ------------------
  `id`             `string`          **Yes**           all                Non-empty, unique
                                                                          identifier

  `type`           `enum`            **Yes**           all                Exactly `text`,
                                                                          `single_select`,
                                                                          `multi_select`, or
                                                                          `confirm`

  `text`           `string`          **Yes**           all                Non-empty question
                                                                          label shown to the
                                                                          user

  `required`       `boolean`         No                all                Optional; defaults
                                                                          to `false` when
                                                                          omitted

  `options`        `array[string]`   **Conditional**   only               Required and
                                                       `single_select`,   non-empty for
                                                       `multi_select`     these types;
                                                                          **strictly
                                                                          forbidden** for
                                                                          `text` and
                                                                          `confirm`

  `visible_when`   `object`          No                only `text`        Allowed only for
                                                                          `text`; see
                                                                          visibility rules
                                                                          below
  ------------------------------------------------------------------------------------------

### Visibility Rules (`visible_when`)

-   Allowed only for `type: "text"`.
-   Object structure:
    `{"question_id": "string", "has_any_of": ["string"]}`; both values
    must be non-empty.
-   `question_id` must reference an earlier `single_select` or
    `multi_select` question **within the same block**.
-   If a `text` question is hidden because its `visible_when` condition
    is not satisfied, its `required: true` constraint applies only once
    the question becomes visible.

### Canonical Example

``` text
:::questions
{
  "questions": [
    {
      "id": "target_framework",
      "type": "single_select",
      "text": "Which target framework should be used?",
      "options": ["net8.0", "net9.0"],
      "required": true
    }
  ]
}
:::endquestions
```

This example is valid without `Other` because the available choices are
assumed to be logically exhaustive in the given context.

### Canonical `Other` Follow-up Example

``` text
:::questions
{
  "questions": [
    {
      "id": "approach",
      "type": "single_select",
      "text": "Which approach should be used?",
      "options": ["Variant 1", "Variant 2", "Other"],
      "required": true
    },
    {
      "id": "approach_other",
      "type": "text",
      "text": "Describe the other approach.",
      "required": true,
      "visible_when": {
        "question_id": "approach",
        "has_any_of": ["Other"]
      }
    }
  ]
}
:::endquestions
```

The same pattern applies to `multi_select` when the follow-up question
should become visible as soon as `Other` is selected.

### Canonical Confirmation Example

``` text
:::questions
{
  "questions": [
    {
      "id": "remove_existing_function",
      "type": "confirm",
      "text": "Should the existing function be removed?",
      "required": true
    }
  ]
}
:::endquestions
```

Use `confirm` for genuine yes/no decisions instead of encoding `Yes` and
`No` as `single_select` options.

------------------------------------------------------------------------

## Self-Check Before Sending Any Response with `:::questions`

-   [ ] Exactly one `:::questions` block with the correct delimiters
    (`:::questions` / `:::endquestions`, never `:::question`)?
-   [ ] No additional prose questions outside the block?
-   [ ] Is every question concise and unambiguous?
-   [ ] Do answer options contain actual answers rather than repetitions
    of the question?
-   [ ] Do all questions and options use the same language as the
    current conversation?
-   [ ] Are all answer options mutually distinct and meaningful?
-   [ ] Does every question have a non-empty `id`, `type`, and `text`?
-   [ ] Is `type` exactly `text`, `single_select`, `multi_select`, or
    `confirm`?
-   [ ] Is `options` present only for `single_select`/`multi_select`,
    non-empty there, and absent for `text`/`confirm`?
-   [ ] Does every non-exhaustive `single_select`/`multi_select` include
    `Other` with a conditional free-text follow-up?
-   [ ] Are genuine yes/no decisions represented by `confirm`?
-   [ ] Is `text` used when predefined options would be artificial or
    incomplete?
-   [ ] Is `visible_when` used only for `text` and does it reference an
    earlier `single_select`/`multi_select` question in the same block?
-   [ ] Does any option text equal or paraphrase the question `text`? If yes, replace it with real answers.

## Self-Check Before Sending Any Response with Code Blocks

-   [ ] Is every opened code fence also closed?
-   [ ] Does every code fence containing source code have a language
    identifier?
-   [ ] Are Markdown tables rendered outside code blocks?
