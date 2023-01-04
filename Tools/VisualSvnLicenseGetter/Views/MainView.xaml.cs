#region FileHeader

// // Project:  VisualSvnLicenceGetter
// // File:  MainView.xaml.cs
// // CreateTime:  2023-01-03 17:50
// // LastUpdateTime:  2023-01-03 17:52

#endregion

#region Nmaespaces

using System.ComponentModel.Composition;
using System.Windows.Controls;

#endregion

namespace VisualSvnLicenseGetter.Views
{
    /// <summary>
    ///     MainView.xaml 的交互逻辑
    /// </summary>
    [Export]
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }
    }
}