using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace OpenClickToDo.Services;

public sealed record CapturedScreen(Bitmap Bitmap, Rectangle Bounds) : IDisposable
{
    public void Dispose() => Bitmap.Dispose();
}

public sealed class ScreenCaptureService
{
    private readonly LoggingService _logger;

    public ScreenCaptureService(LoggingService logger)
    {
        _logger = logger;
    }

    public CapturedScreen CapturePrimaryScreen()
    {
        var screen = Screen.PrimaryScreen
            ?? throw new InvalidOperationException("No primary display was found.");

        var bounds = screen.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new InvalidOperationException("Primary display bounds are invalid.");
        }

        var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        try
        {
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(
                bounds.Left,
                bounds.Top,
                0,
                0,
                bounds.Size,
                CopyPixelOperation.SourceCopy);
            _logger.Info($"Captured primary screen {bounds.Width}x{bounds.Height}.");
            return new CapturedScreen(bitmap, bounds);
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }
}

