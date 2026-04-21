param(
    [string]$Configuration = "Debug",
    [string]$ProjectPath = $(Join-Path $PSScriptRoot "qbookCode.csproj"),
    [string]$AssetsPath = $(Join-Path $PSScriptRoot "obj\project.assets.json"),
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
        [string]$GlobalPackagesRoot,
        [Parameter(Mandatory = $true)]
        [string]$PackageId,
        [Parameter(Mandatory = $true)]
        [string]$PackageVersion
    )

    $candidateFolders = @(
        (Join-Path $GlobalPackagesRoot (Join-Path $PackageId $PackageVersion)),
        (Join-Path $GlobalPackagesRoot (Join-Path $PackageId.ToLowerInvariant() $PackageVersion.ToLowerInvariant()))
    ) | Select-Object -Unique

    $packageFolder = $candidateFolders | Where-Object { Test-Path -Path $_ } | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($packageFolder)) {
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

if (-not (Test-Path -Path $ProjectPath)) {
    throw "Project file was not found: $ProjectPath"
}

if (-not (Test-Path -Path $AssetsPath)) {
    throw "NuGet assets file was not found: $AssetsPath"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $PSScriptRoot "bin\NuGetPackages.csv"
}

$projectRelativePath = Get-RelativePath -BasePath (Split-Path -Parent $PSScriptRoot) -Path $ProjectPath
$globalPackagesRoot = Join-Path $env:USERPROFILE ".nuget\packages"
$projectAssets = Get-Content -Path $AssetsPath -Raw | ConvertFrom-Json
$targetProperty = $projectAssets.targets.PSObject.Properties | Select-Object -First 1
if ($null -eq $targetProperty) {
    throw "No targets were found in $AssetsPath"
}

$targetFramework = $targetProperty.Name
$targetLibraries = $targetProperty.Value.PSObject.Properties
$directDependencies = @{}

foreach ($frameworkProperty in $projectAssets.project.frameworks.PSObject.Properties) {
    foreach ($dependencyProperty in $frameworkProperty.Value.dependencies.PSObject.Properties) {
        $directDependencies[$dependencyProperty.Name] = $true
    }
}

$rows = foreach ($libraryProperty in $targetLibraries) {
    if ($libraryProperty.Value.type -ne 'package') {
        continue
    }

    $separatorIndex = $libraryProperty.Name.LastIndexOf('/')
    if ($separatorIndex -lt 1) {
        continue
    }

    $packageId = $libraryProperty.Name.Substring(0, $separatorIndex)
    $packageVersion = $libraryProperty.Name.Substring($separatorIndex + 1)

    [PSCustomObject]@{
        Project = $projectRelativePath
        PackageId = $packageId
        Version = $packageVersion
        TargetFramework = $targetFramework
        DependencyType = if ($directDependencies.ContainsKey($packageId)) { "Direct" } else { "Transitive" }
        License = Get-PackageLicense -GlobalPackagesRoot $globalPackagesRoot -PackageId $packageId -PackageVersion $packageVersion
    }
}

$outputDirectory = Split-Path -Path $OutputPath -Parent
if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$rows | Sort-Object DependencyType, PackageId, Version | Export-Csv -Path $OutputPath -Delimiter ';' -NoTypeInformation -Encoding UTF8
Write-Host "Generated NuGet package report at $OutputPath"
