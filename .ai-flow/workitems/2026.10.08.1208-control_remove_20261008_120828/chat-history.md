# Chat History

- Session Title: Control Remove - 20261008-120828
- Session Id: 003f1dc9-651f-4207-b220-312ab8d9aca9
- Exported At: 2026-10-08T18:08:10.2107866+00:00
- WorkItem Path: .ai-flow/workitems/2026.10.08.1208-control_remove_20261008_120828

## 2026-10-08T10:08:28.2407996+00:00 | User | 02895939-64fb-4088-91d6-56553fe978a3

# Title Control Remove

## Current
Control.Remove funktioniert grundsätzlich.

## Goal
Zu klären ist wie wir das sauber beim BoxView System sauber lösen.Hier haben wir ja eine Klasse UIBox.Pragmatisch wäre ein Wrapper.Remove() { Frame.Remove(), }Zu klären ist nur ob da nicht noch mehr aufgräumt werden muss.Es gibt ja zwei Fälle:- UIBox eigenständig- UIBox eingehängt per BoxViewIn jedem Fall muss doch auch das Item mit aufgräumt werden bei einer UIBoxWas auch geklärt werden muss:- Remove() ist zu generisch und wird nicht als Fehler in VS Code erkannt. Evtl. RemoveControl()

## Acceptance Criteria


## Out of Scope


Attachments:
- [BoxView.cs](attachments/02895939-64fb-4088-91d6-56553fe978a3/BoxView.cs) (unknown)
- [BoxView.cs](attachments/02895939-64fb-4088-91d6-56553fe978a3/BoxView_2.cs) (unknown)

## 2026-10-08T10:11:53.1282204+00:00 | Assistant | 3fa1bbf1-6f50-4810-af61-4f7af54f9e28

## Analyse

`Control.Remove()` (inkl. `PrepareRemoval`/`OwnedControls`/`ReleaseControl`) ist bereits vorhanden und rekursiv funktionsfähig. Für `UIBox` existiert bereits dieselbe Infrastruktur (`OwnedUIControls()`, `DetachUIControl()`, `ReleaseOwnedUI()` → nullt `Frame`/`Description`/`State`/`Read`/`Led` und ruft `UnregisterObject()`), wird aber nur von *besitzenden* `Control`-Typen genutzt (z. B. `CameraControl`, `TLiteHCameraControl`), nicht von `UIBox` selbst.

`Description`/`State`/`Read`/`Led` werden immer über `boxes:` im `Frame`-Konstruktor erzeugt und damit per `box.Parent = Frame` zu `Frame.Children`. Ein `Frame.Remove()` entfernt diese UI-Boxen also bereits rekursiv aus `Root.ControlDict`. Die fehlende Schicht ist: (1) ein einheitlicher Einstiegspunkt auf `UIBox`, der `Frame.Remove()` + `ReleaseOwnedUI()` kombiniert, und (2) die Bereinigung der **Besitzer-Sammlung**, wenn eine `UIBox` per `BoxView.Add(...)` eingehängt wurde — aktuell landet eine `UIBox` wahlweise in `BoxView.Boxes` (List) oder in `BoxList.Boxes`/`BoxTable.Boxes`/`BoxFlowPanel.Boxes` (Dictionary), aber keine dieser Klassen hat eine `Remove`-Methode, und `UIBox` kennt ihren Besitzer nicht. Ohne Bereinigung bliebe nach einem Remove ein totes `UIBox`-Objekt (mit `Frame == null`) in dieser Liste/diesem Dictionary zurück.

## Offene materielle Punkte

1. **Name des Wrappers** auf `UIBox` (`Remove()` würde mit List-/Dictionary-Remove-Semantik kollidieren, wie beim bestehenden `Control.Remove()` bereits bemängelt).
2. **Umfang der Besitzer-Bereinigung**: Soll der Wrapper die `UIBox` automatisch aus ihrer Besitzer-Sammlung (`BoxView.Boxes` bzw. `BoxList/BoxTable/BoxFlowPanel.Boxes`) entfernen (erfordert eine Owner-Rückreferenz, die beim `Add(...)` gesetzt wird), oder bleibt das für diese Iteration bewusst außerhalb des Umfangs (nur `Frame`-Baum + `UIBox`-Felder werden bereinigt, die Besitzer-Sammlung behält ggf. eine leere Hülle)?

