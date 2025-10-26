(Get-Content "..\..\Properties\AssemblyInfo.cs") | ForEach-Object {
    $version = "20$(Get-Date -Format 'yy.MM.dd').*"
    if ($_ -match "^\[assembly: AssemblyVersion\(") {
        #$_ -replace '([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)', $version
        $_ -replace '\(\".*\"\)', "(""$version"")"
    } else {
        $_
    }
} | Set-Content "..\..\Properties\AssemblyInfo.cs"