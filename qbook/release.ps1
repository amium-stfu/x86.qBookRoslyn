param(
    [Parameter(Mandatory = $true)]
    [string]$BuildRevision
)

$ErrorActionPreference = "Stop"

if ($BuildRevision -notmatch '^[1-9]\d*$') { throw "BuildRevision must be a numeric value between 1 and 65535." }

try { $buildRevisionNumber = [int64]$BuildRevision } catch { throw "BuildRevision must be a numeric value between 1 and 65535." }
if ($buildRevisionNumber -gt 65535) { throw "BuildRevision must be a numeric value between 1 and 65535." }

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$solutionPath = Join-Path $repositoryRoot "qbookStudio.sln"
$vswherePath = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) { throw "qbookStudio.sln was not found." }
if (-not (Test-Path -LiteralPath $vswherePath -PathType Leaf)) { throw "Visual Studio Build Tools were not found." }

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

& $msbuildPath $solutionPath /m /t:Rebuild /p:Configuration=Release /p:Platform=x86 /p:BuildRevision=$BuildRevision
if ($LASTEXITCODE -ne 0) { throw "qbook Release build failed." }

Write-Host "qbook Release build completed with BuildRevision $BuildRevision."
