param(
    [string]$Configuration = "Debug",
    [string]$OutputPath = $(Join-Path $PSScriptRoot "obj\GeneratedAssemblyVersionInfo.cs"),
    [string]$BaseFileVersion = "0.3.0.2",
    [string]$FossStatePath,
    [string]$BuildRevision
)

$ErrorActionPreference = "Stop"
if ($Configuration -ieq "Release") {
    if ([string]::IsNullOrWhiteSpace($FossStatePath) -or -not (Test-Path -LiteralPath $FossStatePath)) { throw "Validated FOSS build state is required for a Release build." }
    if ($BuildRevision -notmatch '^\d+$') { throw "BuildRevision must be a numeric value for a Release build." }
    $fossState = Get-Content -Raw -LiteralPath $FossStatePath | ConvertFrom-Json
    if ($null -eq $fossState -or $fossState.fossRevision -notmatch '^\d+$' -or $fossState.fingerprint -notmatch '^[0-9a-f]{64}$' -or [string]::IsNullOrWhiteSpace($fossState.manifestPath)) { throw "Validated FOSS build state is malformed." }
    $fossRevision = [int64]$fossState.fossRevision; $buildRevisionNumber = [int64]$BuildRevision
    if ($fossRevision -gt 65535 -or $buildRevisionNumber -gt 65535) { throw "FossRevision and BuildRevision must be between 0 and 65535." }
    $year = [DateTime]::UtcNow.ToString("yy")
    $assemblyVersion = "$year.$fossRevision.$buildRevisionNumber.0"; $fileVersion = $assemblyVersion
    $informationalVersion = "{0}.{1:D2}.{2:D5}" -f $year, $fossRevision, $buildRevisionNumber
} else { $assemblyVersion = $BaseFileVersion; $fileVersion = $BaseFileVersion; $informationalVersion = $assemblyVersion }
try { $commitId = (& git -C $PSScriptRoot rev-parse --short HEAD 2>$null).Trim(); if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($commitId)) { $informationalVersion = "$informationalVersion+$commitId" } } catch { }
$outputDirectory = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) { [IO.Directory]::CreateDirectory($outputDirectory) | Out-Null }
$generatedContent = @"
using System.Reflection;

[assembly: AssemblyVersion("$assemblyVersion")]
[assembly: AssemblyFileVersion("$fileVersion")]
[assembly: AssemblyInformationalVersion("$informationalVersion")]
"@
[IO.File]::WriteAllText($OutputPath, $generatedContent, (New-Object Text.UTF8Encoding($false)))
Write-Host "Generated assembly version info at $OutputPath for configuration $Configuration"
