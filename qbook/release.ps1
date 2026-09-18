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

$msbuildPath = (& $vswherePath -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($msbuildPath) -or -not (Test-Path -LiteralPath $msbuildPath -PathType Leaf)) { throw "MSBuild.exe was not found in Visual Studio Build Tools." }

& $msbuildPath $solutionPath /m /t:Rebuild /p:Configuration=Release /p:Platform=x86 /p:BuildRevision=$BuildRevision
if ($LASTEXITCODE -ne 0) { throw "qbook Release build failed." }

Write-Host "qbook Release build completed with BuildRevision $BuildRevision."
