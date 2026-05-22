using System.Windows;
using System.Windows.Media;
using MouseTranslator.Core.Overlay;

namespace MouseTranslator.App;

public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
    }

    public void ShowRequest(
        OverlayRequest request,
        OverlayPlacement placement,
        double maxWidth,
        double maxHeight)
    {
        RootBorder.MaxWidth = maxWidth;
        ContentScrollViewer.MaxHeight = maxHeight;
        OriginalTextBlock.Text = string.Empty;
        OriginalTextBlock.Visibility = Visibility.Collapsed;

        TranslatedTextBlock.Text = request.TranslatedText;
        TranslatedTextBlock.Foreground = request.IsError
            ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xCC, 0x80))
            : System.Windows.Media.Brushes.White;

        if (!IsVisible)
        {
            Left = -10000;
            Top = -10000;
            Show();
        }

        UpdateLayout();
        Left = placement.X;
        Top = placement.Y;
    }
}
