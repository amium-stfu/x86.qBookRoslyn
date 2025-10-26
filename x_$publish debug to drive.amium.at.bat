set dir=setup\
@echo off
FOR /F "delims=" %%I IN ('DIR %dir% /B /A-D /O-D') DO SET newestFile=%%I& GOTO done
:done
ECHO newest file in '%dir%' is '%newestFile%'

echo uploading file '%dir%%newestFile%' to drive.amium.at\Public\Software\qbook\...
curl -u amium:amium07# -T %dir%%newestFile% "https://drive.amium.at/remote.php/dav/files/amium/Public/Software/qbook/
echo done!