# Datumsversion erzeugen (ohne führende Nullen)
$version = (Get-Date).ToString("yyyy.Md.Hm")
Write-Host "Neue Version: $version"

# package.json einlesen
$json = Get-Content -Raw -Path "package.json" | ConvertFrom-Json

# Version setzen
$json.version = $version

# Zurückschreiben
$json | ConvertTo-Json -Depth 10 | Set-Content -Path "package.json" -Encoding UTF8

Write-Host "Version erfolgreich gesetzt."
Write-Host "Taste drücken zum Schließen..."
Read-Host