:::questions
{
  "questions": [
    {
      "id": "wrapper_name",
      "type": "single_select",
      "text": "Wie soll die neue UIBox-Methode heißen, die Frame.Remove() und die Feldbereinigung kombiniert?",
      "options": ["RemoveControl()", "Destroy()", "Other"],
      "required": true
    },
    {
      "id": "wrapper_name_other",
      "type": "text",
      "text": "Welcher andere Methodenname soll verwendet werden?",
      "required": true,
      "visible_when": { "question_id": "wrapper_name", "has_any_of": ["Other"] }
    },
    {
      "id": "owner_cleanup_scope",
      "type": "single_select",
      "text": "Soll die neue Methode die UIBox zusätzlich automatisch aus ihrer Besitzer-Sammlung entfernen (BoxView.Boxes bzw. BoxList/BoxTable/BoxFlowPanel.Boxes), wofür UIBox eine Owner-Rückreferenz erhält, die beim Add(...) gesetzt wird?",
      "options": [
        "Ja, automatische Bereinigung in allen vier Fällen (BoxView, BoxList, BoxTable, BoxFlowPanel)",
        "Ja, aber nur für direktes BoxView.Boxes (die drei Dictionary-Fälle bleiben außerhalb des Umfangs)",
        "Nein, nur Frame-Baum und UIBox-Felder bereinigen; Besitzer-Sammlung bleibt unverändert",
        "Other"
      ],
      "required": true
    },
    {
      "id": "owner_cleanup_scope_other",
      "type": "text",
      "text": "Welcher andere Umfang für die Besitzer-Bereinigung soll gelten?",
      "required": true,
      "visible_when": { "question_id": "owner_cleanup_scope", "has_any_of": ["Other"] }
    }
  ]
}
:::endquestions


## 2026-10-08T10:13:08.1127816+00:00 | User | b3ee80af-d3b3-4f0f-ab36-e5a7aa656573

Wie soll die neue UIBox-Methode heißen, die Frame.Remove() und die Feldbereinigung kombiniert?: Destroy()
Soll die neue Methode die UIBox zusätzlich automatisch aus ihrer Besitzer-Sammlung entfernen (BoxView.Boxes bzw. BoxList/BoxTable/BoxFlowPanel.Boxes), wofür UIBox eine Owner-Rückreferenz erhält, die beim Add(...) gesetzt wird?: Ja, automatische Bereinigung in allen vier Fällen (BoxView, BoxList, BoxTable, BoxFlowPanel)


## 2026-10-08T10:14:08.1599130+00:00 | Assistant | 70786b36-8b61-4fc3-a939-73ce13ba5148

**Owner-Bereinigung bestätigt** (alle vier Fälle: `BoxView`, `BoxList`, `BoxTable`, `BoxFlowPanel`).

**Namenskonflikt bei `Destroy()`:** `Item.Destroy()` ist bereits eine etablierte, virtuelle, applikationsweite Lifecycle-Methode (`Object.cs`), die von `Root.ResetObjectDict()`, `Root.ResetModuleDict()`, `Root.ResetSignalDict()`, `Root.ResetMessageDict()` und `ItemDestroyHandler.DestroyAll()` für **Massen-Teardown** aufgerufen wird (z. B. `CameraControl.Destroy()` stoppt dort nur die Kamera). Aktuell ist `Destroy()` bei `UIBox` ein No-Op; Controls werden beim globalen Reset separat über `Root.ResetWidgetDict()` (`ControlDict.Clear()`, kein `Remove()`) bereinigt.

Würden wir die neue Methode `Destroy()` nennen, entstünden zwei Probleme:
- Ein gezieltes `box.Destroy()` sieht identisch aus wie der globale Teardown-Hook, obwohl die Semantik eine andere ist (gezieltes Entfernen einer einzelnen Box vs. App-weiter Reset).
- Würde `Destroy()` zusätzlich `Frame.Remove()` aufrufen, würde das bei jedem globalen `ResetObjectDict()`/`DestroyAll()` für **jede** `UIBox` ungewollt mitlaufen und mit dem bereits bestehenden separaten `ControlDict`-Cleanup kollidieren.

