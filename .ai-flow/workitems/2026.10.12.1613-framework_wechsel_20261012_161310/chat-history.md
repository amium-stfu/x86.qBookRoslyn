# Chat History

- Session Title: Framework wechsel - 20261012-161310
- Session Id: 65f2cdd0-98c2-4a72-b085-7a3ffdb1fda5
- Exported At: 2026-10-12T14:51:20.6725309+00:00
- WorkItem Path: .ai-flow/workitems/2026.10.12.1613-framework_wechsel_20261012_161310

## 2026-10-12T14:13:10.6847064+00:00 | User | a2497900-e044-4de6-81ce-646bb128beeb

# Title Framework wechsel

## Current
Aktuell läuft das qBook 4.8

## Goal
Kläre ob ein Wechsel auf ein aktuelles Framework machbar wäre.Ideal auf Net 10

## Acceptance Criteria


## Out of Scope



## 2026-10-12T14:15:39.4243878+00:00 | Assistant | 37e438f1-4be6-4bf9-8c22-58adf13d9291

**Ein Wechsel auf .NET 10 erscheint grundsätzlich machbar, erfordert aber eine Portierung mit Anpassungen an Code und Abhängigkeiten.** Eine vollständige Kompatibilitätszusage lässt sich aus der bisherigen Prüfung noch nicht ableiten.

