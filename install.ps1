#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"

$AppName = "ErixOpti"
$Publisher = "Erix"
$ExeName = "ErixOpti.exe"
$InstallDir = Join-Path $env:ProgramFiles $AppName
$StartMenuDir = Join-Path ([Environment]::GetFolderPath("CommonStartMenu")) "Programs"
$DesktopDir = [Environment]::GetFolderPath("CommonDesktopDirectory")
$UninstallKey = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$AppName"

$SourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceExe = Join-Path $SourceDir "publish\singlefile\$ExeName"
if (-not (Test-Path $SourceExe)) {
    $SourceExe = Join-Path $SourceDir $ExeName
}
if (-not (Test-Path $SourceExe)) {
    Write-Host "ERROR: Cannot find $ExeName. Build first with:" -ForegroundColor Red
    Write-Host "  dotnet publish ErixOpti/ErixOpti.csproj -c Release -r win-x64 -o publish/singlefile" -ForegroundColor Yellow
    exit 1
}

Write-Host "Installing $AppName..." -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
Copy-Item -Force $SourceExe (Join-Path $InstallDir $ExeName)

# Copy any loose assets alongside the exe
$AssetsDir = Join-Path $SourceDir "ErixOpti\Assets"
if (Test-Path $AssetsDir) {
    Copy-Item -Recurse -Force $AssetsDir (Join-Path $InstallDir "Assets")
}

# Start Menu shortcut
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$StartMenuDir\$AppName.lnk")
$Shortcut.TargetPath = Join-Path $InstallDir $ExeName
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.Description = "Hardware-aware Windows 11 gaming optimization"
$Shortcut.Save()

# Desktop shortcut
$Shortcut = $WshShell.CreateShortcut("$DesktopDir\$AppName.lnk")
$Shortcut.TargetPath = Join-Path $InstallDir $ExeName
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.Save()

# Add/Remove Programs
New-Item -Path $UninstallKey -Force | Out-Null
Set-ItemProperty -Path $UninstallKey -Name "DisplayName" -Value $AppName
Set-ItemProperty -Path $UninstallKey -Name "Publisher" -Value $Publisher
Set-ItemProperty -Path $UninstallKey -Name "DisplayVersion" -Value "1.0.0"
Set-ItemProperty -Path $UninstallKey -Name "InstallLocation" -Value $InstallDir
Set-ItemProperty -Path $UninstallKey -Name "UninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$InstallDir\uninstall.ps1`""
Set-ItemProperty -Path $UninstallKey -Name "NoModify" -Value 1 -Type DWord
Set-ItemProperty -Path $UninstallKey -Name "NoRepair" -Value 1 -Type DWord

# Copy uninstall script
Copy-Item -Force (Join-Path $SourceDir "uninstall.ps1") (Join-Path $InstallDir "uninstall.ps1")

Write-Host "`n  Installed to $InstallDir" -ForegroundColor Green
Write-Host "  Start Menu shortcut created" -ForegroundColor Green
Write-Host "  Desktop shortcut created" -ForegroundColor Green
Write-Host "  Visible in Add/Remove Programs" -ForegroundColor Green
Write-Host "`n  Launch from Start Menu or Desktop." -ForegroundColor Cyan