Das ist ein materieller Konflikt mit bestehender Architektur, keine reine Namensfrage mehr.

:::questions
{
  "questions": [
    {
      "id": "wrapper_name_v2",
      "type": "single_select",
      "text": "Welcher kollisionsfreie Name soll für die neue UIBox-Methode (Frame.Remove() + Feldbereinigung + Owner-Bereinigung) verwendet werden?",
      "options": ["RemoveControl()", "DestroyUI()", "Other"],
      "required": true
    },
    {
      "id": "wrapper_name_v2_other",
      "type": "text",
      "text": "Welcher andere Methodenname soll verwendet werden?",
      "required": true,
      "visible_when": { "question_id": "wrapper_name_v2", "has_any_of": ["Other"] }
    }
  ]
}
:::endquestions


## 2026-10-08T10:30:05.6071514+00:00 | User | c3f87fec-9532-4abe-b343-cc303e6c6e9b

Welcher kollisionsfreie Name soll für die neue UIBox-Methode (Frame.Remove() + Feldbereinigung + Owner-Bereinigung) verwendet werden?: RemoveControl()


## 2026-10-08T10:31:00.3156116+00:00 | Assistant | 513cff82-d5e4-439b-8bd6-0eda55dea4b5

**Planungsreife erreicht.** `status: ready`, `planning_ready: true`.

## Zusammenfassung

**Ziel:** Einheitliche, eindeutig benannte Methode `UIBox.RemoveControl()`, die eine `UIBox` sowohl eigenständig als auch per `BoxView`-System eingehängt sauber entfernt.

**Umfang:**
- Neue `public void RemoveControl()` auf `UIBox`: idempotent (analog zum bestehenden `Control.Remove()`-Muster, zweiter Aufruf no-op), ruft `Frame?.Remove()` (entfernt rekursiv den kompletten `Frame`-Kindbaum inkl. `Description`/`State`/`Read`/`Led`, die bereits via `boxes:`-Konstruktorparameter `Frame.Children` sind, aus `Root.ControlDict`), danach `ReleaseOwnedUI()` (nullt `Frame`/`Description`/`State`/`Read`/`Led`, ruft `UnregisterObject()` für `Root.ObjectDict`).
- Owner-Bereinigung: `UIBox` erhält eine interne Detach-Rückreferenz (z. B. `Action`-Delegat), die `BoxView.Add(UIBox box, ...)` beim Einhängen setzt — für alle vier Fälle: `BoxView.Boxes` (List), `BoxList.Boxes`, `BoxTable.Boxes`, `BoxFlowPanel.Boxes` (je Dictionary, Key = `Box.Name`). Für den `BoxList`-Fall wird aktuell **zusätzlich** in `BoxView.Boxes` eingetragen – `RemoveControl()` muss daher aus beiden Collections entfernen. `RemoveControl()` ruft die Rückreferenz vor/während der Bereinigung auf.
- Für eigenständige `UIBox` (kein `BoxView.Add(...)`) bleibt die Rückreferenz `null`; `RemoveControl()` beschränkt sich auf Frame- und Feldbereinigung.

**Non-Goals:** Keine Änderung an `Control.Remove()` (funktioniert bereits), an `Item.Destroy()`/`ItemDestroyHandler` (globaler Teardown bleibt unangetastet), an `BoxView.Add(object box)` für `BoxTable`/`BoxFlowPanel`/`BoxList`/`ChartExtended`/`BoxLogView`/`BoxProcessLogView` (eigene Objekttypen, nicht `UIBox`), kein automatisches Nachrücken/Re-Layout nach Entfernen.

**Entscheidungen:** Methodenname `RemoveControl()` (Kollision mit `Destroy()`-Lifecycle ausgeschlossen); Owner-Bereinigung gilt für alle vier Collection-Fälle.

**Validierung:** `validation: true`. Build gemäß `.ai-flow/solution.md`: `msbuild qbookStudio.sln /t:Build /p:Configuration=Release /p:Platform=x86`.

**Offene materielle Punkte:** Keine. **Verbleibende materielle PLAN-Entscheidungen:** Keine.


