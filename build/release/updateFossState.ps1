param(
    [Parameter(Mandatory = $true)]
    [string]$RevisionStatePath,
    [Parameter(Mandatory = $true)]
    [string]$MajorRevision,
    [Parameter(Mandatory = $true)]
    [string]$FeatureRevision
)

$ErrorActionPreference = "Stop"
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$packagesRoot = Join-Path $repositoryRoot "packages"

function Get-RelativePath([string]$Path) {
    $root = $repositoryRoot.TrimEnd("\\") + "\\"
    $uri = New-Object Uri($root)
    return [Uri]::UnescapeDataString($uri.MakeRelativeUri((New-Object Uri([IO.Path]::GetFullPath($Path)))).ToString()).Replace("\\", "/")
}

function Get-FileHashValue([string]$Path) {
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        $stream = [IO.File]::OpenRead($Path)
        try {
            $hashBytes = $sha256.ComputeHash($stream)
        }
        finally {
            $stream.Dispose()
        }
    }
    finally {
        $sha256.Dispose()
    }
    return (($hashBytes | ForEach-Object { $_.ToString("x2") }) -join "")
}

function Add-FileComponent([System.Collections.Generic.List[object]]$Components, [string]$SourcePath, [string]$LogicalPath, [string]$PackageId, [string]$PackageVersion) {
    if (-not (Test-Path -LiteralPath $SourcePath -PathType Leaf)) { throw "Required FOSS runtime file was not found: $(Get-RelativePath $SourcePath)" }
    $Components.Add([ordered]@{ identity = "file|$LogicalPath"; type = "runtimeFile"; logicalReleasePath = $LogicalPath; packageId = $PackageId; packageVersion = $PackageVersion; sha256 = Get-FileHashValue $SourcePath })
}

function Get-PackageDeclarations {
    $declarations = New-Object System.Collections.Generic.List[object]
    foreach ($project in @("qbook", "qbookCsScript")) {
        $configPath = Join-Path $repositoryRoot "$project\packages.config"
        if (-not (Test-Path -LiteralPath $configPath)) { throw "Required package declaration file was not found: $project/packages.config" }
        [xml]$config = Get-Content -Raw -LiteralPath $configPath
        foreach ($package in @($config.packages.package)) {
            if ([string]::IsNullOrWhiteSpace($package.id) -or [string]::IsNullOrWhiteSpace($package.version)) { throw "Package declaration in $project/packages.config is incomplete." }
            $packagePath = Join-Path $packagesRoot "$($package.id).$($package.version)"
            if (-not (Test-Path -LiteralPath $packagePath -PathType Container)) { throw "Restored package was not found: packages/$($package.id).$($package.version)" }
            $declarations.Add([ordered]@{ identity = "package|$project|$($package.id)|$($package.version)"; type = "package"; logicalReleasePath = $null; packageId = [string]$package.id; packageVersion = [string]$package.version; sha256 = $null; project = $project })
        }
    }
    return $declarations
}

function Get-ProjectPackageFiles([System.Collections.Generic.List[object]]$Components, $Declarations) {
    $packageLookup = @{}
    foreach ($declaration in $Declarations) { $packageLookup["$($declaration.packageId).$($declaration.packageVersion)"] = $declaration }
    foreach ($project in @("qbook", "qbookCsScript")) {
        [xml]$projectXml = Get-Content -Raw -LiteralPath (Join-Path $repositoryRoot "$project\$project.csproj")
        $manager = New-Object Xml.XmlNamespaceManager($projectXml.NameTable)
        $manager.AddNamespace("m", "http://schemas.microsoft.com/developer/msbuild/2003")
        foreach ($hintPath in $projectXml.SelectNodes("//m:Reference/m:HintPath", $manager)) {
            $sourcePath = [IO.Path]::GetFullPath((Join-Path (Join-Path $repositoryRoot $project) $hintPath.InnerText))
            if ($sourcePath.StartsWith($packagesRoot, [StringComparison]::OrdinalIgnoreCase)) {
                $packageFolder = (($sourcePath.Substring($packagesRoot.Length) -replace '^[\\/]+', '') -split '[\\/]')[0]
                $package = $packageLookup[$packageFolder]
                if ($null -eq $package) { throw "Referenced package file does not belong to a declared package: $(Get-RelativePath $sourcePath)" }
                Add-FileComponent $Components $sourcePath ("bin/" + [IO.Path]::GetFileName($sourcePath)) $package.packageId $package.packageVersion
            }
        }
    }
}

function Add-PackageTree([System.Collections.Generic.List[object]]$Components, [string]$PackageFolder, [string]$RelativeFolder, [string]$OutputPrefix, $Declarations) {
    $package = $Declarations | Where-Object { "$($_.packageId).$($_.packageVersion)" -eq $PackageFolder } | Select-Object -First 1
    if ($null -eq $package) { throw "Runtime package is not declared: $PackageFolder" }
    $root = Join-Path $packagesRoot "$PackageFolder\$RelativeFolder"
    if (-not (Test-Path -LiteralPath $root -PathType Container)) { throw "Required FOSS runtime directory was not found: $(Get-RelativePath $root)" }
    Get-ChildItem -LiteralPath $root -Recurse -File | Sort-Object FullName | ForEach-Object {
        $relative = $_.FullName.Substring($root.Length).TrimStart("\\").Replace("\\", "/")
        Add-FileComponent $Components $_.FullName "$OutputPrefix/$relative" $package.packageId $package.packageVersion
    }
}

