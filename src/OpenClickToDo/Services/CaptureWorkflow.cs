using System.Windows;

namespace OpenClickToDo.Services;

public sealed class CaptureWorkflow
{
    private readonly ScreenCaptureService _screenCaptureService;
    private readonly OcrService _ocrService;
    private readonly ClipboardService _clipboardService;
    private readonly BrowserSearchService _searchService;
    private readonly BrowserTranslateService _browserTranslateService;
    private readonly TranslationService _translationService;
    private readonly OverlaySelectionService _selectionService;
    private readonly SettingsService _settingsService;
    private readonly LoggingService _logger;

    public CaptureWorkflow(
        ScreenCaptureService screenCaptureService,
        OcrService ocrService,
        ClipboardService clipboardService,
        BrowserSearchService searchService,
        BrowserTranslateService browserTranslateService,
        TranslationService translationService,
        OverlaySelectionService selectionService,
        SettingsService settingsService,
        LoggingService logger)
    {
        _screenCaptureService = screenCaptureService;
        _ocrService = ocrService;
        _clipboardService = clipboardService;
        _searchService = searchService;
        _browserTranslateService = browserTranslateService;
        _translationService = translationService;
        _selectionService = selectionService;
        _settingsService = settingsService;
        _logger = logger;
    }

    public static CaptureWorkflow CreateDefault(LoggingService? logger = null)
    {
        var loggingService = logger ?? new LoggingService();
        var browserTranslateService = new BrowserTranslateService();
        return new CaptureWorkflow(
            new ScreenCaptureService(loggingService),
            new OcrService(loggingService),
            new ClipboardService(),
            new BrowserSearchService(),
            browserTranslateService,
            new TranslationService(browserTranslateService, loggingService),
            new OverlaySelectionService(),
            new SettingsService(),
            loggingService);
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var settings = _settingsService.Load();
        using var screen = _screenCaptureService.CapturePrimaryScreen();
        IReadOnlyList<Models.OcrTextBlock> blocks = Array.Empty<Models.OcrTextBlock>();
        string? startupMessage = null;

        try
        {
            blocks = await _ocrService.RecognizeAsync(screen.Bitmap, cancellationToken);
            if (blocks.Count == 0)
            {
                startupMessage = "No text found on screen.";
            }
        }
        catch (Exception ex)
        {
            _logger.Error("OCR failed.", ex);
            startupMessage = "OCR failed. Check Windows OCR language support and app logs.";
        }

        var overlay = new OverlayWindow(
            screen,
            blocks,
            _clipboardService,
            _searchService,
            _translationService,
            _selectionService,
            _settingsService,
            _logger,
            settings,
            startupMessage);
        overlay.ShowDialog();
    }
}

