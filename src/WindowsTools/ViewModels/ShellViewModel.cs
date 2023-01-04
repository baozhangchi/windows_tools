using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tool.Core;

namespace WindowsTools.ViewModels
{
    [Export]
    internal class ShellViewModel : ViewModelBase
    {
        public List<ToolViewModelBase> Tools { get; }

        [ImportingConstructor]
        public ShellViewModel([ImportMany(typeof(ToolViewModelBase))]IEnumerable<ToolViewModelBase> tools)
        {
            DisplayName = "Windows工具箱";
            Tools = tools.ToList();
        }
    }
}
