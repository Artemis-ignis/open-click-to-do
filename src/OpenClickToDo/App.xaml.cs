using System.Threading;
using System.Windows;
using OpenClickToDo.Services;

namespace OpenClickToDo;

public partial class App : System.Windows.Application
{
    private Mutex? _captureMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logger = new LoggingService();
        if (e.Args.Any(arg => string.Equals(arg, "--capture", StringComparison.OrdinalIgnoreCase)))
        {
            await RunCaptureModeAsync(logger);
            return;
        }

        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    private async Task RunCaptureModeAsync(LoggingService logger)
    {
        _captureMutex = new Mutex(false, @"Local\OpenClickToDo.CaptureOverlay");
        if (!_captureMutex.WaitOne(0))
        {
            logger.Info("Capture request ignored because another overlay is already running.");
            Shutdown();
            return;
        }

        try
        {
            var workflow = CaptureWorkflow.CreateDefault(logger);
            await workflow.RunAsync();
        }
        catch (Exception ex)
        {
            logger.Error("Capture mode failed.", ex);
            System.Windows.MessageBox.Show(
                "Open Click to Do could not start the capture overlay.\n\n" + ex.Message,
                "Open Click to Do",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _captureMutex.ReleaseMutex();
            _captureMutex.Dispose();
            Shutdown();
        }
    }
}