## 2026-10-08T10:32:12.5187045+00:00 | User | 04940c32-69d7-46f0-baae-8e238fb09514

Plan


## 2026-10-08T10:34:17.3244177+00:00 | Assistant | cd098fc8-29d1-4920-80e2-268dc4bec9dd

Implementation-Handoff erstellt: `.ai-flow/workitems/2026.10.08.1208-control_remove_20261008_120828/handoffs/2026.10.08.1208-implementation-handoff.md`

**Umfang:** Einzige Datei `qbookCsScript/Controls/BoxView.cs` — neue `UIBox.RemoveControl()` (Frame-Removal + ReleaseOwnedUI + idempotent) plus `DetachFromOwner`-Rückreferenz, gesetzt in allen vier Zweigen von `BoxView.Add(UIBox, ...)` (BoxView.Boxes, BoxList.Boxes, BoxTable.Boxes, BoxFlowPanel.Boxes). Keine Änderung an `Control.Remove()`, `Item.Destroy()` oder bestehenden Overrides.

**Validierung:** `Worker Command: Build`, `Worker Target: qbookStudio.sln` (bestehender Task `AiFlow Build: qbookStudio`), kein Clean. Launch unverändert (`AiFlow Launch: qbook` bereits vorhanden, nicht erforderlich für diese Änderung).

`planning_ready: true` bleibt erhalten; IMPLEMENT kann direkt gestartet werden.


## 2026-10-08T10:35:05.0645676+00:00 | User | ba739813-d772-4d12-b120-db75a96cb4c6

Implement with gpt-6-astra • Medium


## 2026-10-08T10:35:11.9868421+00:00 | Assistant | 3f498ddf-8a27-4234-82c2-85a0b9ef58d1

Implement completed with errors. The Codex CLI exited with code 1.
Reading prompt from stdin...
Raw Codex output:
{"type":"thread.started","thread_id":"01a11b14-bf63-7133-9086-f55eac1688fe"}
{"type":"turn.started"}
{"type":"error","message":"You’ve hit your usage limit. Upgrade to Pro (https://chatgpt.com/explore/pro), visit https://chatgpt.com/codex/settings/usage to purchase more credits or try again at 1:43 PM."}
{"type":"turn.failed","error":{"message":"You’ve hit your usage limit. Upgrade to Pro (https://chatgpt.com/explore/pro), visit https://chatgpt.com/codex/settings/usage to purchase more credits or try again at 1:43 PM."}}


## 2026-10-08T10:36:08.2078260+00:00 | User | fcf89d67-3aaf-42fc-8ddd-418244d5c580

Debug with claude-sonnet-5 • Medium


## 2026-10-08T10:37:46.1483204+00:00 | Assistant | 48c49825-92af-4847-a28d-06c60bad880c

Build und Report sind vorhanden und erfolgreich (ExitCode 0, "0 Fehler").

## Zusammenfassung

**Ursprünglicher Fehler:** Die IMPLEMENT-Arbeit wurde laut Debug-Handoff wegen TokenLimit nie gestartet — `BoxView.cs` enthielt keine der geplanten Änderungen.

**Korrekturmaßnahmen:**
1. `UIBox`: Felder `DetachFromOwner` (als `System.Action`, da ein Namenskonflikt mit der projekteigenen Klasse `QB.Action` besteht) und `removed` sowie Methode `RemoveControl()` ergänzt (ruft `DetachFromOwner`, `Frame?.Remove()`, `ReleaseOwnedUI()`, idempotent).
2. `BoxView.Add(UIBox box, ...)`: in allen vier Zweigen (`boxList`, `boxTable`, `boxFlowPanel`, Standardfall) die jeweilige `box.DetachFromOwner`-Zuweisung ergänzt.

**Validierung:** MSBuild Release/x86 auf `qbookStudio.sln`, exitCode 0, „0 Fehler". Erster Durchlauf scheiterte mit CS1660 (Lambda↔Action-Konflikt durch gleichnamige Klasse `QB.Action`), behoben durch explizite Qualifikation `System.Action`. Zweiter Durchlauf erfolgreich, Report in `.ai-flow\BuildReport.txt` gesichert.

