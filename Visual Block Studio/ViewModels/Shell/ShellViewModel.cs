using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Visual_Block_Studio.Messages;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.ViewModels.Shell;

public partial class ShellViewModel(TabsFactoryViewModel tabs) : ObservableObject
{
    public List<XamlDesignFile> Files { get; } = [];
    public TabsFactoryViewModel Tabs { get; set; } = tabs;

    [RelayCommand]
    public void Save()
    {
        if (Tabs.SelectedTab.Parameter is not Recent recent)
            return;

        WeakReferenceMessenger.Default.Send(new SaveMessage(recent.Id));
    }

    [RelayCommand]
    public void SaveAll()
    {
        WeakReferenceMessenger.Default.Send(new SaveAllMessage());
    }
}
