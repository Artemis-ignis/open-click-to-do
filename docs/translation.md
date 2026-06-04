# Translation

The default MVP does not require an API key.

## Browser Translate

Browser Translate is the default provider. It URL-encodes selected text and opens a translation website in the default browser.

Privacy impact: selected text is sent to the translation website.

## DeepL

DeepL is a planned provider slot for users who want DeepL API translation.

Privacy impact: selected text would be sent to DeepL when configured.

## OpenAI

OpenAI is a planned provider slot for users who want model-based translation, rewriting, explanation, and related text actions.

Privacy impact: selected text would be sent to OpenAI when configured. API keys must never be hardcoded.

## Gemini

Gemini is a planned provider slot for users who want Google Gemini-based text actions.

Privacy impact: selected text would be sent to Gemini when configured. API keys must never be hardcoded.

## Ollama

Ollama is included as a local HTTP structure in the MVP. Configure `OllamaEndpoint`, `OllamaModel`, and `TargetLanguage` in `%APPDATA%\OpenClickToDo\settings.json`.

Default endpoint:

```json
{
  "TranslationProvider": "Ollama",
  "OllamaEndpoint": "http://localhost:11434",
  "OllamaModel": "llama3.1",
  "TargetLanguage": "Korean"
}
```

Privacy impact: text can stay local if Ollama and the selected model run locally.

## LibreTranslate

LibreTranslate is a planned provider slot for open-source translation servers.

Privacy impact: selected text is sent to the configured LibreTranslate server. A local server can keep text local.

## Logging Rule

Translation logs record provider and text length. They do not store the full original selected text.

