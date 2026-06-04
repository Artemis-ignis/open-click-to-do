# How It Works

Open Click to Do is a separate Windows utility. It does not enable, patch, or modify Microsoft Click to Do.

## Flow

```text
Win+Q
  -> AutoHotkey v2
  -> OpenClickToDo.exe --capture
  -> primary screen screenshot
  -> Windows.Media.Ocr.OcrEngine
  -> OCR text lines and word boxes
  -> fullscreen overlay
  -> click OCR block
  -> action menu
  -> Copy / Translate / Search
```

## Runtime Pieces

- `scripts/WinQ-OpenClickToDo.ahk` registers the default `Win+Q` hotkey.
- `OpenClickToDo.exe --capture` starts the capture workflow immediately.
- `ScreenCaptureService` captures the primary monitor with `Graphics.CopyFromScreen`.
- `OcrService` uses `Windows.Media.Ocr.OcrEngine.TryCreateFromUserProfileLanguages()`, then falls back to English OCR when possible.
- `OverlayWindow` draws OCR text block bounds and lets the user click a block.
- `ClipboardService`, `BrowserTranslateService`, and `BrowserSearchService` run MVP actions.

## Difference From Microsoft Click to Do

Microsoft Click to Do is a Microsoft feature tied to supported Windows and Copilot+ experiences.

Open Click to Do is not a patch, bypass, unlocker, policy editor, service controller, or WindowsApps modifier. It is an independent app that creates a similar workflow using public Windows APIs and AutoHotkey.

## Current MVP Boundary

The MVP focuses on reliable primary-monitor capture, OCR block click selection, Copy, Browser Translate, Search Web, and safe install/uninstall scripts.

Drag selection, polished multi-monitor behavior, and fully inline translation are planned for later versions.

