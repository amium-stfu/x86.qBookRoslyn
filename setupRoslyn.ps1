
<# 
.SYNOPSIS
    Baut qbook (Release) via MSBuild, published qbookCode, kopiert Outputs nach .\Setup\bin\qbook und .\Setup\bin\qbookCode.

.NOTES
    - Nutzt msbuild.exe, wenn verfügbar, sonst dotnet msbuild.
    - Sucht automatisch das Output-Verzeichnis (TFM-agnostisch).
#>

param(
    [string]$ProjectRelPathQbook     = ".\qbook\qbook.csproj",
    [string]$ProjectRelPathQbookCode = ".\qbookCode\qbookCode.csproj",
    [string]$DestQbook               = ".\Setup\bin\qbook",
    [string]$DestQbookCode           = ".\Setup\bin\qbookCode",
    [string]$Configuration           = "Release"
)

# --- Helper: Write info with timestamp
function Write-Info($msg) {
    Write-Host ("[{0}] {1}" -f (Get-Date).ToString("yyyy-MM-dd HH:mm:ss"), $msg) -ForegroundColor Cyan
}
function Write-Ok($msg) {
    Write-Host ("✅ {0}" -f $msg) -ForegroundColor Green
}
function Write-Err($msg) {
    Write-Host ("❌ {0}" -f $msg) -ForegroundColor Red
}

