using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;

namespace WindowsTools
{
    [Export(typeof(IViewManager))]
    internal class ViewManager : Stylet.ViewManager
    {
        [ImportingConstructor]
        public ViewManager([Import] ViewManagerConfig config) : base(config)
        {
        }
    }
}
