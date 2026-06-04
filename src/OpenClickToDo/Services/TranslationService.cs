using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using OpenClickToDo.Models;

namespace OpenClickToDo.Services;

public sealed class TranslationService
{
    private readonly BrowserTranslateService _browserTranslateService;
    private readonly LoggingService _logger;

    public TranslationService(BrowserTranslateService browserTranslateService, LoggingService logger)
    {
        _browserTranslateService = browserTranslateService;
        _logger = logger;
    }

    public async Task<string?> TranslateAsync(
        string text,
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        _logger.Info($"Translate action requested. Provider={settings.TranslationProvider}, TextLength={text.Length}.");

        switch (settings.TranslationProvider)
        {
            case TranslationProvider.Browser:
                _browserTranslateService.Translate(text, settings.BrowserTranslateUrlTemplate);
                return null;
            case TranslationProvider.Ollama:
                return await TranslateWithOllamaAsync(text, settings, cancellationToken);
            case TranslationProvider.DeepL:
            case TranslationProvider.OpenAI:
            case TranslationProvider.Gemini:
            case TranslationProvider.LibreTranslate:
                throw new NotSupportedException(
                    $"{settings.TranslationProvider} translation is a provider slot in this MVP. Use Browser or configure Ollama.");
            default:
                _browserTranslateService.Translate(text, settings.BrowserTranslateUrlTemplate);
                return null;
        }
    }

    private static async Task<string?> TranslateWithOllamaAsync(
        string text,
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        var endpoint = string.IsNullOrWhiteSpace(settings.OllamaEndpoint)
            ? "http://localhost:11434"
            : settings.OllamaEndpoint.TrimEnd('/');
        var model = string.IsNullOrWhiteSpace(settings.OllamaModel) ? "llama3.1" : settings.OllamaModel;
        var target = string.IsNullOrWhiteSpace(settings.TargetLanguage) ? "the user's language" : settings.TargetLanguage;

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
        var payload = new
        {
            model,
            stream = false,
            prompt = $"Translate the following text to {target}. Return only the translation.\n\n{text}"
        };

        using var response = await httpClient.PostAsJsonAsync(
            $"{endpoint}/api/generate",
            payload,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.TryGetProperty("response", out var value)
            ? value.GetString()
            : null;
    }
}
