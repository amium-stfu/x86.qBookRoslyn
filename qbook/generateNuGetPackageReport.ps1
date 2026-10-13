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
        $packageFolder = Join-Path $PackagesRoot ($PackageId.ToLowerInvariant() + "\" + $PackageVersion.ToLowerInvariant())
    }
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
$projectFiles = @("qbook\qbook.csproj", "qbookCsScript\qbookCsScript.csproj")
$rows = foreach ($projectFile in $projectFiles) {
    $projectFullPath = Join-Path $solutionRoot $projectFile
    $projectXml = [xml](Get-Content -LiteralPath $projectFullPath -Raw)
    $assetsPath = Join-Path (Split-Path $projectFullPath) "obj\project.assets.json"
    if (-not (Test-Path -LiteralPath $assetsPath)) {
        throw "Dependency restore is required for $projectFile before generating the package report."
    }
    $assets = Get-Content -LiteralPath $assetsPath -Raw | ConvertFrom-Json
    $packagesRoot = @($assets.packageFolders.PSObject.Properties.Name)[0]
    foreach ($library in $assets.libraries.PSObject.Properties) {
        if ($library.Value.type -ne "package") { continue }
        $packageId, $packageVersion = $library.Name -split '/', 2
        $direct = $projectXml.SelectNodes('//PackageReference') | Where-Object { $_.Include -eq $packageId }
        [PSCustomObject]@{
            Project = $projectFile
            PackageId = $packageId
            Version = $packageVersion
            TargetFramework = [string]$projectXml.Project.PropertyGroup.TargetFramework
            DevelopmentDependency = if ($direct -and $direct.PrivateAssets -eq "all") { "true" } else { "false" }
            License = Get-PackageLicense -PackagesRoot $packagesRoot -PackageId $packageId -PackageVersion $packageVersion
        }
    }
}

$outputDirectory = Split-Path -Path $OutputPath -Parent
if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$rows | Sort-Object Project, PackageId, Version | Export-Csv -Path $OutputPath -Delimiter ';' -NoTypeInformation -Encoding UTF8
Write-Host "Generated NuGet package report at $OutputPath"
