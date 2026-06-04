using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenClickToDo.Models;
using OpenClickToDo.Services;
using DrawingBitmap = System.Drawing.Bitmap;

namespace OpenClickToDo;

public partial class OverlayWindow : Window
{
    private readonly IReadOnlyList<OcrTextBlock> _blocks;
    private readonly CapturedScreen _screen;
    private readonly ClipboardService _clipboardService;
    private readonly BrowserSearchService _searchService;
    private readonly TranslationService _translationService;
    private readonly OverlaySelectionService _selectionService;
    private readonly SettingsService _settingsService;
    private readonly LoggingService _logger;
    private readonly AppSettings _settings;
    private readonly string? _startupMessage;
    private OcrTextBlock? _selectedBlock;

    public OverlayWindow(
        CapturedScreen screen,
        IReadOnlyList<OcrTextBlock> blocks,
        ClipboardService clipboardService,
        BrowserSearchService searchService,
        TranslationService translationService,
        OverlaySelectionService selectionService,
        SettingsService settingsService,
        LoggingService logger,
        AppSettings settings,
        string? startupMessage)
    {
        InitializeComponent();
        _screen = screen;
        _blocks = blocks;
        _clipboardService = clipboardService;
        _searchService = searchService;
        _translationService = translationService;
        _selectionService = selectionService;
        _settingsService = settingsService;
        _logger = logger;
        _settings = settings;
        _startupMessage = startupMessage;

        Left = screen.Bounds.Left;
        Top = screen.Bounds.Top;
        Width = screen.Bounds.Width;
        Height = screen.Bounds.Height;
        ScreenshotImage.Source = CreateBitmapSource(screen.Bitmap);
        Loaded += (_, _) =>
        {
            Focus();
            RenderBlocks();
            UpdateStatus();
        };
    }

    private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e) => RenderBlocks();

    private void RenderBlocks()
    {
        if (!IsLoaded || RootGrid.ActualWidth <= 0 || RootGrid.ActualHeight <= 0)
        {
            return;
        }

        OverlayCanvas.Children.Clear();
        var scaleX = RootGrid.ActualWidth / Math.Max(1, _screen.Bounds.Width);
        var scaleY = RootGrid.ActualHeight / Math.Max(1, _screen.Bounds.Height);

        foreach (var block in _blocks)
        {
            if (string.IsNullOrWhiteSpace(block.Text) || block.Bounds.Width <= 0 || block.Bounds.Height <= 0)
            {
                continue;
            }

            var selected = ReferenceEquals(block, _selectedBlock);
            var border = new Border
            {
                Tag = block,
                ToolTip = block.Text,
                BorderThickness = new Thickness(selected ? 3 : 1.4),
                BorderBrush = selected
                    ? System.Windows.Media.Brushes.White
                    : new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 96, 165, 250)),
                Background = selected
                    ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(72, 37, 99, 235))
                    : new SolidColorBrush(System.Windows.Media.Color.FromArgb(34, 14, 165, 233)),
                CornerRadius = new CornerRadius(3),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            border.MouseLeftButtonDown += OcrBlock_MouseLeftButtonDown;
            Canvas.SetLeft(border, block.Bounds.X * scaleX);
            Canvas.SetTop(border, block.Bounds.Y * scaleY);
            border.Width = Math.Max(8, block.Bounds.Width * scaleX);
            border.Height = Math.Max(8, block.Bounds.Height * scaleY);
            OverlayCanvas.Children.Add(border);
        }
    }

    private void OcrBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border { Tag: OcrTextBlock block })
        {
            _selectedBlock = block;
            SelectedTextPreview.Text = _selectionService.GetSelectedText(block);
            ActionPanel.Visibility = Visibility.Visible;
            StatusText.Text = "Selected text block. Choose an action, press Ctrl+C, Enter, or Esc.";
            RenderBlocks();
            e.Handled = true;
        }
    }

    private void UpdateStatus()
    {
        if (!string.IsNullOrWhiteSpace(_startupMessage))
        {
            StatusText.Text = _startupMessage;
            return;
        }

        StatusText.Text = _blocks.Count == 0
            ? "No text found on screen."
            : $"Found {_blocks.Count} OCR text block(s). Click a block to act on it.";
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e) => CopySelectedText();

    private async void TranslateButton_Click(object sender, RoutedEventArgs e)
    {
        await TranslateSelectedTextAsync();
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var text = GetSelectedTextOrWarn();
        if (text.Length == 0)
        {
            return;
        }

        _searchService.Search(text, _settings.SearchUrlTemplate);
        StatusText.Text = "Opened web search in the default browser.";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            return;
        }

        if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            CopySelectedText();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            ExecuteDefaultAction();
            e.Handled = true;
        }
    }

    private void ExecuteDefaultAction()
    {
        switch (_settings.DefaultAction)
        {
            case ActionCommand.Translate:
                _ = TranslateSelectedTextAsync();
                break;
            case ActionCommand.SearchWeb:
                SearchButton_Click(this, new RoutedEventArgs());
                break;
            case ActionCommand.Copy:
            default:
                CopySelectedText();
                break;
        }
    }

    private void CopySelectedText()
    {
        var text = GetSelectedTextOrWarn();
        if (text.Length == 0)
        {
            return;
        }

        _clipboardService.CopyText(text);
        StatusText.Text = "Copied selected text to clipboard.";
    }

    private async Task TranslateSelectedTextAsync()
    {
        var text = GetSelectedTextOrWarn();
        if (text.Length == 0)
        {
            return;
        }

        try
        {
            var inlineResult = await _translationService.TranslateAsync(text, _settings, CancellationToken.None);
            if (!string.IsNullOrWhiteSpace(inlineResult))
            {
                SelectedTextPreview.Text = inlineResult;
                StatusText.Text = "Translated with configured provider.";
                return;
            }

            StatusText.Text = "Opened translation in the default browser.";
        }
        catch (Exception ex)
        {
            _logger.Error("Translation action failed.", ex);
            StatusText.Text = ex.Message;
        }
    }

    private string GetSelectedTextOrWarn()
    {
        var text = _selectionService.GetSelectedText(_selectedBlock);
        if (text.Length == 0)
        {
            StatusText.Text = "Select an OCR text block first.";
        }

        return text;
    }

    private static BitmapSource CreateBitmapSource(DrawingBitmap bitmap)
    {
        var handle = bitmap.GetHbitmap();
        try
        {
            var source = Imaging.CreateBitmapSourceFromHBitmap(
                handle,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            DeleteObject(handle);
        }
    }

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);
}
