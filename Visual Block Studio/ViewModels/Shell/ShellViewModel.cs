using CommunityToolkit.Mvvm.ComponentModel;

namespace Visual_Block_Studio.ViewModels.Shell;

public partial class ShellViewModel(TabsFactoryViewModel tabs, SolutionViewModel solution) : ObservableObject
{
    public TabsFactoryViewModel Tabs { get; set; } = tabs;
    public SolutionViewModel Solution { get; set; } = solution;
}
