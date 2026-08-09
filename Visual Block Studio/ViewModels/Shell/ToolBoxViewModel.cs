using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Visual_Block_Studio.Enums;
using Visual_Block_Studio.Messages;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Views;

namespace Visual_Block_Studio.ViewModels;

public partial class ToolBoxViewModel : ObservableObject
{
    public ObservableCollection<ToolboxItem> Items { get; set; } = [];

    public ToolBoxExplorerViewModel ToolBoxExplorer { get; set; }

    public ToolBoxViewModel(ToolBoxExplorerViewModel toolBarExplorer)
    {
        ToolBoxExplorer = toolBarExplorer;
        Items.CollectionChanged += OnItemsCollectionChanged; ;
    }

    private void OnItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (ToolboxItem item in e.NewItems!)
                    ToolBoxExplorer.AddToolbarItem(item);
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (ToolboxItem item in e.OldItems!)
                    ToolBoxExplorer.RemoveToolbarItem(item);
                break;

            case NotifyCollectionChangedAction.Reset:
                ToolBoxExplorer.ToolBoxMenuItems.Clear();
                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (ToolboxItem item in e.OldItems!)
                    ToolBoxExplorer.RemoveToolbarItem(item);
                foreach (ToolboxItem item in e.NewItems!)
                    ToolBoxExplorer.AddToolbarItem(item);
                break;
        }
    }

    public async Task LoadToolBarItemsAsync()
    {
        Items.Clear();

        Items.Add(new()
        {
            Type = ToolbarItemType.Xaml,
            Name = "Window block",
            Glyph = "\uE737",
            Block = new WindowBlock
            {
                Children = [],
                BaseClass = "Window",
                Namespace = "Hello",
                WindowName = "Window",
                Title = "Window",
            }
        });
    }
}

public partial class ToolBoxExplorerViewModel : ObservableObject
{
    public ObservableCollection<NavigationViewItem> ToolBoxMenuItems { get; } = new();

    public void AddToolbarItem(ToolboxItem item)
    {
        MenuFlyout contextMenu = new();
        MenuFlyoutItem addBlockItem = new()
        {
            Text = "Add To Workspace",
            Tag = item
        };

        addBlockItem.Click += ContextAddBlock_Click;
        contextMenu.Items.Add(addBlockItem);

        var navItem = new NavigationViewItem
        {
            Content = item.Name,
            Icon = new FontIcon { Glyph = item.Glyph },
            Tag = item,
            ContextFlyout = contextMenu
        };
        ToolBoxMenuItems.Add(navItem);
    }

    private void ContextAddBlock_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is ToolboxItem item)
        {
            WeakReferenceMessenger.Default.Send(new AddBlockRequestMessage(new AddBlockRequestArgs
            {
                Block = item.Block
            }));
        }
    }

    public void RemoveToolbarItem(ToolboxItem item)
    {
        var toRemove = ToolBoxMenuItems.FirstOrDefault(n => n.Tag == item);
        if (toRemove != null)
            ToolBoxMenuItems.Remove(toRemove);
    }
}