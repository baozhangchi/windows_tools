using System.Windows;
using Telerik.Windows.Controls;

namespace WindowsTools.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : RadTabbedWindow
    {
        public ShellView()
        {
            InitializeComponent();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            Application.Current.Shutdown(0);
        }
    }
}
