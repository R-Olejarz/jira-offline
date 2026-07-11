@echo off
setlocal

cd /d "%~dp0"

echo Publishing ToDoListApp for Linux x64...

dotnet publish -c Release -r linux-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\linux-x64

if errorlevel 1 (
    echo.
    echo Publish failed.
    pause
    exit /b 1
)

echo.
echo Publish completed successfully.
echo Output folder: %CD%\publish\linux-x64
pause