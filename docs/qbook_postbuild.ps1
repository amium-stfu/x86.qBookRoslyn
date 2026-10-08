param(
    [string]$SolutionDir,
    [string]$Configuration = "Debug"
)

# --- Initialisierung ---
if ([string]::IsNullOrWhiteSpace($SolutionDir)) {
    $SolutionDir = (Get-Location).Path
} else {
    $SolutionDir = $SolutionDir.TrimEnd('\','"')
}

$ErrorActionPreference = "Stop"
$targetBase = Join-Path $SolutionDir "qbook\bin\$Configuration"
$logFile = Join-Path $targetBase "postbuild_ps.log"

function Log($msg) {
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    "$timestamp $msg" | Out-File -Append $logFile -Encoding UTF8
}

Log "PostBuild started"
$errors = @()

# --- Verzeichnisse ---
$dirs = @(
    "$targetBase\libs",
    "$targetBase\libs.cef",
    "$targetBase\libs.cef\x86",
    "$targetBase\libs.cef\x64",
    "$targetBase\libs.cef\dll",
    "$targetBase\libs.cef\locales",
    "$targetBase\libs\x86",
    "$targetBase\libs\x64"
)

foreach ($d in $dirs) {
    if (-not (Test-Path $d)) {
        try {
            New-Item -ItemType Directory -Path $d | Out-Null
            Log "Created directory: $d"
        } catch {
            $errors += "Failed to create directory: $d"
        }
    }
}

# --- Hilfsfunktion für Move ---
function SafeMove($src, $dest) {
    try {
        if (Test-Path $src) {
            Move-Item -Path $src -Destination $dest -Force
            Log "Moved: $src -> $dest"
        }
    } catch {
        $errors += "Failed to move $src to $dest"
    }
}

# --- CEF-Dateien verschieben ---
$cefFiles = @(
    "CefSharp*.*","libcef.dll","libEGL.dll","libGLESv2.dll","chrome.elf",
    "chrome_100_percent.pak","chrome_200_percent.pak","resources.pak",
    "vk_swiftshader.dll","vk_swiftshader_icd.json","vulkan-1.dll",
    "OpenCvSharp*.*","d3dcompiler_47.dll"
)

foreach ($pattern in $cefFiles) {
    Get-ChildItem "$targetBase" -Filter $pattern -ErrorAction SilentlyContinue | ForEach-Object {
        SafeMove $_.FullName "$targetBase\libs.cef"
    }
}

# --- Unterordner x86/x64/dll/locales ---
foreach ($sub in @("x86","x64","dll","locales")) {
    $srcDir = Join-Path $targetBase $sub
    $destDir = Join-Path "$targetBase\libs.cef" $sub
    if (Test-Path $srcDir) {
        Get-ChildItem $srcDir -Recurse | ForEach-Object {
            SafeMove $_.FullName $destDir
        }
        try { Remove-Item $srcDir -Recurse -Force } catch {}
    }
}

# --- SQLite.Interop.dll verschieben ---
SafeMove "$targetBase\libs.cef\x86\SQLite.Interop.dll" "$targetBase\libs\x86"
SafeMove "$targetBase\libs.cef\x64\SQLite.Interop.dll" "$targetBase\libs\x64"

# --- Allgemeine Dateien nach libs ---
$exclude = @("*.exe","qbookStudio.pdb","qbookStudio.exe.config","log4net.config","*.manifest","*.pdf","qbookStudio.application")
$excludeDirs = @("libs","libs.cef","assets","CsSnippets","dnSpy","html","qbooks","qbooks.backup")

Get-ChildItem $targetBase -Recurse | Where-Object {
    ($exclude -notcontains $_.Name) -and ($excludeDirs -notcontains $_.Directory.Name)
} | ForEach-Object {
    SafeMove $_.FullName "$targetBase\libs"
}

# --- DLLs aus Runtime kopieren ---
$RuntimeDir = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory
$requiredDlls = @("System.dll","System.Core.dll","System.Data.dll","System.Xml.dll","System.Net.Http.dll","System.Runtime.dll","System.Linq.dll","System.Collections.dll")
foreach ($dll in $requiredDlls) {
    $src = Join-Path $RuntimeDir $dll
    if (Test-Path $src) {
        try {
            Copy-Item $src -Destination "$targetBase\libs" -Force
            Log "Copied: $dll"
        } catch {
            $errors += "Failed to copy $dll"
        }
    }
}

# --- Abschluss ---
if ($errors.Count -eq 0) {
    Log "PostBuild completed successfully"
    Write-Host " PostBuild erfolgreich"
} else {
    Log "PostBuild completed with errors:"
    $errors | ForEach-Object { Log $_ }
    Write-Host "PostBuild Fehler: $($errors.Count) -> siehe Logfile"
    Start-Process notepad.exe $logFile
}