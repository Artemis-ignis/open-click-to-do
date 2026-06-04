using System.IO;

namespace OpenClickToDo.Services;

public sealed class LoggingService
{
    private readonly object _sync = new();

    public string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenClickToDo",
        "logs");

    public string LogFilePath => Path.Combine(LogDirectory, "app.log");

    public void Info(string message) => Write("INFO", message);

    public void Error(string message, Exception? exception = null)
    {
        var detail = exception is null
            ? message
            : $"{message} {exception.GetType().Name}: {exception.Message}";
        Write("ERROR", detail);
    }

    private void Write(string level, string message)
    {
        lock (_sync)
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(
                LogFilePath,
                $"{DateTimeOffset.Now:O} [{level}] {message}{Environment.NewLine}");
        }
    }
}

