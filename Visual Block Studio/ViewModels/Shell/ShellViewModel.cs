using CommunityToolkit.Mvvm.ComponentModel;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.ViewModels.Shell;

public partial class ShellViewModel(TabsFactoryViewModel tabs) : ObservableObject
{
    public List<XamlDesignFile> Files { get; } = [];
    public TabsFactoryViewModel Tabs { get; set; } = tabs;
}
