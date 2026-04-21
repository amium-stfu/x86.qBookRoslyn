param(
    [string]$Configuration = "Debug",
    [string]$OutputPath = $(Join-Path $PSScriptRoot "obj\GeneratedAssemblyVersionInfo.cs"),
    [string]$BaseVersion = "1.0.0.0"
)

$version = if ($Configuration -ieq "Release") {
    Get-Date -Format 'yyyy.MM.dd.HHmm'
}
else {
    $BaseVersion
}

$informationalVersion = $version

try {
    $commitId = (& git -C $PSScriptRoot rev-parse --short HEAD 2>$null).Trim()
    if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($commitId)) {
        $informationalVersion = "$version+$commitId"
    }
}
catch {
}

$outputDirectory = Split-Path -Path $OutputPath -Parent
if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$generatedContent = @"
using System.Reflection;

[assembly: AssemblyVersion("$version")]
[assembly: AssemblyFileVersion("$version")]
[assembly: AssemblyInformationalVersion("$informationalVersion")]
"@

Set-Content -Path $OutputPath -Value $generatedContent -Encoding UTF8
Write-Host "Generated assembly version info at $OutputPath for configuration $Configuration"
