param(
    [string]$Configuration = "Debug",
    [string]$OutputPath = $(Join-Path $PSScriptRoot "bin\NuGetPackages.csv")
)

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BasePath,
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $resolvedBasePath = (Resolve-Path -Path $BasePath).Path.TrimEnd('\') + '\'
    $resolvedPath = (Resolve-Path -Path $Path).Path
    $baseUri = New-Object System.Uri($resolvedBasePath)
    $pathUri = New-Object System.Uri($resolvedPath)

    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($pathUri).ToString()).Replace('/', '\')
}

function Get-PackageLicense {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PackagesRoot,
        [Parameter(Mandatory = $true)]
        [string]$PackageId,
        [Parameter(Mandatory = $true)]
        [string]$PackageVersion
    )

    $packageFolder = Join-Path $PackagesRoot "$PackageId.$PackageVersion"
    if (-not (Test-Path -Path $packageFolder)) {
        return ""
    }

    $nuspecFile = Get-ChildItem -Path $packageFolder -Filter "*.nuspec" -File | Select-Object -First 1
    if ($null -eq $nuspecFile) {
        return ""
    }

    try {
        $nuspecXml = [xml](Get-Content -Path $nuspecFile.FullName -Raw)
        $metadata = $nuspecXml.package.metadata
        if ($null -eq $metadata) {
            return ""
        }

        if ($null -ne $metadata.license -and -not [string]::IsNullOrWhiteSpace($metadata.license.'#text')) {
            $licenseType = $metadata.license.type
            $licenseValue = $metadata.license.'#text'.Trim()
            if ([string]::IsNullOrWhiteSpace($licenseType)) {
                return $licenseValue
            }

            return "${licenseType}:$licenseValue"
        }

        if (-not [string]::IsNullOrWhiteSpace($metadata.licenseUrl)) {
            return $metadata.licenseUrl.Trim()
        }
    }
    catch {
    }

    return ""
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $PSScriptRoot "bin\NuGetPackages.csv"
}

$solutionRoot = Split-Path -Parent $PSScriptRoot
$packagesRoot = Join-Path $solutionRoot "packages"
$packagesConfigFiles = Get-ChildItem -Path $solutionRoot -Filter "packages.config" -Recurse -File | Sort-Object FullName

$rows = foreach ($packagesConfigFile in $packagesConfigFiles) {
    $packageConfigXml = [xml](Get-Content -Path $packagesConfigFile.FullName -Raw)
    $projectPath = Get-RelativePath -BasePath $solutionRoot -Path $packagesConfigFile.FullName

    foreach ($package in $packageConfigXml.packages.package) {
        [PSCustomObject]@{
            Project = $projectPath
            PackageId = $package.id
            Version = $package.version
            TargetFramework = $package.targetFramework
            DevelopmentDependency = if ($package.developmentDependency -eq "true") { "true" } else { "false" }
            License = Get-PackageLicense -PackagesRoot $packagesRoot -PackageId $package.id -PackageVersion $package.version
        }
    }
}

$outputDirectory = Split-Path -Path $OutputPath -Parent
if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$rows | Sort-Object Project, PackageId, Version | Export-Csv -Path $OutputPath -Delimiter ';' -NoTypeInformation -Encoding UTF8
Write-Host "Generated NuGet package report at $OutputPath"