# --- Helper: resolve an MSBuild command
function Get-MSBuild {
    # 1) Try msbuild.exe from VS (common locations)
    $candidatePaths = @(
        "$env:ProgramFiles\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "$env:ProgramFiles\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "$env:ProgramFiles(x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "$env:ProgramFiles(x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "$env:ProgramFiles(x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    ) | Where-Object { Test-Path $_ }

    if ($candidatePaths.Count -gt 0) {
        return $candidatePaths[0]
    }

    # 2) Fallback: dotnet msbuild
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnet) {
        return "dotnet msbuild"
    }

    throw "Kein MSBuild gefunden (weder msbuild.exe noch dotnet). Bitte Visual Studio oder .NET SDK installieren."
}

# --- Helper: run a process and fail on non-zero exit
function Invoke-Tool {
    param(
        [Parameter(Mandatory=$true)][string]$FilePathOrCommand,
        [Parameter(Mandatory=$true)][string[]]$Arguments
    )

    Write-Info ("Starte: {0} {1}" -f $FilePathOrCommand, ($Arguments -join ' '))

    if ($FilePathOrCommand -eq "dotnet msbuild") {
        # dotnet msbuild als einzelner String + args
        & dotnet msbuild @Arguments
        $exitCode = $LASTEXITCODE
    } else {
        & $FilePathOrCommand @Arguments
        $exitCode = $LASTEXITCODE
    }

    if ($exitCode -ne 0) {
        throw "Befehl fehlgeschlagen (ExitCode=$exitCode): $FilePathOrCommand $($Arguments -join ' ')"
    }
    Write-Ok "Fertig: $FilePathOrCommand"
}

# --- Helper: find build output directory for Build target (bin\Release\<TFM>\)
function Find-BuildOutput {
    param(
        [Parameter(Mandatory=$true)][string]$ProjectPath,
        [Parameter(Mandatory=$true)][string]$Configuration
    )
    $projDir = Split-Path -Path $ProjectPath -Parent
    $binDir  = Join-Path $projDir "bin"
    if (!(Test-Path $binDir)) { return $null }

    $confDir = Join-Path $binDir $Configuration
    if (!(Test-Path $confDir)) { return $null }

    # Wähle den tiefsten TFM-Ordner (z.B. net8.0), bevorzugt mit Dateien
    $tfmDirs = Get-ChildItem $confDir -Directory -ErrorAction SilentlyContinue
    if ($tfmDirs.Count -eq 0) { return $confDir } # Falls kein TFM unterordner (ältere Projekte)

    # Bevorzuge Ordner mit Dateien
    $best = $tfmDirs | Sort-Object {
        (Get-ChildItem $_.FullName -Recurse -ErrorAction SilentlyContinue | Measure-Object).Count
    } -Descending | Select-Object -First 1

    return $best.FullName
}

# --- Helper: find publish output directory (bin\Release\<TFM>\publish)
function Find-PublishOutput {
    param(
        [Parameter(Mandatory=$true)][string]$ProjectPath,
        [Parameter(Mandatory=$true)][string]$Configuration
    )
    $buildBase = Find-BuildOutput -ProjectPath $ProjectPath -Configuration $Configuration
    if ($null -eq $buildBase) { return $null }

    $pubDir = Join-Path $buildBase "publish"
    if (Test-Path $pubDir) { return $pubDir }

    # Fallback: direkt im conf dir nach publish suchen
    $projDir = Split-Path -Path $ProjectPath -Parent
    $confDir = Join-Path (Join-Path $projDir "bin") $Configuration
    $pubDirs = Get-ChildItem $confDir -Recurse -Directory -Filter "publish" -ErrorAction SilentlyContinue
    if ($pubDirs.Count -gt 0) { return $pubDirs[0].FullName }

    return $null
}

# --- Helper: clean and copy directory
function Copy-DirectoryClean {
    param(
        [Parameter(Mandatory=$true)][string]$SourceDir,
        [Parameter(Mandatory=$true)][string]$DestDir
    )
    Write-Info "Kopiere von '$SourceDir' nach '$DestDir' ..."
    if (Test-Path $DestDir) {
        Remove-Item -Path $DestDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
    Copy-Item -Path (Join-Path $SourceDir "*") -Destination $DestDir -Recurse -Force
    Write-Ok "Kopieren abgeschlossen."
}

# --- MAIN ---
try {
    $msbuild = Get-MSBuild
    Write-Info "MSBuild: $msbuild"

    # 1) Build qbook (Release)
    $argsBuildQbook = @(
        $ProjectRelPathQbook,
        "/t:Build",
        "/p:Configuration=$Configuration",
        "/verbosity:minimal"
    )
    Invoke-Tool -FilePathOrCommand $msbuild -Arguments $argsBuildQbook

    $qbookOut = Find-BuildOutput -ProjectPath $ProjectRelPathQbook -Configuration $Configuration
    if ($null -eq $qbookOut) {
        throw "Build-Output-Verzeichnis für qbook nicht gefunden."
    }
    Write-Info "qbook Output: $qbookOut"

    # 2) Publish qbookCode (Release)
    $argsPublishQbookCode = @(
        $ProjectRelPathQbookCode,
        "/t:Publish",
        "/p:Configuration=$Configuration",
        "/p:RuntimeIdentifier=win-x86",
        "/p:SelfContained=true"
        "/verbosity:minimal"
        # Optional: Runtime, SingleFile, Trim, SelfContained usw. können hier ergänzt werden.
        # Beispiel: "/p:PublishSingleFile=true", "/p:RuntimeIdentifier=win-x64"
    )

    Invoke-Tool -FilePathOrCommand $msbuild -Arguments $argsPublishQbookCode

    $qbookCodeOut = Find-PublishOutput -ProjectPath $ProjectRelPathQbookCode -Configuration $Configuration
    if ($null -eq $qbookCodeOut) {
        throw "Publish-Output-Verzeichnis für qbookCode nicht gefunden."
    }
    Write-Info "qbookCode Publish Output: $qbookCodeOut"

    # 3) Kopieren
    Copy-DirectoryClean -SourceDir $qbookOut      -DestDir $DestQbook
    Copy-DirectoryClean -SourceDir $qbookCodeOut  -DestDir $DestQbookCode

    Write-Ok "Alles erledigt. Artefakte liegen in:"
    Write-Host " - $DestQbook"     -ForegroundColor Yellow
    Write-Host " - $DestQbookCode" -ForegroundColor Yellow
}
catch {
    Write-Err $_
    exit 1
}
