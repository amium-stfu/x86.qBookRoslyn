# Application Output Rules

## Invarianten (gelten immer, unabhängig vom restlichen Kontext)

1. Antworten sind Markdown. Quellcode wird **immer** als eingezäunter Codeblock mit Sprachkennung ausgegeben (z. B. `csharp`, `json`, `yaml`, `powershell`), nie als unformatierter Klartext.
2. Höchstens **ein** `:::questions`-Block pro Antwort; niemals `:::question` (falsches Delimiter, Singular). Wenn ein `:::questions`-Block ausgegeben wird, dürfen **keine zusätzlichen Prosa-Fragen** im übrigen Antworttext gestellt werden.
3. Bild-/Dateiziele sind **nur** sicher, wenn sie auf existierende Dateien unter `.ai-flow/attachments/` der aktiven Solution auflösen. Alles andere (arbiträre Remote-Bilder, lokale Pfade, `file:`, UNC, Traversal, unsichere URLs) gilt als inert/blockiert — auch wenn es syntaktisch wie ein gültiger Markdown-Link aussieht.
4. Aktive Links sind ausschließlich `http`/`https`-Ziele.

**Diese vier Punkte haben Vorrang vor allen Detailregeln unten. Bei Widerspruch zwischen einer Detailregel und einer Invariante gilt die Invariante.**

---

## Markdown-Formatierung

- Prosa knapp halten; längere Antworten mit sinnvollen Überschriften und Listen strukturieren.
- Inline-Code-Formatierung für technische Bezeichner, Befehle, Variablen, Dateinamen und kurze Codefragmente.
- Markdown-Tabellen für echte tabellarische Daten; Tabellen niemals in Codeblöcke einwickeln.

---

## Bild- und Link-Pipeline

Positivregel (siehe Invariante 3–4): Sicher ist ausschließlich

- eine persistierte, message-eigene Rasterdatei, oder
- ein Markdown-Bildziel, das auf eine existierende Datei unter `.ai-flow/attachments/` der aktiven Solution auflöst, oder
- ein aktiver Link mit `http`/`https`-Ziel.

Der Host schreibt akzeptierte Attachment-Store-Ziele auf kontrollierte opake Transcript-Ressourcen um. Alles außerhalb dieser Positivliste (beliebige Remote-Bilder, beliebige lokale Pfade, `file:`-URLs, UNC-Pfade, Traversal-Ziele, unsichere URLs, unsicheres HTML, Transcript-Seitennavigation) wird als inerter oder blockierter Inhalt behandelt und darf nicht als funktionierend vorausgesetzt werden.

---

## Formal Question Blocks

Wenn explizite Nutzerentscheidungen erforderlich sind: genau einen unterstützten `:::questions`-Block statt Prosa-Fragen ausgeben (siehe Invariante 2). Delimiter jeweils auf eigener Zeile: `:::questions` … `:::endquestions`. Der eingeschlossene Payload ist valides JSON mit einem nicht-leeren `questions`-Array im Root-Objekt.

### Feldmatrix (ein Feld pro Zeile)

| Feld | Typ | Pflicht | Erlaubt bei `type` | Constraints |
|---|---|---|---|---|
| `id` | `string` | **Ja** | alle | Nicht-leerer, eindeutiger Identifier |
| `type` | `enum` | **Ja** | alle | Exakt `text`, `single_select`, `multi_select` oder `confirm` |
| `text` | `string` | **Ja** | alle | Nicht-leeres, dem Nutzer angezeigtes Fragelabel |
| `required` | `boolean` | Nein | alle | Optional; Standardwert `false`, wenn weggelassen |
| `options` | `array[string]` | **Bedingt** | nur `single_select`, `multi_select` | Pflicht und nicht-leer bei diesen Typen; **strikt verboten** bei `text` und `confirm` |
| `visible_when` | `object` | Nein | nur `text` | Nur bei `text` erlaubt; siehe Sichtbarkeitsregeln unten |

### Sichtbarkeitsregeln (`visible_when`)

- Nur bei `type: "text"` zulässig.
- Objektstruktur: `{"question_id": "string", "has_any_of": ["string"]}`, beide nicht leer.
- `question_id` muss eine frühere `single_select`- oder `multi_select`-Frage **im selben Block** referenzieren.
- Ist eine `text`-Frage wegen nicht erfüllter `visible_when`-Bedingung verborgen, greift ihr `required: true` erst, sobald sie sichtbar wird.

### Canonical Example

```text
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

### Canonical `Other`-Follow-up Example

```text
:::questions
{
  "questions": [
    {
      "id": "approach",
      "type": "single_select",
      "text": "How should I do this?",
      "options": ["Variant 1", "Variant 2", "Other"],
      "required": true
    },
    {
      "id": "approach_other",
      "type": "text",
      "text": "Describe your proposal.",
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

Dasselbe Muster gilt analog für `multi_select`, wenn die Folgefrage erscheinen soll, sobald eine der gelisteten Optionen gewählt wurde.

---

## Self-Check vor dem Senden jeder Antwort mit `:::questions`

- [ ] Genau ein `:::questions`-Block, korrektes Delimiter (`:::questions` / `:::endquestions`, nicht `:::question`)?
- [ ] Keine zusätzlichen Prosa-Fragen neben dem Block?
- [ ] Jede Frage hat nicht-leere `id`, `type`, `text`?
- [ ] `type` ist exakt `text`, `single_select`, `multi_select` oder `confirm`?
- [ ] `options` nur bei `single_select`/`multi_select`, dort nicht leer; bei `text`/`confirm` nicht vorhanden?
- [ ] `visible_when` nur bei `text`, referenziert eine frühere `single_select`/`multi_select`-Frage im selben Block?

## Self-Check vor dem Senden jeder Antwort mit Codeblöcken

- [ ] Ist jeder geöffnete Codefence (```` ``` ````) auch wieder geschlossen?
- [ ] Hat jeder Code-Fence eine Sprachkennung (kein nackter ```` ``` ```` vor Quellcode)?
- [ ] Sind Markdown-Tabellen außerhalb von Codeblöcken gerendert?
