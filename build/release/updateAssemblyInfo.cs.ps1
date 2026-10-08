param(
    [string]$Configuration = "Debug",
    [string]$OutputPath = $(Join-Path $PSScriptRoot "obj\GeneratedAssemblyVersionInfo.cs"),
    [string]$BaseFileVersion = "0.3.0.2",
    [string]$NugetRevisionPath
)

$ErrorActionPreference = "Stop"
if ($Configuration -ieq "Release") {
    if ([string]::IsNullOrWhiteSpace($NugetRevisionPath) -or -not (Test-Path -LiteralPath $NugetRevisionPath)) { throw "NuGet revision state is required for a Release build." }
    try { $revisionState = Get-Content -Raw -LiteralPath $NugetRevisionPath | ConvertFrom-Json } catch { throw "NuGet revision state is malformed or incomplete." }
    if ($null -eq $revisionState -or $revisionState.schemaVersion -ne 1 -or $revisionState.majorRevision -notmatch '^\d+$' -or $revisionState.featureRevision -notmatch '^\d+$' -or $revisionState.fossRevision -notmatch '^\d+$' -or $revisionState.buildId -notmatch '^\d+$' -or $revisionState.fingerprint -notmatch '^[0-9a-f]{64}$' -or $null -eq $revisionState.components) { throw "NuGet revision state is malformed or incomplete." }
    try { $majorRevision = [int64]$revisionState.majorRevision; $featureRevision = [int64]$revisionState.featureRevision; $fossRevision = [int64]$revisionState.fossRevision } catch { throw "NuGet revision state is malformed or incomplete." }
    if ($majorRevision -gt 65535 -or $featureRevision -gt 65535 -or $fossRevision -gt 65535) { throw "MajorRevision, FeatureRevision, and FossRevision must be between 0 and 65535." }
    $releaseDate = [DateTime]::Now
    $buildVersion = $releaseDate.ToString('yy') + $releaseDate.DayOfYear.ToString('000')
    $assemblyVersion = "$majorRevision.$featureRevision.$fossRevision.$buildVersion"
    $fileVersion = $assemblyVersion
    $informationalVersion = $assemblyVersion
} else { $assemblyVersion = $BaseFileVersion; $fileVersion = $BaseFileVersion; $informationalVersion = $assemblyVersion }
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
