using System.Windows;

namespace MouseTranslator.App;

public partial class MainWindow : Window
{
    public MainWindow(string details)
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(details);
    }
}

public sealed record MainWindowViewModel(string Details);
