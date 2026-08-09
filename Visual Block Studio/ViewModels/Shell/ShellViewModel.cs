using CommunityToolkit.Mvvm.ComponentModel;

namespace Visual_Block_Studio.ViewModels.Shell
{
    public partial class ShellViewModel(TabsFactoryViewModel tabs, SolutionViewModel solution, ToolBoxViewModel toolBox) : ObservableObject
    {
        public TabsFactoryViewModel Tabs { get; set; } = tabs;
        public SolutionViewModel Solution { get; set; } = solution;
        public ToolBoxViewModel ToolBox { get; set; } = toolBox;
    }
}
