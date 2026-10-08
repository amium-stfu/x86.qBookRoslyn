param(
    [Parameter(Mandatory = $true)]
    [string]$RevisionStatePath,
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $RevisionStatePath -PathType Leaf)) { throw "NuGet revision state was not found: $RevisionStatePath" }
if (-not (Test-Path -LiteralPath $OutputDirectory -PathType Container)) { throw "Release output directory was not found: $OutputDirectory" }

try { $revisionState = Get-Content -Raw -LiteralPath $RevisionStatePath | ConvertFrom-Json } catch { throw "NuGet revision state is malformed or incomplete." }
if ($null -eq $revisionState -or $revisionState.schemaVersion -ne 1 -or $revisionState.majorRevision -notmatch '^\d+$' -or $revisionState.featureRevision -notmatch '^\d+$' -or $revisionState.fossRevision -notmatch '^\d+$' -or $revisionState.buildId -notmatch '^\d+$' -or $revisionState.fingerprint -notmatch '^[0-9a-f]{64}$') { throw "NuGet revision state is malformed or incomplete." }

try { $majorRevision = [int64]$revisionState.majorRevision; $featureRevision = [int64]$revisionState.featureRevision; $fossRevision = [int64]$revisionState.fossRevision; $buildId = [int64]$revisionState.buildId } catch { throw "NuGet revision state is malformed or incomplete." }
if ($majorRevision -gt 65535 -or $featureRevision -gt 65535 -or $fossRevision -gt 65535) { throw "MajorRevision, FeatureRevision, and FossRevision must be between 0 and 65535." }

$releaseDate = [DateTime]::Now
$buildVersion = $releaseDate.ToString('yy') + $releaseDate.DayOfYear.ToString('000')
$releaseVersion = "$majorRevision.$featureRevision.$fossRevision.$buildVersion"
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$commitId = (& git -C $repositoryRoot rev-parse HEAD 2>$null).Trim()
if ($LASTEXITCODE -ne 0 -or $commitId -notmatch '^[0-9a-f]{40}$') { throw "The current source commit could not be obtained." }

$content = @"
# qbook Build Information

- Release version: $releaseVersion
- Generated at (UTC): $([DateTime]::UtcNow.ToString('o'))
- Source commit: $commitId
- FOSS revision: $fossRevision
- FOSS fingerprint: $($revisionState.fingerprint)
- Build ID: $buildId
"@

[IO.File]::WriteAllText((Join-Path $OutputDirectory "BuildInfo.md"), $content.Replace("`r`n", "`n"), (New-Object Text.UTF8Encoding($false)))
Write-Host "Generated BuildInfo.md in $OutputDirectory"
