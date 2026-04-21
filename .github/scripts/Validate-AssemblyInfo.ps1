param(
    [string]$AssemblyInfoPath = "qbook/Properties/AssemblyInfo.cs",
    [string]$ProjectPath = "qbook/qbook.csproj",
    [string]$VersionScriptPath = "qbook/updateAssemblyInfo.cs.ps1"
)

if (-not (Test-Path -Path $AssemblyInfoPath)) {
    throw "AssemblyInfo file was not found: $AssemblyInfoPath"
}

$content = Get-Content -Path $AssemblyInfoPath -Raw

if ([string]::IsNullOrWhiteSpace($content)) {
    throw "AssemblyInfo file is empty: $AssemblyInfoPath"
}

$requiredMarkers = @(
    'using System.Reflection;',
    '[assembly: AssemblyTitle("qbook")]',
    '[assembly: Guid("364f8cb2-a7ef-4f78-a515-ba8108aabfff")]',
    '[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]'
)

$missingMarkers = $requiredMarkers | Where-Object { -not $content.Contains($_) }

if ($missingMarkers.Count -gt 0) {
    $missingList = $missingMarkers -join ', '
    throw "AssemblyInfo file is missing required content: $missingList"
}

$forbiddenMarkers = @(
    '[assembly: AssemblyVersion(',
    '[assembly: AssemblyFileVersion('
)

$presentForbiddenMarkers = $forbiddenMarkers | Where-Object { $content.Contains($_) }

if ($presentForbiddenMarkers.Count -gt 0) {
    $presentList = $presentForbiddenMarkers -join ', '
    throw "AssemblyInfo file still contains generated version attributes: $presentList"
}

if (-not (Test-Path -Path $ProjectPath)) {
    throw "Project file was not found: $ProjectPath"
}

if (-not (Test-Path -Path $VersionScriptPath)) {
    throw "Version generation script was not found: $VersionScriptPath"
}

$projectContent = Get-Content -Path $ProjectPath -Raw

$requiredProjectMarkers = @(
    'GenerateAssemblyVersionInfo',
    'updateAssemblyInfo.cs.ps1',
    'GeneratedAssemblyVersionInfo.cs'
)

$missingProjectMarkers = $requiredProjectMarkers | Where-Object { -not $projectContent.Contains($_) }

if ($missingProjectMarkers.Count -gt 0) {
    $missingProjectList = $missingProjectMarkers -join ', '
    throw "Project file is missing generated version configuration: $missingProjectList"
}

Write-Host "AssemblyInfo validation passed for $AssemblyInfoPath"
