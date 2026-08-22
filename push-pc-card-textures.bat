@echo off
REM Copies PC card textures onto a connected Android device into the app's persistent storage.
REM Requires: adb on PATH, phone with USB debugging, DCGO installed (com.DCGO.DCGO).
REM Source: F:\DCGO_Application\Assets\Textures\Card  (~1GB — may take a while)

set "SRC=F:\DCGO_Application\Assets\Textures\Card"
set "PKG=com.DCGO.DCGO"
set "DEST=/sdcard/Android/data/%PKG%/files/Textures/Card"

if not exist "%SRC%" (
  echo Source not found: %SRC%
  exit /b 1
)

where adb >nul 2>&1
if errorlevel 1 (
  echo adb not found on PATH. Install Android platform-tools or add it to PATH.
  exit /b 1
)

echo Pushing card textures to device...
echo   from: %SRC%
echo   to:   %DEST%
adb shell mkdir -p "%DEST%"
adb push "%SRC%." "%DEST%/"
echo Done. Relaunch DCGO on the phone.
exit /b 0
