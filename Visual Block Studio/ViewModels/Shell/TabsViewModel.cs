using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using Visual_Block_Studio.Messages;

namespace Visual_Block_Studio.ViewModels.Shell;

public partial class TabViewModel(string header, Type targetPageType, object? param = null) : ObservableObject
{
    [ObservableProperty]
    public partial string TabHeader { get; set; } = header;

    public Frame? ContentFrame { get; set; }
    public Type TargetPageType { get; } = targetPageType;
    public object? Parameter { get; } = param;
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

    public void OpenNewPageTab(string title, Type pageType, object parameter = null!)
    {
        var newTab = new TabViewModel(title, pageType, parameter);
        OpenTabs.Add(newTab);
        SelectedTab = newTab;
    }

    public void CloseTab(TabViewModel tab)
    {
        if (!OpenTabs.Remove(tab))
            return;

        if (ReferenceEquals(SelectedTab, tab))
            SelectedTab = OpenTabs.LastOrDefault()!;
    }
}