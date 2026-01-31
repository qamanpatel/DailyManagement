using Avalonia.Controls;

namespace DailyManagementSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
    }
}
