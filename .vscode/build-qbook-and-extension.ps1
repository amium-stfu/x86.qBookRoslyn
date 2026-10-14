$ErrorActionPreference = 'Stop'
$buildWorkspace = Split-Path -Parent $PSScriptRoot
try {
    Push-Location -LiteralPath $buildWorkspace
    try {
        & 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe' 'qbookStudio.sln' '/t:Build' '/p:Configuration=Release' '/p:Platform=x64'
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    finally { Pop-Location }
    Push-Location -LiteralPath (Join-Path $buildWorkspace 'VScodePlugin/amium-qbook')
    try {
        & 'C:/Program Files/nodejs/node.exe' 'node_modules/typescript/bin/tsc' '-p' './'
        exit $LASTEXITCODE
    }
    finally { Pop-Location }
}
catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}
