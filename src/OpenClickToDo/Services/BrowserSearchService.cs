using System.Diagnostics;
using System.Globalization;

namespace OpenClickToDo.Services;

public sealed class BrowserSearchService
{
    public void Search(string text, string searchUrlTemplate)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var encoded = Uri.EscapeDataString(text);
        var template = string.IsNullOrWhiteSpace(searchUrlTemplate)
            ? "https://www.google.com/search?q={0}"
            : searchUrlTemplate;
        OpenUrl(string.Format(CultureInfo.InvariantCulture, template, encoded));
    }

    private static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}

