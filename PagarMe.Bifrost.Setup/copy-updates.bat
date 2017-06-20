@echo off

setlocal enabledelayedexpansion

for /F "delims=" %%l in (
	'findstr "AssemblyVersion" %1..\PagarMe.Bifrost\Properties\BifrostAssemblyInfo.cs'
) do set line=%%l

for /f "tokens=1 delims=)" %%i in ("%line%") do (set prefix=%%i)

set removeStart=!line:[assembly: AssemblyVersion("=!
set version=!removeStart:")]=!

copy %2 %1..\PagarMe.Bifrost.Updates\bifrost-installer-%version%.msi

echo { "last_version_name": "%version%" } > %1..\PagarMe.Bifrost.Updates\update.json