function Get-Manifest {
    $declarations = Get-PackageDeclarations
    $components = New-Object System.Collections.Generic.List[object]
    foreach ($declaration in $declarations) { $components.Add($declaration) }
    Get-ProjectPackageFiles $components $declarations
    Add-PackageTree $components "cef.redist.x64.120.2.7" "CEF" "bin/cef/x64" $declarations
    Add-PackageTree $components "cef.redist.x86.120.2.7" "CEF" "bin/cef/x86" $declarations
    Add-PackageTree $components "chromiumembeddedframework.runtime.win-x64.141.0.11" "CEF/win-x64" "bin/cef/x64" $declarations
    Add-PackageTree $components "chromiumembeddedframework.runtime.win-x86.141.0.11" "CEF/win-x86" "bin/cef/x86" $declarations
    Add-PackageTree $components "OpenCvSharp4.runtime.win.4.10.0.20241108" "runtimes" "bin/dll" $declarations
    Add-PackageTree $components "Stub.System.Data.SQLite.Core.NetFramework.1.0.119.0" "build/net46" "bin/sqlite" $declarations
    $buildHost = Join-Path $env:USERPROFILE ".nuget\packages\microsoft.codeanalysis.workspaces.msbuild\4.14.0\contentFiles\any\any\BuildHost-netcore"
    if (-not (Test-Path -LiteralPath $buildHost -PathType Container)) { throw "Required Roslyn BuildHost-netcore runtime directory was not found: $buildHost" }
    Get-ChildItem -LiteralPath $buildHost -Recurse -File | Sort-Object FullName | ForEach-Object { Add-FileComponent $components $_.FullName ("bin/libs/BuildHost-netcore/" + $_.FullName.Substring($buildHost.Length).TrimStart("\\").Replace("\\", "/")) "Microsoft.CodeAnalysis.Workspaces.MSBuild" "4.14.0" }
    Add-FileComponent $components (Join-Path $repositoryRoot "qbook\PdfSharp.dll") "bin/PdfSharp.dll" $null $null
    Add-FileComponent $components (Join-Path $repositoryRoot "qbook\ThirdPartyNotices.txt") "bin/ThirdPartyNotices.txt" $null $null
    $ordered = @($components | Sort-Object identity | ForEach-Object { $copy = [ordered]@{ identity=$_.identity; type=$_.type; logicalReleasePath=$_.logicalReleasePath; packageId=$_.packageId; packageVersion=$_.packageVersion; sha256=$_.sha256 }; $copy })
    return [ordered]@{ schemaVersion = 1; components = $ordered }
}

function ConvertTo-CanonicalJson($Object) { return (($Object | ConvertTo-Json -Depth 8 -Compress) + "`n") }
function ConvertTo-IndentedJson($Object) { return (($Object | ConvertTo-Json -Depth 8) + "`n") }
function Write-Utf8NoBom([string]$Path, [string]$Content) { [IO.Directory]::CreateDirectory((Split-Path -Parent $Path)) | Out-Null; [IO.File]::WriteAllText($Path, $Content.Replace("`r`n", "`n"), (New-Object Text.UTF8Encoding($false))) }

try {
    if (-not (Test-Path -LiteralPath $RevisionStatePath -PathType Leaf)) { throw "NuGet revision state was not found: $RevisionStatePath" }
    if ($MajorRevision -notmatch '^\d+$' -or $FeatureRevision -notmatch '^\d+$') { throw "MajorRevision and FeatureRevision must be non-negative integers not greater than 65535." }
    try { $selectedMajorRevision = [int64]$MajorRevision; $selectedFeatureRevision = [int64]$FeatureRevision } catch { throw "MajorRevision and FeatureRevision must be non-negative integers not greater than 65535." }
    if ($selectedMajorRevision -gt 65535 -or $selectedFeatureRevision -gt 65535) { throw "MajorRevision and FeatureRevision must be non-negative integers not greater than 65535." }
    $manifest = Get-Manifest
    $manifestJson = ConvertTo-CanonicalJson $manifest
    $manifestBytes = (New-Object Text.UTF8Encoding($false)).GetBytes($manifestJson)
    $fingerprint = ([Security.Cryptography.SHA256]::Create().ComputeHash($manifestBytes) | ForEach-Object { $_.ToString("x2") }) -join ""
    try { $state = Get-Content -Raw -LiteralPath $RevisionStatePath | ConvertFrom-Json } catch { throw "NuGet revision state is malformed or incomplete." }
    if ($null -eq $state -or $state.schemaVersion -ne 1 -or $state.majorRevision -notmatch '^\d+$' -or $state.featureRevision -notmatch '^\d+$' -or $state.fossRevision -notmatch '^\d+$' -or $state.buildId -notmatch '^\d+$' -or $state.fingerprint -notmatch '^[0-9a-f]{64}$' -or $null -eq $state.components) { throw "NuGet revision state is malformed or incomplete." }
    try { $fossRevision = [int64]$state.fossRevision; $buildId = [int64]$state.buildId } catch { throw "NuGet revision state is malformed or incomplete." }
    if ($fossRevision -gt 65535) { throw "FossRevision must be between 0 and 65535." }
    if ($state.fingerprint -ne $fingerprint) {
        if ($fossRevision -eq 65535) { throw "FossRevision cannot be incremented beyond 65535." }
        $fossRevision++
    }
    if ($buildId -eq [int64]::MaxValue) { throw "BuildId cannot be incremented beyond $([int64]::MaxValue)." }
    $buildId++
    $newState = [ordered]@{ schemaVersion = 1; majorRevision = $selectedMajorRevision; featureRevision = $selectedFeatureRevision; fossRevision = $fossRevision; buildId = $buildId; fingerprint = $fingerprint; components = $manifest.components }
    Write-Utf8NoBom $RevisionStatePath (ConvertTo-IndentedJson $newState)
    Write-Host "Updated release state with FossRevision $fossRevision and BuildId $buildId."
}
catch { Write-Error $_.Exception.Message; exit 1 }
