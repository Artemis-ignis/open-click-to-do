using System.Diagnostics;
using System.IO;
using System.Windows;
using OpenClickToDo.Services;

namespace OpenClickToDo;

public partial class MainWindow : Window
{
    private readonly LoggingService _logger = new();
    private readonly SettingsService _settingsService = new();

    public MainWindow()
    {
        InitializeComponent();
        _settingsService.Load();
    }

    private async void CaptureNow_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "Starting capture overlay...";
            Hide();
            var workflow = CaptureWorkflow.CreateDefault(_logger);
            await workflow.RunAsync();
        }
        catch (Exception ex)
        {
            _logger.Error("Manual capture failed.", ex);
            System.Windows.MessageBox.Show(ex.Message, "Open Click to Do", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Show();
            Activate();
            StatusText.Text = "Capture overlay closed.";
        }
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        var path = _settingsService.SettingsFilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (!File.Exists(path))
        {
            _settingsService.Save(_settingsService.Load());
        }

        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
    }

    private void OpenLogs_Click(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(_logger.LogDirectory);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{_logger.LogDirectory}\"") { UseShellExecute = true });
    }
}
