using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DailyManagementSystem.Views
{
    public partial class ClientReportView : UserControl
    {
        public ClientReportView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