**Verbleibende Probleme:** Keine. Bestehende Warnungen (449) sind unverändert vorbestehend und nicht durch diese Änderung verursacht.

**Annahmen:** Keine über den Handoff hinausgehenden; einzige Abweichung von der Spezifikation ist die notwendige Typqualifikation `System.Action` statt `Action`, um der Projekt-Namenskollision Rechnung zu tragen.


## 2026-10-08T17:46:03.6039999+00:00 | User | c75e8bb4-7f0e-4a6a-85ca-059ad0196104

Was jetzt noch fehlt ist:{BoxTable}.RemoveControl()Also alle Container Klassen


## 2026-10-08T17:48:50.4137132+00:00 | Assistant | 8f959491-c2f3-4c55-b861-31e8ff938040

Die vorherige Umsetzung (`UIBox.RemoveControl()` + `DetachFromOwner`) ist bereits im Code. Ich sehe aber eine Lücke: `BoxTable.Add(UIBox box, ...)` und `BoxFlowPanel.Add(...)` (der direkte Weg, ohne `BoxView.Add(box, boxTable: ...)`) setzen `DetachFromOwner` **nicht** – dort fehlt die Owner-Rückreferenz komplett. `BoxList` hat gar keine eigene `Add`-Methode (wird nur über `BoxView.Add` befüllt).

Vorschlag für den fehlenden Teil:

1. **`BoxTable.Add(...)`** und **`BoxFlowPanel.Add(...)`** zusätzlich `box.DetachFromOwner = () => Boxes.Remove(box.Name);` setzen (analog zu `BoxView.Add`).
2. Jede Container-Klasse (`BoxView`, `BoxList`, `BoxTable`, `BoxFlowPanel`) erhält eine eigene `public void RemoveControl(UIBox box)`, die **delegiert**: `box.RemoveControl();` (nutzt die bereits vorhandene Infrastruktur – Frame-Entfernung + Owner-Cleanup via `DetachFromOwner`). Für `BoxList` entsprechend, da `Boxes` dort ein `Dictionary<string, UIBox>` ist.

Damit gibt es ein symmetrisches `Add()`/`RemoveControl()`-Paar auf jeder Container-Klasse, ohne die bestehende `DetachFromOwner`-Logik zu duplizieren.

**Frage:** Passt dieser Ansatz (Container-`RemoveControl(box)` als dünner Delegations-Wrapper auf `box.RemoveControl()`, plus Nachrüsten von `DetachFromOwner` in `BoxTable.Add`/`BoxFlowPanel.Add`), oder soll `{Container}.RemoveControl(box)` stattdessen eigenständig (ohne Rückruf auf `box.RemoveControl()`) nur die eigene Collection bereinigen?


## 2026-10-08T17:54:40.9284628+00:00 | User | 10aa1a5f-d98a-46c5-9538-e087070b0b2f

Hintergrund ist zb. die Klasse BoxDialogWithParameters, oder BoxDialog.Hier möchte ich mit den neuen Möglichkeiten schaffen dass ich immer nur temporäre Dialoge öffne.entweder per using oder:   BoxDialogWithParameters edit;
            Add(edit = new BoxDialogWithParameters("dialogWithParameters", position: "center", w: 100, backColor: BackColor, page: Page, border: 1, borderColor: "Black"));
...edit.RemoveControl()


## 2026-10-08T17:56:00.6644360+00:00 | Assistant | 69189897-7dc4-4ccb-b71e-96a572493316

Verstanden: `BoxDialogWithParameters : BoxFlowPanel : BoxTable` ist **kein** `UIBox` – deshalb hilft das bereits gebaute `UIBox.RemoveControl()` hier nicht. `BoxDialog : UIBox` funktioniert über die bestehende Lösung bereits (`edit.RemoveControl()` würde dort schon greifen).

**Konkreter Lückenbefund für `BoxTable`/`BoxFlowPanel`/`BoxList`:**
- `BoxTable` hat ein eigenes `Frame` und ein `Boxes`-Dictionary mit Kind-`UIBox`-Objekten, aber keine `RemoveControl()` und keine Owner-Rückreferenz zu `BoxView`.
- `BoxView.Add(object box)` legt `BoxTable`/`BoxFlowPanel`/`BoxList`-Instanzen nur in eigene `List<T>` ab (`BoxTables`, `BoxFlowPanels`, `BoxLists`) – ohne Rückverknüpfung.

