@echo off
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File ".\ViewLogs.ps1" %*