.NET 10 ist eine unterstützte LTS-Version bis November 2028. WinForms kann dabei erhalten bleiben; das naheliegende Ziel wäre `net10.0-windows` mit x64. [Microsoft-Supportübersicht](https://dotnet.microsoft.com/en-us/platform/support/policy), [WinForms-Migration](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/migration/)

Die wesentlichen Befunde im aktuellen Repository:

| Bereich | Befund und Bedeutung |
|---|---|
| Projekte | `qbook` verwendet Framework 4.8, `qbookCsScript` 4.7.2. Beide müssen berücksichtigt werden. |
| Projektformat | Klassische `.csproj`-Dateien und `packages.config` müssen für die Migration angepasst werden. |
| Thread-Lebenszyklus | `qbookCsScript` nutzt `Thread.Abort()` unter anderem in Netzwerkclients und `Controls/Object.cs`. Diese Aufrufe funktionieren unter .NET 10 nicht und benötigen eine andere Abbruchlogik. [API-Dokumentation](https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.abort?view=net-10.0) |
| Framework-APIs | `Studio/Helpers.cs` verwendet `System.Web.Script.Serialization.JavaScriptSerializer`. Diese Framework-Abhängigkeit muss ersetzt werden. |
| Browser | Für die verwendete CefSharp-Version existiert `CefSharp.WinForms.NETCore`. Das eröffnet einen Migrationsweg; die Paketkompatibilität ersetzt noch keinen Funktionstest. [Paketinformationen](https://www.nuget.org/packages/CefSharp.WinForms.NETCore/141.0.110) |
| Skripte und Bücher | `BookBuilder.cs` erzeugt bereits Projekte für `net8.0`; `RoslynService.cs` enthält einen Ansatz mit `AssemblyLoadContext`. Referenzen, Kompilierung und wiederholtes Laden müssen dennoch zusammen geprüft werden. |
| Weitere Abhängigkeiten | Actipro, Datenbankanbieter, Kamera-/Gerätebibliotheken und Outlook-COM benötigen eine gezielte Prüfung ihrer tatsächlich benötigten Funktionen. |

Die gefundenen `AppDomain.CreateDomain`-Stellen liegen in derzeit nicht eingebundenen Editor-Dateien. Der `BinaryFormatter`-Treffer ist auskommentiert. Ich werte beides deshalb bislang nicht als aktiven Migrationsblocker.

**Vorläufige Bewertung:** Ein konkreter Migrationsweg ist erkennbar. Entscheidend sind die benötigten Integrationen und die Kompatibilität vorhandener Bücher beziehungsweise zusätzlicher DLLs. Die externen Dateieinbindungen der Projektdateien betreffen native DLLs; eingebundene externe C#-Quellen konnte ich dort nicht bestätigen.

Es wurden keine Dateien verändert und keine Builds ausgeführt. `planning_ready` bleibt `false`.

:::questions
{
  "questions": [
    {
      "id": "deployment_targets",
      "type": "text",
      "text": "Auf welchen Betriebssystemen und Geräten muss qBook nach dem Wechsel laufen, insbesondere Windows-Versionen und gegebenenfalls Raspberry Pi?",
      "required": true
    },
    {
      "id": "required_integrations",
      "type": "text",
      "text": "Welche Integrationen müssen erhalten bleiben, insbesondere Kamera, PicoScope, Datenbanken, HTML/CefSharp und Outlook?",
      "required": true
    },
    {
      "id": "book_compatibility",
      "type": "text",
      "text": "Müssen bestehende Bücher und Skripte unverändert laufen, und werden zusätzliche vorkompilierte .NET-Framework-DLLs geladen?",
      "required": true
    }
  ]
}
:::endquestions


## 2026-10-12T14:19:15.7878234+00:00 | User | 1651d145-aceb-4062-8b59-867759e3c7e9

Auf welchen Betriebssystemen und Geräten muss qBook nach dem Wechsel laufen, insbesondere Windows-Versionen und gegebenenfalls Raspberry Pi?: Windows only. 
aktuelle Windows Version
Welche Integrationen müssen erhalten bleiben, insbesondere Kamera, PicoScope, Datenbanken, HTML/CefSharp und Outlook?: Kamera, Datenbank
Müssen bestehende Bücher und Skripte unverändert laufen, und werden zusätzliche vorkompilierte .NET-Framework-DLLs geladen?: Idealerweise ja.
Im Ornder .DebugProject findest du aktuelle Projekte. Diese müssen lauffähig bleiben


## 2026-10-12T14:21:26.8290034+00:00 | Assistant | ff3c0a97-ba91-4e8d-9d49-331827128ae8

**Unter deinen Vorgaben ist .NET 10 ein realistisches Ziel. Vollständig unverändert können allerdings nicht alle vorhandenen Projekte weiterlaufen.**

Im Ordner `DebugProject` habe ich fünf aktuelle Projektstände geprüft: **AkbSps, Audi/CellTester, CalibrationStudioVT, Cast und GO83P05 mShedAK**.

Dabei zeigt sich:

- **Projektdateien:** Alle fünf deklarieren `net8.0`, referenzieren aber weiterhin .NET-Framework-Assemblies aus dem Windows-GAC. Diese Referenzen und die Projektgenerierung müssen umgestellt werden. Die Deklaration allein belegt keine .NET-8-Kompatibilität.
- **Skriptcode:** Die Sequencer `QS` und `ShedSystem` in `GO83P05 mShedAK` verwenden direkt `System.Threading.Thread.Abort()`. Unter .NET 10 wirft diese Methode eine Ausnahme. Für funktionierende Abbrüche sind dort Codeanpassungen erforderlich. [Microsoft-Dokumentation](https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.abort?view=net-10.0)
- **Datenbank:** Mehrere Projekte verwenden `QB.Logging.SqlLogger` mit SQLite. Für den verwendeten Anbieter existiert eine .NET-Standard-Variante. Damit ist ein Migrationsweg erkennbar, ohne automatisch das Datenbankformat wechseln zu müssen. Schreiben, Lesen und die benutzerdefinierte SQLite-Funktion müssen anschließend geprüft werden. [SQLite-Paket](https://www.nuget.org/packages/Stub.System.Data.SQLite.Core.NetStandard/1.0.119)
- **Kamera:** Die Projekte verwenden `CameraControl` auf Basis von AForge/DirectShow. Das eingebundene Paket weist .NET-Framework-Kompatibilität aus. Die Kameraanbindung bleibt deshalb ein offener technischer Prüfpunkt; ihre Funktionsfähigkeit unter .NET 10 ist bislang nicht nachgewiesen. [AForge-Paket](https://www.nuget.org/packages/AForge.Video.DirectShow/2.2.5)

**Meine Empfehlung:** Windows/x64 und WinForms beibehalten, .NET 10 direkt als Ziel bewerten und die fünf Projekte als verbindliche Kompatibilitätsfälle verwenden. Öffnen, Kompilieren, Ausführen, Stoppen sowie Kameraaufnahme und Datenbankzugriff müssen erhalten bleiben. Ein fehlerfreier Build allein genügt dafür nicht.

Die Einschätzung basiert auf Quellcode und Paketinformationen; es gab keine Änderungen oder Laufzeittests. `planning_ready` bleibt `false`, weil der zulässige Umfang der Kompatibilitätsanpassungen noch offen ist.

:::questions
{
  "questions": [
    {
      "id": "compatibility_adjustments",
      "type": "single_select",
      "text": "Welche Anpassungen sind mit der geforderten Lauffähigkeit bestehender Projekte vereinbar?",
      "options": [
        "Notwendige Skriptanpassungen und neu erzeugte Projektdateien sind zulässig; Funktionen und Daten bleiben erhalten.",
        "Die Projektquellen müssen unverändert bleiben; eine vollständige Portierung scheidet bei Inkompatibilitäten aus.",
        "Andere Vorgabe"
      ],
      "required": true
    },
    {
      "id": "compatibility_adjustments_other",
      "type": "text",
      "text": "Welche Kompatibilitätsvorgabe soll gelten?",
      "required": true,
      "visible_when": {
        "question_id": "compatibility_adjustments",
        "has_any_of": ["Andere Vorgabe"]
      }
    },
    {
      "id": "database_scope",
      "type": "text",
      "text": "Ist SQLite die benötigte Datenbank, oder müssen zusätzlich andere Datenbanksysteme unterstützt bleiben?",
      "required": true
    }
  ]
}
:::endquestions


## 2026-10-12T14:25:18.0280363+00:00 | User | b9c0044b-e593-436f-8d10-c4ed050eb40a

Welche Anpassungen sind mit der geforderten Lauffähigkeit bestehender Projekte vereinbar?: Notwendige Skriptanpassungen und neu erzeugte Projektdateien sind zulässig; Funktionen und Daten bleiben erhalten.
Ist SQLite die benötigte Datenbank, oder müssen zusätzlich andere Datenbanksysteme unterstützt bleiben?: Aktuell nur SQLite


## 2026-10-12T14:26:37.9247434+00:00 | Assistant | 0e35c86e-cd83-4e92-bf66-4b7a709bb4f5

**Mit diesen Vorgaben erscheint eine Portierung auf .NET 10 technisch machbar.** Die erlaubten Skriptanpassungen lösen den wesentlichen Konflikt mit den bestehenden Projekten.

Der sinnvolle Zielrahmen ist:

- Aktuelles Windows, x64, bestehende WinForms-Oberfläche.
- Alle fünf aktuellen Projekte aus `DebugProject` bleiben funktional erhalten.
- Notwendige Skriptänderungen und neu erzeugte Projektdateien sind zulässig.
- SQLite, vorhandene Daten und Kamerafunktionen bleiben erhalten.

Die wichtigsten Arbeiten wären die Umstellung der Projekt- und Paketreferenzen, die Anpassung der Roslyn-Kompilierung und das Ersetzen von `Thread.Abort()` in Bibliothek und betroffenen Skripten. Ein .NET-10-SDK ist lokal bereits installiert.

**SQLite besitzt einen erkennbaren Migrationsweg:** Der verwendete Anbieter bietet eine .NET-Standard-Variante. Ein Wechsel des Datenbankformats erscheint deshalb nicht erforderlich. [SQLite-Paket](https://www.nuget.org/packages/Stub.System.Data.SQLite.Core.NetStandard/1.0.119)

**Die Kameraanbindung ist die größte verbleibende Unsicherheit.** AForge enthält intern ebenfalls `Thread.Abort()`, das insbesondere im Kameradialog über `Stop()` erreicht werden kann. Die Anbindung muss angepasst oder ersetzt werden; OpenCvSharp ist bereits eingebunden und wäre ein zu prüfender Kandidat. Die tatsächliche Gerätekompatibilität bleibt durch Tests nachzuweisen. [AForge-Quellcode](https://github.com/andrewkirillov/AForge.NET/blob/master/Sources/Video.DirectShow/VideoCaptureDevice.cs)

Als Abnahmemaßstab sollten alle fünf Projekte öffnen, kompilieren, laufen und korrekt stoppen sowie Kameraaufnahme und SQLite-Zugriffe einschließlich vorhandener Daten funktionieren.

Das Ergebnis ist eine **positive Machbarkeitseinschätzung mit konkretem Anpassungsbedarf**, noch kein Laufzeitnachweis. Für eine konkrete Migrationsplanung bleiben Kamerastrategie und weitere Paketkompatibilität offen; `planning_ready` bleibt `false`. Nur Workflow-Metadaten wurden aktualisiert.


## 2026-10-12T14:29:29.3959300+00:00 | User | 50feda05-b8e6-4a7a-9762-0d7a2c1142c3

Gibt es für Thread.Abort() nicht noch eine Hintertür mit der man das wieder aktivieren kann?


## 2026-10-12T14:30:24.2653675+00:00 | Assistant | 21de3d36-622b-4821-9531-89ca5f740ea2

**Unter .NET 10 gibt es keinen Schalter, der das ursprüngliche `Thread.Abort()` wieder aktiviert.** Die Implementierung wirft unmittelbar `PlatformNotSupportedException`. Reflection oder das Unterdrücken der Compilerwarnung ändern daran nichts. [Quellcode von .NET 10](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Threading/Thread.cs#L431)

Über native Windows-Funktionen wie `TerminateThread` lässt sich ein Thread gewaltsam beenden. Das stellt jedoch die frühere .NET-Abbruchlogik nicht wieder her: Aufräumarbeiten entfallen, Sperren können bestehen bleiben und der Prozesszustand kann beschädigt werden. Für qBook mit Kamera- und Datenbankzugriffen wäre das keine tragfähige Lösung. [Microsoft-Dokumentation](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-terminatethread)

Für die Migration ergeben sich daher zwei unterschiedliche Anforderungen:

- **Kontrollierte Abläufe:** kooperativer Abbruch über `CancellationToken` und abbrechbare Warteoperationen.
- **Beliebiger Skriptcode, der auch bei einer Endlosschleife zwangsweise stoppbar sein muss:** Ausführung in einem separaten Prozess, der bei Bedarf beendet wird. [Microsoft-Empfehlung](https://learn.microsoft.com/en-us/dotnet/standard/threading/destroying-threads)

Gerade die zweite Anforderung wäre eine wesentliche Architekturentscheidung: Ein `CancellationToken` beendet nur Code, der den Abbruch auch berücksichtigt.


## 2026-10-12T14:34:09.4756143+00:00 | User | 931e2671-7af3-408b-b900-0b5f75f420b3

Ok dann bleiben wir beim sauberen Weg. Ich nehme den Migrationsaufwand in kauf.


## 2026-10-12T14:35:16.4583360+00:00 | Assistant | 0b6a0e90-7850-48a1-821e-f05b7d6e3a39

Damit ist **die Portierung auf .NET 10 mit notwendigen Anpassungen** als Ziel bestätigt. Windows, Kamera, SQLite sowie Funktionen und Daten der fünf Projekte aus `DebugProject` bleiben maßgeblich.

Eine wesentliche Entscheidung bleibt beim Skriptstopp: Die Skripte greifen derzeit direkt auf qBook-Objekte und die Oberfläche zu. **Ich empfehle, diese Architektur beizubehalten und kooperativen Abbruch einzuführen.** Schleifen und Warteoperationen werden dafür angepasst.

Soll hingegen auch fehlerhafter Skriptcode ohne Abbruchprüfung zwangsweise stoppbar sein, wäre eine separate Prozessausführung nötig. Das würde zusätzlich die Kommunikation zwischen Skripten, Oberfläche und Geräten verändern. [Microsoft-Empfehlung](https://learn.microsoft.com/en-us/dotnet/standard/threading/destroying-threads)

`planning_ready` bleibt bis zur Klärung dieser Anforderung und der verbleibenden Abhängigkeiten `false`.

:::questions
{
  "questions": [
    {
      "id": "script_stop_guarantee",
      "type": "single_select",
      "text": "Welche Garantie soll der Skriptstopp bieten?",
      "options": [
        "Kooperativer Abbruch im qBook-Prozess: Skripte werden angepasst und müssen Abbruchsignale berücksichtigen (empfohlen).",
        "Zwangsweiser Abbruch auch bei Endlosschleifen ohne Abbruchprüfung: separate Prozessausführung mit zusätzlichem Architekturaufwand.",
        "Andere Vorgabe"
      ],
      "required": true
    },
    {
      "id": "script_stop_guarantee_other",
      "type": "text",
      "text": "Welche Anforderungen soll der Skriptstopp erfüllen?",
      "required": true,
      "visible_when": {
        "question_id": "script_stop_guarantee",
        "has_any_of": ["Andere Vorgabe"]
      }
    }
  ]
}
:::endquestions


## 2026-10-12T14:35:44.4479054+00:00 | User | 164f4717-e439-4fd7-907d-065070a8effa

Welche Garantie soll der Skriptstopp bieten?: Kooperativer Abbruch im qBook-Prozess: Skripte werden angepasst und müssen Abbruchsignale berücksichtigen (empfohlen).


## 2026-10-12T14:37:44.6013814+00:00 | Assistant | eeb1f40f-3eef-4c98-92d1-7f65da1037e0

Der **kooperative Abbruch im qBook-Prozess** ist bestätigt. Skripte und Hintergrundoperationen berücksichtigen Abbruchsignale; eine separate Skriptprozess-Architektur entfällt.

Für die Kamera empfehle ich, **AForge durch das bereits eingebundene OpenCvSharp zu ersetzen**. `CameraControl` behält die benötigten Funktionen: Geräteauswahl, Start/Stopp, Livebild, Sichtbarkeit und Snapshots. Die tatsächliche Kamera muss anschließend geprüft werden.

Damit wäre die Migration abgegrenzt:

- `qbook`, `qbookCsScript`, Roslyn-Referenzen und Projektgenerierung auf `net10.0-windows`/x64 umstellen.
- Die fünf aktuellen Projekte aus `DebugProject` bei Bedarf anpassen; Funktionen und Daten erhalten.
- SQLite einschließlich vorhandener Datenbanken und Loggerfunktionen erhalten.
- Keine neue Oberfläche, zusätzliche Plattform oder zwangsweise Skriptbeendigung einführen.

Als Abnahme gelten ein vollständiger Build sowie Öffnen, Kompilieren, Ausführen, kooperatives Stoppen und erneutes Starten aller fünf Projekte. Hinzu kommen Kamera- und SQLite-Funktionsprüfungen. Bei `validation: true` werden das konkrete automatische Build-Ziel und die Ausführung über Worker ausdrücklich an PLAN delegiert.

`planning_ready` bleibt bis zur Entscheidung über die Kameraumstellung `false`.

:::questions
{
  "questions": [
    {
      "id": "camera_backend",
      "type": "single_select",
      "text": "Soll die Kameraaufnahme auf OpenCvSharp umgestellt werden, wobei die beschriebenen CameraControl-Funktionen erhalten bleiben?",
      "options": [
        "Ja, OpenCvSharp verwenden.",
        "Andere Vorgabe"
      ],
      "required": true
    },
    {
      "id": "camera_backend_other",
      "type": "text",
      "text": "Welche Vorgabe soll für die Kameraanbindung gelten?",
      "required": true,
      "visible_when": {
        "question_id": "camera_backend",
        "has_any_of": ["Andere Vorgabe"]
      }
    }
  ]
}
:::endquestions


## 2026-10-12T14:41:28.3453375+00:00 | User | 9a50dff8-498a-47ee-b2ce-dd63cd24753f

Soll die Kameraaufnahme auf OpenCvSharp umgestellt werden, wobei die beschriebenen CameraControl-Funktionen erhalten bleiben?: Ja, OpenCvSharp verwenden.


## 2026-10-12T14:42:46.7482977+00:00 | Assistant | 122773c8-acba-4136-8128-f4bc355f5be3

**Die Migration ist planungsreif.** Im aktiven `workitem.md` stehen jetzt `status: ready` und `planning_ready: true`.

Die festgelegte Lösung:

- **Ziel und Umfang:** `qbook`, `qbookCsScript`, Abhängigkeiten, Roslyn-Kompilierung und Projektgenerierung auf `net10.0-windows`/x64 umstellen. WinForms bleibt erhalten.
- **Skripte:** Ausführung weiterhin im qBook-Prozess mit kooperativem Abbruch. Notwendige Anpassungen an den fünf aktuellen Projekten aus `DebugProject` sind zulässig; Funktionen und Daten bleiben erhalten.
- **Kamera und Datenbank:** OpenCvSharp ersetzt AForge für die Kameraaufnahme. Geräteauswahl, Livebild, Start/Stopp und Snapshots bleiben erhalten. SQLite einschließlich vorhandener Daten und Loggerfunktionen bleibt bestehen.
- **Nichtziele:** Neue Oberfläche, zusätzliche Betriebssysteme, separate Skriptprozesse und erzwungener Threadabbruch. Die fünf geschützten `Amium.Items`-Dateien bleiben unverändert.

**Abnahme:** Alle fünf Projekte müssen öffnen, kompilieren, laufen, kooperativ stoppen und erneut starten. Kameraaufnahme und SQLite-Zugriffe müssen funktionieren; vorhandene Daten müssen weiter nutzbar sein.

**Validierung:** `validation: true`. Das konkrete automatische Validierungsziel und der exakte Build-Befehl werden an PLAN delegiert. Der vorhandene Visual-Studio-MSBuild-Task für `qbookStudio.sln` ist geprüft. Paketrestore bleibt benutzer- oder hostgesteuert. Der vorhandene Start-Eintrag `AiFlow Launch: qbook` muss für .NET 10 berücksichtigt werden.

**Offene wesentliche Punkte und verbleibende wesentliche PLAN-Entscheidungen: keine.** Die technische Funktionsfähigkeit wird während der Umsetzung geprüft.

Nur Workflow-Metadaten wurden geändert; Quellcode und Build-Ausführung bleiben dem nächsten Modus vorbehalten.


## 2026-10-12T14:43:54.0471329+00:00 | User | b27c89fd-1479-4885-bb8e-0bbe3c9eefa6

Plan


## 2026-10-12T14:51:20.4855933+00:00 | Assistant | d4a102e1-8907-45de-84ee-a64d0c7e5463

Der [Implementierungsplan](B:/x86.qBookRoslyn/.ai-flow/workitems/2026.10.12.1613-framework_wechsel_20261012_161310/handoffs/2026.10.12.1613-implementation-handoff.md) ist erstellt.

Er enthält Dateiumfang, Umsetzungsschritte, Schutzregeln, Abnahmekriterien sowie den verbindlichen Worker-Build und die Launch-Anpassung.

`planning_ready: true` bleibt erhalten. Nur das Handoff wurde geschrieben; Quellcodeänderungen und Builds wurden nicht ausgeführt.