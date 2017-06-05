@echo off

REM Firefox
dir /b/s %appdata%\Mozilla\*cert*.db > cert_path.txt
set /p cert_path=<cert_path.txt

for %%A in ("%cert_path%") do (
    set cert_path=%%~dpA
)

REM certutil.exe -d %cert_path% -L
certutil.exe -d %cert_path% -D -n Bifrost

echo on
certutil.exe -d %cert_path% -A -t "TCu,Cuw,Tuw" -n Bifrost -i %1\%2.crt
