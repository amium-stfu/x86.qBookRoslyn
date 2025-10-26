$rootDir=get-location
$rootDir=-join($rootDir,'')


#create a setup.temp$ dir and copy everything we need there...
write-output 'creating temporary setup.temp$ directory...'
$setupDir=$rootDir+"\.setup.temp$"
write-output 'setupDir='$setupDir
if (Test-Path "$setupDir") {
  Remove-Item "$setupDir" -Force -Recurse
}
New-Item "$setupDir" -ItemType Directory



write-output 'copying items...'
$binDir = $rootDir+'\qbook\bin\Debug\'
Copy-Item -Path $($binDir+"qbookStudio.exe") -Destination $setupDir
Copy-Item -Path $($binDir+"qbookStudio.exe.config") -Destination $setupDir
Copy-Item -Path $($binDir+"log4net.config") -Destination $setupDir
Copy-Item -Path $($binDir+"\libs") -Destination $setupDir -Recurse
Copy-Item -Path $($binDir+"\CsSnippets") -Destination $setupDir -Recurse
#Copy-Item -Path $($binDir+"\assets") -Destination $setupDir -Recurse
#Copy-Item -Path $($binDir+"\qbook.examples") -Destination $setupDir -Recurse



write-output 'getting assembly-version info...'
#get version information
$mainAssemblyPath = $setupDir+'\qbookStudio.exe'
$Assemblyversion = [System.Reflection.AssemblyName]::GetAssemblyName($mainAssemblyPath).Version;
$version = 'v'+$Assemblyversion.Major+'.'+$Assemblyversion.Minor+'.'+$Assemblyversion.Build+'.'+$Assemblyversion.Revision
write-output $('   -> AssemblyVersion: '+$version)



write-output 'creating setup*.zip...'
#change to setup.temp$ and zip everything...
Set-Location -Path $setupDir
$7zPath = $rootDir+'\7z.exe'
$setupTargetPath = $rootDir+'\setup\'+"qbookStudio."+$version+".zip"

#$filelistInclude = $rootDir+'\setup.filelist.include.txt'
#Start-Process -FilePath $7zPath -ArgumentList "a", "-tzip", "-ir@""$filelistInclude""", $setupTargetPath -Wait
Start-Process -FilePath $7zPath -ArgumentList "a", "-tzip", $setupTargetPath -Wait



write-output 'removing temporary setup.temp$ directory...'
Set-Location -Path $rootDir
if (Test-Path "$setupDir") {
  Remove-Item "$setupDir" -Force -Recurse
}

exit
# ----------------------------------------------------------------------------------------







$binPath=$workingDir+'\qbook\bin\Debug'
$compressPath=@(
"$binPath\libs\",
"$binPath\qbooks\",
"$binPath\assets\",
"$binPath\CsSnippets\",

"$binPath\*.dll",
"$binPath\*.exe",
"$binPath\*.exe.config",
"$binPath\log4net.config"

# "qbook.Manual.pdf",

# "$binPath\qbook.exe"
)

if (Test-Path '.\setup\') {
}
else {
  New-Item '.\setup\' -ItemType Directory
}
$destinationPath=$workingDir+'\setup\qbookStudio.'+$version

if ($Args -contains "-html") {
$compressPath += @(
"$binPath\libs.cef\",
"$binPath\html\"
);
$destinationPath += @("+html");
}
if ($Args -contains "-dnspy") {
$compressPath += @(
"$binPath\dnSpy\"
);
$destinationPath += @("+dnspy");
}




if ($FALSE)
{
  $randomSuffix = -join((65..90) + (97..122) | Get-Random -Count 1 | % {[char]$_})
  $randomSuffix += -join((48..57) + (65..90) + (97..122) | Get-Random -Count 9 | % {[char]$_})
  $destinationPath += '.'+$randomSuffix
}
$destinationPath += '.zip'

if (Test-Path $destinationPath) {
  Remove-Item $destinationPath
}
write-output $compressPath
write-output $destinationPath

$compress = @{
  Path = $compressPath
  CompressionLevel = "Optimal"
  DestinationPath = $destinationPath
}

#..."$binPath\qbooks\example.*.qbook" puts files to \ not to \qbooks\, so:
#HACK: only add \qbook.for_setup$\* and \qbook.for_setup$\* to setup-archive
if (Test-Path "$binPath\qbooks.temp$") {
    Remove-Item "$binPath\qbooks.temp$" -Force -Recurse
}
Rename-Item "$binPath\qbooks" "$binPath\qbooks.temp$"
#Remove-Item "$binPath\qbooks" -Force -Recurse
if (Test-Path "$binPath\qbooks.for_setup$") {
}
else {
    New-Item "$binPath\qbooks.for_setup$" -ItemType Directory
    New-Item "$binPath\qbooks.for_setup$\dummy.txt" -ItemType File
}
Copy-Item "$binPath\qbooks.for_setup$" "$binPath\qbooks" -Force -Recurse

if (Test-Path "$binPath\assets.temp$") {
    Remove-Item "$binPath\assets.temp$" -Force -Recurse
}
Rename-Item "$binPath\assets" "$binPath\assets.temp$"
if (Test-Path "$binPath\assets.for_setup$") {
}
else {
    New-Item "$binPath\assets.for_setup$" -ItemType Directory
    New-Item "$binPath\assets.for_setup$\dummy.txt" -ItemType File
}
Copy-Item "$binPath\assets.for_setup$" "$binPath\assets" -Force -Recurse


Compress-Archive @compress -Force


Remove-Item "$binPath\qbooks" -Force -Recurse
Rename-Item "$binPath\qbooks.temp$" "$binPath\qbooks"

Remove-Item "$binPath\assets" -Force -Recurse
Rename-Item "$binPath\assets.temp$" "$binPath\assets"
