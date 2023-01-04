using System.ComponentModel.Composition;

namespace Tool.Core
{
    [InheritedExport]
    public abstract class ToolViewModelBase : ViewModelBase
    {
        protected ToolViewModelBase(string toolName)
        {
            DisplayName = toolName;
        }
    }
}