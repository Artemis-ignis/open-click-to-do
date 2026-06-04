# Privacy

Open Click to Do is designed to keep the default OCR and copy workflow local.

## Local Data

- Screen capture is processed in memory for OCR.
- OCR runs through Windows OCR APIs on the local machine.
- Copy writes selected text to the local Windows clipboard.
- Settings are stored at `%APPDATA%\OpenClickToDo\settings.json`.
- Logs are stored at `%LOCALAPPDATA%\OpenClickToDo\logs\app.log`.

## External Transmission

Selected text may leave the device in these cases:

- Browser Translate opens a translation website with selected text in the URL.
- DeepL, OpenAI, Gemini, or LibreTranslate providers send selected text to the configured provider when implemented and enabled.
- Search Web opens the default browser search engine with selected text as the query.

## Local Provider Option

Ollama can keep translation local when:

- Ollama runs on the same machine or a trusted local network endpoint.
- The configured model runs locally.
- The endpoint is not routed to a remote service.

## Logging Principle

Logs should help diagnose failures without capturing private content. Translation logging records provider and text length, not the full selected source text.

## What This App Does Not Touch

Open Click to Do does not modify:

- Microsoft Click to Do
- Copilot settings
- WSAIFabricSvc
- WorkloadsSessionHost
- WindowsWorkload packages
- WindowsApps folders
- DisableClickToDo policy

