using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenClickToDo.Models;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using WpfRect = System.Windows.Rect;

namespace OpenClickToDo.Services;

public sealed class OcrService
{
    private readonly LoggingService _logger;

    public OcrService(LoggingService logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<OcrTextBlock>> RecognizeAsync(Bitmap bitmap, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var engine = CreateEngine();
        if (engine is null)
        {
            throw new InvalidOperationException(
                "Windows OCR is not available. Install a Windows OCR language pack, then try again.");
        }

        LogAvailableLanguages(engine);

        using var randomAccessStream = new InMemoryRandomAccessStream();
        await WriteBitmapToStreamAsync(bitmap, randomAccessStream, cancellationToken);
        randomAccessStream.Seek(0);

        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
        using var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied);

        cancellationToken.ThrowIfCancellationRequested();
        var result = await engine.RecognizeAsync(softwareBitmap);
        return ConvertResult(result, engine.RecognizerLanguage?.LanguageTag);
    }

    private OcrEngine? CreateEngine()
    {
        var engine = OcrEngine.TryCreateFromUserProfileLanguages();
        if (engine is not null)
        {
            return engine;
        }

        try
        {
            return OcrEngine.TryCreateFromLanguage(new Language("en-US"));
        }
        catch (Exception ex)
        {
            _logger.Error("English OCR engine fallback failed.", ex);
            return null;
        }
    }

    private void LogAvailableLanguages(OcrEngine engine)
    {
        try
        {
            var languages = OcrEngine.AvailableRecognizerLanguages
                .Select(language => language.LanguageTag)
                .DefaultIfEmpty("none");
            _logger.Info("OCR language: " + (engine.RecognizerLanguage?.LanguageTag ?? "unknown"));
            _logger.Info("Available OCR languages: " + string.Join(", ", languages));
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to list OCR languages.", ex);
        }
    }

    private static async Task WriteBitmapToStreamAsync(
        Bitmap bitmap,
        IRandomAccessStream randomAccessStream,
        CancellationToken cancellationToken)
    {
        await using var pngStream = new MemoryStream();
        bitmap.Save(pngStream, ImageFormat.Png);
        pngStream.Position = 0;

        cancellationToken.ThrowIfCancellationRequested();
        using var writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0));
        writer.WriteBytes(pngStream.ToArray());
        await writer.StoreAsync();
        await writer.FlushAsync();
        writer.DetachStream();
    }

    private static IReadOnlyList<OcrTextBlock> ConvertResult(OcrResult result, string? sourceLanguage)
    {
        var blocks = new List<OcrTextBlock>();
        foreach (var line in result.Lines)
        {
            var words = line.Words
                .Select(word => new OcrWordBox(word.Text, ToWpfRect(word.BoundingRect)))
                .Where(word => !string.IsNullOrWhiteSpace(word.Text))
                .ToArray();

            if (words.Length == 0)
            {
                continue;
            }

            var text = string.IsNullOrWhiteSpace(line.Text)
                ? string.Join(" ", words.Select(word => word.Text))
                : line.Text.Trim();

            blocks.Add(new OcrTextBlock
            {
                Text = text,
                Bounds = Combine(words.Select(word => word.Bounds)),
                Confidence = null,
                SourceLanguage = sourceLanguage,
                Words = words
            });
        }

        return blocks;
    }

    private static WpfRect ToWpfRect(Windows.Foundation.Rect rect)
    {
        return new WpfRect(rect.X, rect.Y, rect.Width, rect.Height);
    }

    private static WpfRect Combine(IEnumerable<WpfRect> rects)
    {
        var hasRect = false;
        var combined = WpfRect.Empty;
        foreach (var rect in rects)
        {
            if (!hasRect)
            {
                combined = rect;
                hasRect = true;
                continue;
            }

            combined.Union(rect);
        }

        return hasRect ? combined : WpfRect.Empty;
    }
}
