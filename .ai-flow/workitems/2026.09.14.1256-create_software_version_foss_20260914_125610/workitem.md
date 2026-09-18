---
id: 2026.09.14.1256-create_software_version_foss_20260914_125610
title: Create Software Version FOSS - 20260914-125610
owner_user_id: StefanFunk
status: failed
branch: main
updated_at: 2026-09-18T10:14:18+02:00
progress: Replaced fragile GitHub Actions Visual Studio MSBuild discovery in qbook/release.ps1 with installation-path-based resolution for MSBuild\\Current\\Bin\\MSBuild.exe and an MSBuild\\17.0 fallback. Required no-restore verification reached the Release projects but failed at the legacy ResolveComReference task, which .NET Core MSBuild cannot execute; GitHub runner confirmation remains required.
next_step: Run the manually dispatched GitHub Release workflow to confirm Visual Studio MSBuild discovery.
planning_ready: false
ai_mode: false
---