**Vorschlag:**
1. `BoxTable` (geerbt von `BoxFlowPanel`, `BoxDialogWithParameters`) erhält `internal Action DetachFromOwner`, `removed`-Flag und `public void RemoveControl()`: ruft `DetachFromOwner`, dann für jede Box in `Boxes.Values` `box.RemoveControl()`, leert `Boxes`, dann `Frame?.Remove(); Frame = null;`.
2. `BoxList` erhält dieselbe Struktur (ohne `Frame`-Schritt, da `BoxList` kein eigenes `Frame` besitzt).
3. `BoxView.Add(object box)` setzt in den Zweigen `BoxTable`/`BoxFlowPanel`/`BoxList` die jeweilige `DetachFromOwner`-Rückreferenz (`BoxTables.Remove(...)` usw.).
4. Zusätzlich: `BoxTable.Add(UIBox,...)`/`BoxFlowPanel.Add(...)` setzen `box.DetachFromOwner`, damit auch direkt (ohne `BoxView.Add(box, boxTable:...)`) hinzugefügte Kind-Boxen sauber sind (unabhängig davon aber bereits durch Schritt 1 abgedeckt, da der Container alle Kinder beim eigenen Entfernen mit entfernt).

:::questions
{"questions":[{"id":"scope_confirm","type":"single_select","text":"Passt dieser Entwurf (RemoveControl() auf BoxTable/BoxFlowPanel/BoxList inkl. Owner-Rückreferenz aus BoxView.Add(object box)) für den aktuellen Anwendungsfall (temporäre Dialoge wie BoxDialogWithParameters)?","options":["Ja, genau so umsetzen","Other"],"required":true},{"id":"scope_confirm_other","type":"text","text":"Was soll stattdessen anders umgesetzt werden?","required":true,"visible_when":{"question_id":"scope_confirm","has_any_of":["Other"]}},{"id":"chart_scope","type":"single_select","text":"Sollen ChartExtended/BoxLogView/BoxProcessLogView (ebenfalls in BoxView.Add(object box) verwaltet) in diesem Schritt dieselbe Owner-Rückreferenz/RemoveControl() erhalten, oder bleibt das außerhalb des Umfangs?","options":["Außerhalb des Umfangs (nur BoxTable/BoxFlowPanel/BoxList)","Ja, auch diese drei einbeziehen"],"required":true},{"id":"boxview_scope","type":"single_select","text":"Benötigt BoxView selbst (das Seiten-Top-Level-Objekt) ebenfalls eine RemoveControl()-Methode, oder bleibt BoxView außerhalb des Umfangs (da es nicht in einen anderen Container eingehängt wird)?","options":["BoxView bleibt außerhalb des Umfangs","Ja, BoxView soll auch RemoveControl() erhalten"],"required":true}]}
:::endquestions


## 2026-10-08T17:57:56.6519969+00:00 | User | e1d38008-f79f-4172-b379-9ba668bac9a5

Passt dieser Entwurf (RemoveControl() auf BoxTable/BoxFlowPanel/BoxList inkl. Owner-Rückreferenz aus BoxView.Add(object box)) für den aktuellen Anwendungsfall (temporäre Dialoge wie BoxDialogWithParameters)?: Ja, genau so umsetzen
Sollen ChartExtended/BoxLogView/BoxProcessLogView (ebenfalls in BoxView.Add(object box) verwaltet) in diesem Schritt dieselbe Owner-Rückreferenz/RemoveControl() erhalten, oder bleibt das außerhalb des Umfangs?: Ja, auch diese drei einbeziehen
Benötigt BoxView selbst (das Seiten-Top-Level-Objekt) ebenfalls eine RemoveControl()-Methode, oder bleibt BoxView außerhalb des Umfangs (da es nicht in einen anderen Container eingehängt wird)?: BoxView bleibt außerhalb des Umfangs


## 2026-10-08T17:59:05.3390395+00:00 | Assistant | 3a35de31-ebdf-4eff-9f64-c23cffdecb2e

