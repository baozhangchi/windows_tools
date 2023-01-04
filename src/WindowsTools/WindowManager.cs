using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;

namespace WindowsTools
{
    [Export(typeof(IWindowManager))]
    internal class WindowManager:Stylet.WindowManager
    {
        [ImportingConstructor]
        public WindowManager([Import]IViewManager viewManager, [Import] Func<IMessageBoxViewModel> messageBoxViewModelFactory, [Import] IWindowManagerConfig config) : base(viewManager, messageBoxViewModelFactory, config)
        {
        }
    }
}
