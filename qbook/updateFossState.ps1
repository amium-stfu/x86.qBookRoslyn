param(
    [ValidateSet("Validate", "Approve")]
    [string]$Mode = "Validate",
    [string]$ResultPath = $(Join-Path $PSScriptRoot "obj\ValidatedFossState.json"),
    [string]$ManifestOutputPath
)

$ErrorActionPreference = "Stop"
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$packagesRoot = Join-Path $repositoryRoot "packages"
$complianceRoot = Join-Path $repositoryRoot "compliance\foss"
$approvedStatePath = Join-Path $complianceRoot "approved-state.json"

function Get-RelativePath([string]$Path) {
    $root = $repositoryRoot.TrimEnd("\\") + "\\"
    $uri = New-Object Uri($root)
    return [Uri]::UnescapeDataString($uri.MakeRelativeUri((New-Object Uri([IO.Path]::GetFullPath($Path)))).ToString()).Replace("\\", "/")
}

function Get-FileHashValue([string]$Path) {
    return (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
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
                $packageFolder = $sourcePath.Substring($packagesRoot.Length).TrimStart("\\").Split("\\")[0]
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
    Add-FileComponent $components (Join-Path $PSScriptRoot "PdfSharp.dll") "bin/PdfSharp.dll" $null $null
    Add-FileComponent $components (Join-Path $PSScriptRoot "ThirdPartyNotices.txt") "bin/ThirdPartyNotices.txt" $null $null
    $ordered = @($components | Sort-Object identity | ForEach-Object { $copy = [ordered]@{ identity=$_.identity; type=$_.type; logicalReleasePath=$_.logicalReleasePath; packageId=$_.packageId; packageVersion=$_.packageVersion; sha256=$_.sha256 }; $copy })
    return [ordered]@{ schemaVersion = 1; components = $ordered }
}

function ConvertTo-CanonicalJson($Object) { return (($Object | ConvertTo-Json -Depth 8 -Compress) + "`n") }
function Write-Utf8NoBom([string]$Path, [string]$Content) { [IO.Directory]::CreateDirectory((Split-Path -Parent $Path)) | Out-Null; [IO.File]::WriteAllText($Path, $Content.Replace("`r`n", "`n"), (New-Object Text.UTF8Encoding($false))) }

function Get-CanonicalFingerprint([string]$Json, [string]$Description) {
    try { $parsed = $Json | ConvertFrom-Json } catch { throw "$Description is malformed." }
    if ($null -eq $parsed -or $parsed.schemaVersion -ne 1 -or $null -eq $parsed.components) { throw "$Description is malformed or incomplete." }
    foreach ($component in @($parsed.components)) {
        if ($null -eq $component -or [string]::IsNullOrWhiteSpace($component.identity) -or [string]::IsNullOrWhiteSpace($component.type) -or -not $component.PSObject.Properties.Match("logicalReleasePath") -or -not $component.PSObject.Properties.Match("packageId") -or -not $component.PSObject.Properties.Match("packageVersion") -or -not $component.PSObject.Properties.Match("sha256")) { throw "$Description is malformed or incomplete." }
    }
    $canonicalJson = ConvertTo-CanonicalJson $parsed
    $bytes = (New-Object Text.UTF8Encoding($false)).GetBytes($canonicalJson)
    return ([Security.Cryptography.SHA256]::Create().ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") }) -join ""
}

try {
    $manifest = Get-Manifest
    $manifestJson = ConvertTo-CanonicalJson $manifest
    $manifestBytes = (New-Object Text.UTF8Encoding($false)).GetBytes($manifestJson)
    $fingerprint = ([Security.Cryptography.SHA256]::Create().ComputeHash($manifestBytes) | ForEach-Object { $_.ToString("x2") }) -join ""
    if ($Mode -eq "Approve") {
        if (-not [string]::IsNullOrWhiteSpace($ManifestOutputPath)) { Write-Utf8NoBom $ManifestOutputPath $manifestJson }
        Write-Output $fingerprint
        exit 0
    }
    if (-not (Test-Path -LiteralPath $approvedStatePath)) { throw "Approved FOSS state was not found: compliance/foss/approved-state.json" }
    try { $state = Get-Content -Raw -LiteralPath $approvedStatePath | ConvertFrom-Json } catch { throw "Approved FOSS state is malformed or incomplete." }
    if ($null -eq $state -or $state.schemaVersion -ne 1 -or $state.fossRevision -notmatch '^[1-9]\d*$' -or $state.fingerprint -notmatch '^[0-9a-f]{64}$' -or [string]::IsNullOrWhiteSpace($state.manifestPath) -or $state.manifestPath -ne "compliance/foss/manifests/$($state.fingerprint).json" -or $state.approvalCommit -notmatch '^[0-9a-f]{40}([0-9a-f]{24})?$' -or [string]::IsNullOrWhiteSpace($state.approvedAt)) { throw "Approved FOSS state is malformed or incomplete." }
    try { $approvedAt = [DateTime]::Parse($state.approvedAt, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind) } catch { throw "Approved FOSS state is malformed or incomplete." }
    try { $fossRevision = [int64]$state.fossRevision } catch { throw "Approved FOSS state is malformed or incomplete." }
    if ($fossRevision -gt 65535) { throw "Approved FOSS state contains an invalid FossRevision." }
    $approvedManifest = Join-Path $repositoryRoot $state.manifestPath
    if (-not (Test-Path -LiteralPath $approvedManifest)) { throw "Approved FOSS manifest was not found: $($state.manifestPath)" }
    $approvedManifestJson = Get-Content -Raw -LiteralPath $approvedManifest
    $approvedFingerprint = Get-CanonicalFingerprint $approvedManifestJson "Approved FOSS manifest"
    if ($approvedFingerprint -ne $state.fingerprint) { throw "Approved FOSS manifest fingerprint does not match the approved baseline." }
    if ($state.fingerprint -ne $fingerprint) {
        $old = $approvedManifestJson | ConvertFrom-Json
        $oldIndex=@{}; foreach($item in $old.components){$oldIndex[$item.identity]=($item | ConvertTo-Json -Compress)}; $newIndex=@{}; foreach($item in $manifest.components){$newIndex[$item.identity]=($item | ConvertTo-Json -Compress)}
        foreach($key in @($oldIndex.Keys + $newIndex.Keys | Sort-Object -Unique)){if(-not $oldIndex.ContainsKey($key)){Write-Host "FOSS added: $key"}elseif(-not $newIndex.ContainsKey($key)){Write-Host "FOSS removed: $key"}elseif($oldIndex[$key] -ne $newIndex[$key]){Write-Host "FOSS changed: $key"}}
        throw "FOSS state fingerprint does not match the approved baseline."
    }
    Write-Utf8NoBom $ResultPath (ConvertTo-CanonicalJson ([ordered]@{ fossRevision=$fossRevision; fingerprint=$fingerprint; manifestPath=$state.manifestPath }))
    Write-Host "Validated approved FOSS state $fingerprint"
}
catch { Write-Error $_.Exception.Message; exit 1 }
