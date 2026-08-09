using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using Visual_Block_Studio.Messages;
using Visual_Block_Studio.Views;

namespace Visual_Block_Studio.ViewModels.Shell;

public partial class TabViewModel(string header, System.Type targetPageType, object navParams = null!) : ObservableObject
{
    [ObservableProperty]
    public partial string TabHeader { get; set; } = header;

    public System.Type TargetPageType { get; } = targetPageType ?? typeof(BlocksPage);
    public object NavigationParameter { get; } = navParams;
}

public partial class TabsFactoryViewModel : ObservableObject
{
    public ObservableCollection<TabViewModel> OpenTabs { get; } = [];

    [ObservableProperty]
    public partial TabViewModel SelectedTab { get; set; } = default!;

    public TabsFactoryViewModel()
    {
        WeakReferenceMessenger.Default.Register<OpenTabRequestMessage>(this, (recipient, message) =>
        {
            var args = message.Value;
            OpenNewPageTab(args.Title, args.TargetPageType, args.Parameter);
        });
    }

    public void OpenNewPageTab(string title, System.Type pageType, object parameter = null!)
    {
        var newTab = new TabViewModel(title, pageType, parameter);
        OpenTabs.Add(newTab);
        SelectedTab = newTab;
    }

    public void CloseTab(TabViewModel tab)
    {
        OpenTabs.Remove(tab);
    }
}