param(
    [Parameter(Mandatory = $true)]
    [string]$RevisionStatePath,
    [Parameter(Mandatory = $true)]
    [string]$ReleaseOutputDirectory
)

$ErrorActionPreference = "Stop"

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$setupDirectory = Join-Path $repositoryRoot "Setup"
$buildInfoPath = Join-Path $ReleaseOutputDirectory "BuildInfo.md"
$sevenZipPath = Join-Path $repositoryRoot "7z.exe"
$sevenZipLibraryPath = Join-Path $repositoryRoot "7z.dll"

if (-not (Test-Path -LiteralPath $RevisionStatePath -PathType Leaf)) { throw "NuGet revision state was not found: $RevisionStatePath" }
if (-not (Test-Path -LiteralPath $ReleaseOutputDirectory -PathType Container)) { throw "Release output directory was not found: $ReleaseOutputDirectory" }
if (-not (Test-Path -LiteralPath $buildInfoPath -PathType Leaf)) { throw "BuildInfo.md was not found: $buildInfoPath" }
if (-not (Test-Path -LiteralPath $sevenZipPath -PathType Leaf)) { throw "7z.exe was not found: $sevenZipPath" }
if (-not (Test-Path -LiteralPath $sevenZipLibraryPath -PathType Leaf)) { throw "7z.dll was not found: $sevenZipLibraryPath" }

try { $revisionState = Get-Content -Raw -LiteralPath $RevisionStatePath | ConvertFrom-Json } catch { throw "NuGet revision state is malformed or incomplete." }
if ($null -eq $revisionState -or $revisionState.schemaVersion -ne 1 -or $revisionState.majorRevision -notmatch '^\d+$' -or $revisionState.featureRevision -notmatch '^\d+$' -or $revisionState.fossRevision -notmatch '^\d+$') { throw "NuGet revision state is malformed or incomplete." }

try { $majorRevision = [int64]$revisionState.majorRevision; $featureRevision = [int64]$revisionState.featureRevision; $fossRevision = [int64]$revisionState.fossRevision } catch { throw "NuGet revision state is malformed or incomplete." }
if ($majorRevision -gt 65535 -or $featureRevision -gt 65535 -or $fossRevision -gt 65535) { throw "MajorRevision, FeatureRevision, and FossRevision must be between 0 and 65535." }

$releaseDate = [DateTime]::Now
$buildVersion = $releaseDate.ToString('yy') + $releaseDate.DayOfYear.ToString('000')
$releaseVersion = "$majorRevision.$featureRevision.$fossRevision.$buildVersion"
$archivePath = Join-Path $setupDirectory "qbook_$releaseVersion.zip"

[IO.Directory]::CreateDirectory($setupDirectory) | Out-Null
Copy-Item -LiteralPath $RevisionStatePath -Destination (Join-Path $setupDirectory "NugetRevision.json") -Force
Copy-Item -LiteralPath $buildInfoPath -Destination (Join-Path $setupDirectory "BuildInfo.md") -Force

& $sevenZipPath a -tzip -mx=9 -y $archivePath (Join-Path $ReleaseOutputDirectory "*")
if ($LASTEXITCODE -ne 0) { throw "7z.exe failed to create the release archive." }

Write-Host "Created release archive: $archivePath"
