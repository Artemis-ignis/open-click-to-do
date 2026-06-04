using System.Diagnostics;
using System.Globalization;

namespace OpenClickToDo.Services;

public sealed class BrowserTranslateService
{
    public void Translate(string text, string translateUrlTemplate)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var encoded = Uri.EscapeDataString(text);
        var template = string.IsNullOrWhiteSpace(translateUrlTemplate)
            ? "https://translate.google.com/?sl=auto&tl=auto&text={0}&op=translate"
            : translateUrlTemplate;
        OpenUrl(string.Format(CultureInfo.InvariantCulture, template, encoded));
    }

    private static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}

