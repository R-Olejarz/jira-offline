@echo off
setlocal

cd /d "%~dp0"

echo Publishing ToDoListApp for Windows x64...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\win-x64

if errorlevel 1 (
    echo.
    echo Publish failed.
    pause
    exit /b 1
)

echo.
echo Publish completed successfully.
echo Output folder: %CD%\publish\win-x64
pause
