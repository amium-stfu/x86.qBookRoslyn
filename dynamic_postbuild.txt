
param(
    [string]$SolutionDir,
    [string]$Configuration = "Debug"
)

if ([string]::IsNullOrWhiteSpace($SolutionDir)) {
    $SolutionDir = (Get-Location).Path
} else {
    $SolutionDir = $SolutionDir.TrimEnd('\','"')
}

$ErrorActionPreference = "Stop"
$RuntimeDir = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()


$ErrorActionPreference = "Stop"

# Zielverzeichnis für DLLs
$targetLibs = Join-Path $SolutionDir "qbook\\bin\\$Configuration\\libs"
if (!(Test-Path $targetLibs)) {
    New-Item -ItemType Directory -Path $targetLibs | Out-Null
}

# Log-Datei
$logFile = Join-Path $targetLibs "postbuild_ps.log"
function Log($msg) {
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    "$timestamp $msg" | Out-File -Append $logFile
}

Log "PostBuild started"
Log "RuntimeDir: $RuntimeDir"

# Funktion zur Prüfung, ob eine DLL verwaltet ist (PE-Header-Check)

function IsManagedAssembly($path) {
    try {
        [System.Reflection.AssemblyName]::GetAssemblyName($path) | Out-Null
        return $true
    } catch {
        return $false
    }
}


# Alle DLLs aus RuntimeDir prüfen und kopieren

$requiredDlls = @(
    "System.dll",
    "System.Core.dll",
    "System.Data.dll",
    "System.Xml.dll",
    "System.Net.Http.dll",
    "System.Runtime.dll",
    "System.Linq.dll",
    "System.Collections.dll"
)
foreach ($dll in $requiredDlls) {
    $src = Join-Path $RuntimeDir $dll
    if (Test-Path $src) {
        Copy-Item $src -Destination $targetLibs -Force
        Log "Copied: $dll"
    }
}


Log "PostBuild completed successfully"
notepad $logFile