Weitere Untersuchung zeigt: `BoxLogView`/`BoxProcessLogView` und `ChartExtended` passen nicht 1:1 in das einfache `BoxTable`-Muster – das ändert den Umfang spürbar gegenüber der vorigen Antwort.

**Befund:**
- `BoxLogView` und `BoxProcessLogView` (beide `: BoxFlowPanel`) haben zusätzlich ein eigenständiges `Navigation`-Box-Objekt (mit `Up`/`Down`/`Last`-Kindern), das **nicht** Teil von `Boxes` oder `Frame.Children` ist – ein geerbtes `BoxTable.RemoveControl()` würde es nicht entfernen. `BoxProcessLogView` hat zusätzlich Event-Abos (`processLog.EntryAdded`/`DisplaySettingsChanged`), die beim Entfernen abgemeldet werden müssten (sonst Leak).
- `ChartExtended` erbt **nicht** von `BoxTable`/`UIBox` und hat keine einzelne `Frame`/`Boxes`-Struktur, sondern ca. 20 einzelne `Box`/`Control`-Felder (`yFrame`, `xControl`, `RecordControl`, `f1Hz..f1kHz`, `y1Control..y4Control`, ein eigenes verschachteltes `AxisControl` (`BoxTable`), `Execute`, `yMin/yMax/yAuto`, ein `Chart`-Objekt usw.). Eine symmetrische `RemoveControl()` wäre hier eine eigenständige, deutlich größere Implementierung, kein einfaches Wiederverwenden des Containermusters.

:::questions
{"questions":[{"id":"chart_scope_revised","type":"single_select","text":"Angesichts dieser Komplexität: Soll ChartExtended.RemoveControl() trotzdem in diesem Arbeitspaket bespoke implementiert werden (alle ~20 Felder einzeln entfernen), oder als separates Workitem ausgelagert werden?","options":["In diesem Arbeitspaket bespoke umsetzen","Als separates Workitem auslagern (aus diesem Umfang entfernen)"],"required":true},{"id":"logview_hook","type":"single_select","text":"Für BoxLogView/BoxProcessLogView: soll RemoveControl() in BoxTable einen virtuellen Erweiterungspunkt bekommen (z.B. protected virtual void ReleaseAdditionalControls()), den beide Klassen überschreiben, um Navigation+Kinder zu entfernen (und BoxProcessLogView zusätzlich die Event-Abos abmeldet)?","options":["Ja, virtuellen Hook einführen","Other"],"required":true},{"id":"logview_hook_other","type":"text","text":"Wie soll die Navigation-Bereinigung stattdessen gelöst werden?","required":true,"visible_when":{"question_id":"logview_hook","has_any_of":["Other"]}}]}
:::endquestions


## 2026-10-08T18:02:10.9339001+00:00 | User | 14e59c60-b307-42f0-acd9-0a07904f14a5

Um Komplexität raus zu nehmen erstmal nur auf BoxTable beschränken.Alle relevanten Klassen sind Ableitungen


## 2026-10-08T18:03:06.0846858+00:00 | Assistant | 46b41bcf-16f9-43d0-9837-fe041c23e59d

**Scope reduziert – nur `BoxTable` (Basisklasse), keine bespoke Behandlung von `BoxList`/`ChartExtended`/Navigation-Spezialfällen in dieser Iteration.**

## Finaler Umfang

1. **`BoxTable`** (Datei `qbookCsScript/Controls/BoxView.cs`) erhält:
   - `internal System.Action DetachFromOwner;` (Owner-Rückreferenz, analog zu `UIBox`)
   - `private bool removed;` (Idempotenz-Flag)
   - `public void RemoveControl()`: ruft `DetachFromOwner` (einmalig, dann `null`), iteriert über eine Kopie von `Boxes.Values` und ruft für jede Box `box.RemoveControl()`, leert `Boxes`, ruft `Frame?.Remove(); Frame = null;`.
   - Damit erben `BoxFlowPanel`, `BoxDialogWithParameters`, `BoxLogView`, `BoxProcessLogView` die Methode automatisch (alle Ableitungen von `BoxTable`).

