@echo off
REM Builds a Development Android APK using Unity 2021.3.45f2 batchmode.
REM Usage: build-android-dev.bat [path\to\Unity.exe]
setlocal

set "UNITY=%~1"
if "%UNITY%"=="" (
  if exist "E:\Unity3d\Unity Editor\2021.3.45f2\Editor\Unity.exe" (
    set "UNITY=E:\Unity3d\Unity Editor\2021.3.45f2\Editor\Unity.exe"
  ) else (
    set "UNITY=C:\Program Files\Unity\Hub\Editor\2021.3.45f2\Editor\Unity.exe"
  )
)

if not exist "%UNITY%" (
  echo Unity not found at: %UNITY%
  echo Pass the Unity.exe path as the first argument.
  exit /b 1
)

set "PROJECT=%~dp0"
set "LOG=%PROJECT%Builds\Android\build-android-dev.log"
if not exist "%PROJECT%Builds\Android" mkdir "%PROJECT%Builds\Android"

echo Building Development APK with:
echo   Unity:   %UNITY%
echo   Project: %PROJECT%
echo   Log:     %LOG%

REM Playable APK uses less IL2CPP RAM than Development+debugger builds.
"%UNITY%" -quit -batchmode -nographics -projectPath "%PROJECT%" -executeMethod AndroidDevelopmentBuild.BuildPlayableApk -logFile "%LOG%"
set "CODE=%ERRORLEVEL%"
echo Exit code: %CODE%
exit /b %CODE%
