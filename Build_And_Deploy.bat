@echo off
color 0B
echo ========================================================
echo   NetSupport MVP (2026 Edition) - Automated Deployment
echo ========================================================
echo.
echo This script will compile all three applications into
echo standalone, easily clickable .exe files.
echo.
pause

set RELEASE_DIR=NetSupport_Release

echo.
echo Cleaning up old releases...
if exist "%RELEASE_DIR%" rmdir /s /q "%RELEASE_DIR%"
mkdir "%RELEASE_DIR%"

echo.
echo [1/3] Building NetSupport.Designer...
dotnet publish NetSupport.Designer/NetSupport.Designer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "%RELEASE_DIR%"

echo.
echo [2/3] Building NetSupport.Student...
dotnet publish NetSupport.Student/NetSupport.Student.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "%RELEASE_DIR%"

echo.
echo [3/3] Building NetSupport.Tutor...
dotnet publish NetSupport.Tutor/NetSupport.Tutor.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "%RELEASE_DIR%"

echo.
echo ========================================================
echo   DEPLOYMENT COMPLETE!
echo ========================================================
echo.
echo Look for the 'NetSupport_Release' folder in this directory!
echo Inside, you will find 3 neat .exe files:
echo  1. NetSupport.Designer.exe
echo  2. NetSupport.Student.exe
echo  3. NetSupport.Tutor.exe
echo.
echo You can zip the 'NetSupport_Release' folder and share it!
echo.
pause
