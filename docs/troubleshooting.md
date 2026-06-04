# Troubleshooting

Use Windows PowerShell for these commands unless noted.

## Win+Q Does Not Work

Check whether the Open Click to Do AutoHotkey script is running:

```powershell
Get-CimInstance Win32_Process | Where-Object {
  $_.Name -like "AutoHotkey*" -and $_.CommandLine -like "*WinQ-OpenClickToDo.ahk*"
} | Select-Object ProcessId, Name, CommandLine
```

If it is not running, reinstall:

```powershell
Set-ExecutionPolicy -Scope Process Bypass -Force
.\install.ps1
```

Another app may already use `Win+Q`. Edit `scripts\WinQ-OpenClickToDo.ahk` and change `#q` to another AutoHotkey v2 hotkey such as `#+q`.

## AutoHotkey Does Not Start

Check AutoHotkey:

```powershell
Get-Command AutoHotkey64.exe, AutoHotkey.exe -ErrorAction SilentlyContinue
```

If missing, install AutoHotkey v2:

```powershell
winget install --id AutoHotkey.AutoHotkey --exact --source winget
```

## OCR Result Is Empty

Open an image or page with larger, clearer text and test again.

Check app logs:

```powershell
Get-Content -Tail 120 -Encoding UTF8 "$env:LOCALAPPDATA\OpenClickToDo\logs\app.log"
```

OCR accuracy depends on Windows OCR language support, font clarity, contrast, and image resolution.

## Korean OCR Does Not Work

Install Korean language OCR support in Windows Settings:

```text
Settings -> Time & language -> Language & region -> Korean -> Language options
```

Install language features that include OCR, then sign out or restart if Windows asks.

Check available OCR languages in the log:

```powershell
Get-Content -Tail 120 -Encoding UTF8 "$env:LOCALAPPDATA\OpenClickToDo\logs\app.log"
```

## Translation Does Not Open

Check `BrowserTranslateUrlTemplate` in:

```powershell
notepad "$env:APPDATA\OpenClickToDo\settings.json"
```

The default value is:

```json
"BrowserTranslateUrlTemplate": "https://translate.google.com/?sl=auto&tl=auto&text={0}&op=translate"
```

The selected text is URL-encoded and inserted into `{0}`.

## App Opens Multiple Times

The WPF app uses a capture mutex to avoid multiple active overlays. If AutoHotkey is duplicated, stop only this project's script:

```powershell
Get-CimInstance Win32_Process | Where-Object {
  $_.Name -like "AutoHotkey*" -and $_.CommandLine -like "*WinQ-OpenClickToDo.ahk*"
} | ForEach-Object {
  Invoke-CimMethod -InputObject $_ -MethodName Terminate
}
```

Then reinstall:

```powershell
.\install.ps1
```

## Administrator Permission Error

Run PowerShell as Administrator, then run:

```powershell
Set-ExecutionPolicy -Scope Process Bypass -Force
.\install.ps1
```

The installer checks administrator permission because it may install AutoHotkey v2 with winget and writes a startup shortcut.

## Execution Policy Error

Use a process-scoped bypass:

```powershell
Set-ExecutionPolicy -Scope Process Bypass -Force
.\install.ps1
```

This changes execution policy only for the current PowerShell process.

