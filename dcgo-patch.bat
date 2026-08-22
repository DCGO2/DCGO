@echo off
REM DCGO custom patching tool wrapper.
REM Usage: dcgo-patch.bat export|apply|rebase|status|verify [args...]
setlocal
set "SCRIPT=%~dp0tools\dcgo-patch\dcgo-patch.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" %*
exit /b %ERRORLEVEL%
