param()
$ErrorActionPreference = "Stop"
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$complianceRoot = Join-Path $repositoryRoot "compliance\foss"
$statePath = Join-Path $complianceRoot "approved-state.json"
try {
    Write-Host "Run this command only after external FOSS review. Commit the generated compliance/foss files on a branch and open a pull request; merging that pull request is the approval event."
    $temporaryManifest = Join-Path ([IO.Path]::GetTempPath()) ("qbook-foss-" + [Guid]::NewGuid().ToString("N") + ".json")
    $fingerprint = (& $PSScriptRoot\updateFossState.ps1 -Mode Approve -ManifestOutputPath $temporaryManifest).Trim()
    if ($LASTEXITCODE -ne 0 -or $fingerprint -notmatch '^[0-9a-f]{64}$') { throw "Current FOSS state could not be discovered." }
    $revision = 1
    if (Test-Path -LiteralPath $statePath) { $previous = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json; if ($previous.fingerprint -eq $fingerprint) { throw "The current FOSS state is already approved." }; if ($previous.fossRevision -notmatch '^[1-9]\d*$') { throw "Approved FOSS state is malformed." }; $revision = [int64]$previous.fossRevision + 1 }
    if ($revision -gt 65535) { throw "FossRevision must be between 1 and 65535." }
    $manifestPath = "compliance/foss/manifests/$fingerprint.json"
    $manifestFile = Join-Path $repositoryRoot $manifestPath
    if (Test-Path -LiteralPath $manifestFile) { throw "Approved FOSS manifest already exists: $manifestPath" }
    [IO.Directory]::CreateDirectory((Split-Path -Parent $manifestFile)) | Out-Null
    Move-Item -LiteralPath $temporaryManifest -Destination $manifestFile
    $commit = (& git -C $repositoryRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($commit)) { throw "Current Git commit could not be determined." }
    $state = [ordered]@{ schemaVersion = 1; fossRevision = $revision; fingerprint = $fingerprint; approvalCommit = $commit; approvedAt = [DateTime]::UtcNow.ToString("o"); manifestPath = $manifestPath }
    [IO.File]::WriteAllText($statePath, (($state | ConvertTo-Json -Compress) + "`n"), (New-Object Text.UTF8Encoding($false)))
    Write-Host "Approved FOSS state revision $revision with fingerprint $fingerprint"
}
catch { Write-Error $_.Exception.Message; exit 1 }
