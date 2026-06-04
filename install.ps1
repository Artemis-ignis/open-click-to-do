#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet("win-x64", "win-arm64")]
    [string]$Runtime = "win-x64",

    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"

function Test-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Get-AutoHotkeyExe {
    $candidates = @(
        "$env:ProgramFiles\AutoHotkey\v2\AutoHotkey64.exe",
        "$env:ProgramFiles\AutoHotkey\v2\AutoHotkey.exe",
        "$env:ProgramFiles\AutoHotkey\AutoHotkey64.exe",
        "$env:ProgramFiles\AutoHotkey\AutoHotkey.exe"
    )

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate)) {
            return $candidate
        }
    }

    $command = Get-Command AutoHotkey64.exe -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }

    $command = Get-Command AutoHotkey.exe -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }

    return $null
}

function Install-AutoHotkey {
    $winget = Get-Command winget.exe -ErrorAction SilentlyContinue
    if (-not $winget) {
        throw "AutoHotkey v2 was not found and winget is not available. Install AutoHotkey v2, then run install.ps1 again."
    }

    Write-Host "AutoHotkey v2 not found. Installing with winget..."
    & $winget.Source install --id AutoHotkey.AutoHotkey --exact --source winget --accept-source-agreements --accept-package-agreements
    if ($LASTEXITCODE -ne 0) {
        throw "winget failed to install AutoHotkey."
    }
}

function Stop-ProjectAutoHotkey {
    $processes = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
        Where-Object {
            $_.Name -like "AutoHotkey*" -and
            $_.CommandLine -and
            $_.CommandLine.IndexOf("WinQ-OpenClickToDo.ahk", [StringComparison]::OrdinalIgnoreCase) -ge 0
        }

    foreach ($process in $processes) {
        Write-Host "Stopping existing Open Click to Do AutoHotkey process $($process.ProcessId)..."
        Invoke-CimMethod -InputObject $process -MethodName Terminate | Out-Null
    }
}

if (-not (Test-Administrator)) {
    Write-Error "Run PowerShell as Administrator, then run .\install.ps1 again."
    exit 1
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $repoRoot "src\OpenClickToDo\OpenClickToDo.csproj"
$scriptSource = Join-Path $repoRoot "scripts\WinQ-OpenClickToDo.ahk"
$installDir = Join-Path $env:LOCALAPPDATA "OpenClickToDo"
$scriptsDir = Join-Path $installDir "scripts"
$publishDir = Join-Path $repoRoot "artifacts\publish\$Runtime"
$publishedExe = Join-Path $publishDir "OpenClickToDo.exe"

if (-not (Test-Path -LiteralPath $projectPath)) {
    throw "Project file not found: $projectPath"
}

if (-not (Test-Path -LiteralPath $scriptSource)) {
    throw "AutoHotkey script not found: $scriptSource"
}

$ahkExe = Get-AutoHotkeyExe
if (-not $ahkExe) {
    Install-AutoHotkey
    $ahkExe = Get-AutoHotkeyExe
}

if (-not $ahkExe) {
    throw "AutoHotkey v2 executable was not found after installation."
}

$dotnet = Get-Command dotnet.exe -ErrorAction SilentlyContinue
if (-not (Test-Path -LiteralPath $publishedExe)) {
    if (-not $dotnet) {
        throw ".NET SDK was not found and no published build exists at $publishedExe. Install .NET 8 SDK or place a published build there."
    }

    Write-Host "Publishing Open Click to Do..."
    $selfContainedArg = if ($SelfContained) { "true" } else { "false" }
    & $dotnet.Source publish $projectPath -c Release -r $Runtime --self-contained $selfContainedArg -o $publishDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed."
    }
}

if (-not (Test-Path -LiteralPath $publishedExe)) {
    throw "Published executable not found: $publishedExe"
}

Stop-ProjectAutoHotkey

New-Item -ItemType Directory -Force -Path $installDir, $scriptsDir | Out-Null
Copy-Item -Path (Join-Path $publishDir "*") -Destination $installDir -Recurse -Force
Copy-Item -LiteralPath $scriptSource -Destination (Join-Path $scriptsDir "WinQ-OpenClickToDo.ahk") -Force

$settingsDir = Join-Path $env:APPDATA "OpenClickToDo"
$settingsFile = Join-Path $settingsDir "settings.json"
if (-not (Test-Path -LiteralPath $settingsFile)) {
    New-Item -ItemType Directory -Force -Path $settingsDir | Out-Null
    @"
{
  "TranslationProvider": "Browser",
  "DefaultAction": "Copy",
  "BrowserTranslateUrlTemplate": "https://translate.google.com/?sl=auto&tl=auto&text={0}&op=translate",
  "SearchUrlTemplate": "https://www.google.com/search?q={0}",
  "OllamaEndpoint": "http://localhost:11434",
  "OllamaModel": "llama3.1",
  "TargetLanguage": "auto"
}
"@ | Set-Content -LiteralPath $settingsFile -Encoding UTF8
}

$startup = [Environment]::GetFolderPath("Startup")
$shortcutPath = Join-Path $startup "Open Click to Do WinQ.lnk"
$installedScript = Join-Path $scriptsDir "WinQ-OpenClickToDo.ahk"
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $ahkExe
$shortcut.Arguments = '"' + $installedScript + '"'
$shortcut.WorkingDirectory = $installDir
$shortcut.IconLocation = Join-Path $installDir "OpenClickToDo.exe"
$shortcut.Description = "Open Click to Do Win+Q hotkey"
$shortcut.Save()

Write-Host "Starting Open Click to Do Win+Q hotkey..."
Start-Process -FilePath $ahkExe -ArgumentList ('"' + $installedScript + '"') -WorkingDirectory $installDir -WindowStyle Hidden

Write-Host ""
Write-Host "Open Click to Do installed."
Write-Host "Install path: $installDir"
Write-Host "Hotkey: Win+Q"
