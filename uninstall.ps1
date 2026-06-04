#requires -Version 5.1
[CmdletBinding()]
param(
    [switch]$RemoveSettings
)

$ErrorActionPreference = "Stop"

function Test-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Stop-ProjectAutoHotkey {
    $processes = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
        Where-Object {
            $_.Name -like "AutoHotkey*" -and
            $_.CommandLine -and
            $_.CommandLine.IndexOf("WinQ-OpenClickToDo.ahk", [StringComparison]::OrdinalIgnoreCase) -ge 0
        }

    foreach ($process in $processes) {
        Write-Host "Stopping Open Click to Do AutoHotkey process $($process.ProcessId)..."
        Invoke-CimMethod -InputObject $process -MethodName Terminate | Out-Null
    }
}

if (-not (Test-Administrator)) {
    Write-Error "Run PowerShell as Administrator, then run .\uninstall.ps1 again."
    exit 1
}

$installDir = Join-Path $env:LOCALAPPDATA "OpenClickToDo"
$startup = [Environment]::GetFolderPath("Startup")
$shortcutPath = Join-Path $startup "Open Click to Do WinQ.lnk"

Stop-ProjectAutoHotkey

if (Test-Path -LiteralPath $shortcutPath) {
    Remove-Item -LiteralPath $shortcutPath -Force
    Write-Host "Removed startup shortcut: $shortcutPath"
}

if (Test-Path -LiteralPath $installDir) {
    Remove-Item -LiteralPath $installDir -Recurse -Force
    Write-Host "Removed install directory: $installDir"
}

if ($RemoveSettings) {
    $settingsDir = Join-Path $env:APPDATA "OpenClickToDo"
    if (Test-Path -LiteralPath $settingsDir) {
        Remove-Item -LiteralPath $settingsDir -Recurse -Force
        Write-Host "Removed user settings: $settingsDir"
    }
} else {
    Write-Host "User settings were kept. Re-run with -RemoveSettings to delete %APPDATA%\OpenClickToDo."
}

Write-Host "Open Click to Do uninstalled."

