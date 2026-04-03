#Requires -RunAsAdministrator
$ErrorActionPreference = "SilentlyContinue"

$AppName = "ErixOpti"
$InstallDir = Join-Path $env:ProgramFiles $AppName
$StartMenuDir = Join-Path ([Environment]::GetFolderPath("CommonStartMenu")) "Programs"
$DesktopDir = [Environment]::GetFolderPath("CommonDesktopDirectory")
$UninstallKey = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$AppName"

Write-Host "Uninstalling $AppName..." -ForegroundColor Cyan
Remove-Item -Force "$StartMenuDir\$AppName.lnk"
Remove-Item -Force "$DesktopDir\$AppName.lnk"
Remove-Item -Path $UninstallKey -Recurse -Force
Remove-Item -Recurse -Force $InstallDir

Write-Host "  $AppName has been uninstalled." -ForegroundColor Green