2. **`BoxView.Add(object box)`**: In den vier `BoxTable`-abgeleiteten Zweigen (`BoxTable`, `BoxFlowPanel`, `BoxLogView`, `BoxProcessLogView`) wird nach dem jeweiligen `List.Add(...)` die Owner-Rückreferenz gesetzt, z. B. `((BoxTable)box).DetachFromOwner = () => BoxTables.Remove((BoxTable)box);` (entsprechend für `BoxFlowPanels`/`boxLogViews`/`boxProcessLogViews`). `BoxList`- und `ChartExtended`-Zweig bleiben unverändert (außerhalb des Umfangs).

3. **`BoxTable.Add(UIBox box, ...)`** und **`BoxFlowPanel.Add(...)`** (Override) setzen zusätzlich `box.DetachFromOwner = () => Boxes.Remove(box.Name);`, damit auch direkt (ohne `BoxView.Add(box, boxTable: ...)`) hinzugefügte Kind-`UIBox`-Instanzen bei eigenständigem `box.RemoveControl()` korrekt aus `Boxes` entfernt werden.

**Explizit außerhalb des Umfangs (diese Iteration):** `BoxList`, `ChartExtended`, Navigation-Spezialbereinigung in `BoxLogView`/`BoxProcessLogView` (werden beim Entfernen eines `BoxLogView`/`BoxProcessLogView` nicht mitbereinigt – bekannte, akzeptierte Lücke), `BoxView` selbst.

Damit ist z. B. `edit.RemoveControl()` für `BoxDialogWithParameters edit = new BoxDialogWithParameters(...)` nach `Add(edit)` vollständig funktionsfähig: Frame weg, alle Kind-Boxen bereinigt, Eintrag aus `BoxView.BoxFlowPanels` entfernt.

Keine offenen materiellen Punkte mehr. **Planungsreife erreicht** – `PLAN` kann einen Implementation-Handoff mit Build-Validierung (`msbuild qbookStudio.sln /t:Build /p:Configuration=Release /p:Platform=x86`, bestehender Task `AiFlow Build: qbookStudio`) erstellen.


## 2026-10-08T18:05:55.6691804+00:00 | User | 56c5067e-8831-4d83-a797-45bbec2a9131

Wenn Planungsreife bitte die workitem.md schreiben.


## 2026-10-08T18:06:50.5082093+00:00 | Assistant | 60d91413-503c-40e7-86e1-176ed5fe727e

`workitem.md`, `workitem.json` und `status.md` sind aktualisiert: `status: ready`, `planning_ready: true`. Der Scope-Body in `workitem.md` enthält jetzt Goal, Acceptance Criteria und Out-of-Scope für `BoxTable.RemoveControl()`. PLAN kann direkt gestartet werden.


## 2026-10-08T18:06:57.2258317+00:00 | User | 930e9211-19aa-49d2-a95e-ed34dee18f1e

Plan


## 2026-10-08T18:08:10.0379349+00:00 | Assistant | a0c93ef0-5da7-4958-83a7-dac64fcf278a

Implementation-Handoff erstellt: `.ai-flow/workitems/2026.10.08.1208-control_remove_20261008_120828/handoffs/2026.10.08.1208-implementation-handoff.md`

**Umfang:** Einzige Datei `qbookCsScript/Controls/BoxView.cs` — neue `BoxTable.RemoveControl()` (Frame-Removal + Kind-Boxen-Cleanup + Owner-Detach, idempotent), Owner-Wiring in `BoxView.Add(object box)` für die vier `BoxTable`-Ableitungen (`BoxTable`, `BoxFlowPanel`, `BoxLogView`, `BoxProcessLogView`), sowie `DetachFromOwner`-Nachrüstung in `BoxTable.Add(UIBox,...)`/`BoxFlowPanel.Add(...)` für direkt hinzugefügte Kind-Boxen. `BoxList`, `ChartExtended`, `BoxView` bewusst unverändert.

**Validierung:** `Worker Command: Build`, `Worker Target: qbookStudio.sln` (bestehender Task `AiFlow Build: qbookStudio`), kein Clean. `planning_ready: true` bleibt erhalten; IMPLEMENT kann direkt gestartet werden.