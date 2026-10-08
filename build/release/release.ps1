$ErrorActionPreference = "Stop"

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$solutionPath = Join-Path $repositoryRoot "qbookStudio.sln"
$revisionStatePath = Join-Path $repositoryRoot "NugetRevision.json"
$vswherePath = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) { throw "qbookStudio.sln was not found." }
if (-not (Test-Path -LiteralPath $revisionStatePath -PathType Leaf)) { throw "NuGet revision state was not found: $revisionStatePath" }
if (-not (Test-Path -LiteralPath $vswherePath -PathType Leaf)) { throw "Visual Studio Build Tools were not found." }

try { $revisionState = Get-Content -Raw -LiteralPath $revisionStatePath | ConvertFrom-Json } catch { throw "NuGet revision state is malformed or incomplete." }
if ($null -eq $revisionState -or $revisionState.schemaVersion -ne 1 -or $revisionState.majorRevision -notmatch '^\d+$' -or $revisionState.featureRevision -notmatch '^\d+$' -or $revisionState.fossRevision -notmatch '^\d+$' -or $revisionState.buildId -notmatch '^\d+$' -or $revisionState.fingerprint -notmatch '^[0-9a-f]{64}$' -or $null -eq $revisionState.components) { throw "NuGet revision state is malformed or incomplete." }

function Get-RevisionValue([string]$Name, [object]$CurrentValue) {
    try { $currentRevision = [int64]$CurrentValue } catch { throw "NuGet revision state is malformed or incomplete." }
    if ($currentRevision -lt 0 -or $currentRevision -gt 65535) { throw "$Name revision must be between 0 and 65535." }

    $answer = Read-Host "Change $($Name.ToLowerInvariant()) revision? (y/n)"
    if ($answer -eq 'n') { return $currentRevision }
    if ($answer -ne 'y') { throw "Invalid answer. Enter y or n." }

    $replacement = Read-Host "Enter replacement $($Name.ToLowerInvariant()) revision"
    if ($replacement -notmatch '^\d+$') { throw "$Name revision must be a non-negative integer not greater than 65535." }
    try { $replacementValue = [int64]$replacement } catch { throw "$Name revision must be a non-negative integer not greater than 65535." }
    if ($replacementValue -gt 65535) { throw "$Name revision must be a non-negative integer not greater than 65535." }
    return $replacementValue
}

$majorRevision = Get-RevisionValue "Major" $revisionState.majorRevision
$featureRevision = Get-RevisionValue "Feature" $revisionState.featureRevision

& (Join-Path $PSScriptRoot "updateFossState.ps1") -RevisionStatePath $revisionStatePath -MajorRevision $majorRevision -FeatureRevision $featureRevision
if (-not $?) { throw "Release state update failed." }

$vswhereJson = & $vswherePath -all -prerelease -products * -format json
if ($LASTEXITCODE -ne 0) { throw "vswhere.exe failed to enumerate Visual Studio installations." }

$visualStudioInstances = @($vswhereJson | ConvertFrom-Json)
if ($visualStudioInstances.Count -eq 0) { throw "A Visual Studio installation was not found. vswhere.exe reported no installations (including prerelease and incomplete products)." }

$msbuildPath = $null
foreach ($instance in ($visualStudioInstances | Sort-Object -Property installationVersion -Descending)) {
    if ([string]::IsNullOrWhiteSpace($instance.installationPath) -or -not (Test-Path -LiteralPath $instance.installationPath -PathType Container)) { continue }

    $candidate = @(
        (Join-Path $instance.installationPath "MSBuild\Current\Bin\MSBuild.exe"),
        (Join-Path $instance.installationPath "MSBuild\17.0\Bin\MSBuild.exe")
    ) | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1

    if (-not [string]::IsNullOrWhiteSpace($candidate)) { $msbuildPath = $candidate; break }
}

if ([string]::IsNullOrWhiteSpace($msbuildPath)) {
    $foundInstallations = ($visualStudioInstances | ForEach-Object { "$($_.displayName) $($_.installationVersion) at $($_.installationPath)" }) -join "; "
    throw "MSBuild.exe was not found in any discovered Visual Studio installation. Discovered installations: $foundInstallations"
}

& $msbuildPath $solutionPath /m /t:Rebuild /p:Configuration=Release /p:Platform=x86
if ($LASTEXITCODE -ne 0) { throw "qbook Release build failed." }

& (Join-Path $PSScriptRoot "updateBuildInfo.ps1") -RevisionStatePath $revisionStatePath -OutputDirectory (Join-Path $repositoryRoot "Setup\Release\bin\qbook")
if (-not $?) { throw "BuildInfo generation failed." }

& (Join-Path $PSScriptRoot "packageRelease.ps1") -RevisionStatePath $revisionStatePath -ReleaseOutputDirectory (Join-Path $repositoryRoot "Setup\Release\bin\qbook")
if (-not $?) { throw "Release packaging failed." }

Write-Host "qbook Release build completed."
