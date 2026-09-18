---
id: 2026.09.14.1256-create_software_version_foss_20260914_125610
title: Create Software Version FOSS - 20260914-125610
owner_user_id: StefanFunk
status: implemented
branch: main
updated_at: 2026-09-18T10:06:44+02:00
progress: Confirmed the manual GitHub Release process: select the target ref, start the Release qbook workflow, and download its successful artifact. The workflow supplies github.run_number automatically as BuildRevision. The numeric version remains yy.FossRevision.BuildRevision.0; its fixed fourth component can remain because GitHub run numbers provide the required monotonic build identity.
next_step: Report may be started explicitly.
planning_ready: false
ai_mode: false
---
