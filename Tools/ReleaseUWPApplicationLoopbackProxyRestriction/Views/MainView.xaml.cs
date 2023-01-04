using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls.Primitives;

namespace ReleaseUWPApplicationLoopbackProxyRestriction.Views
{
    /// <summary>
    ///     MainView.xaml 的交互逻辑
    /// </summary>
    [Export]
    public partial class MainView : UserControl
    {
        private bool _isUpdating;

        public MainView()
        {
            InitializeComponent();
        }
    }
}