using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Visual_Block_Studio.Contracts;
using Visual_Block_Studio.Messages;
using Visual_Block_Studio.Models.Explorer;
using Visual_Block_Studio.Services;
using Visual_Block_Studio.Views;

namespace Visual_Block_Studio.ViewModels.Shell;

public partial class SolutionViewModel : ObservableObject
{
    private readonly SolutionManager SolutionManager;

    public SolutionExplorerViewModel SolutionExplorer { get; set; }

    [ObservableProperty]
    public partial Solution This { get; set; } = new();

    [ObservableProperty]
    public partial string SolutionName { get; set; } = null!;

    [ObservableProperty]
    public partial CreateSolutionRequest Request { get; set; } = new();

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = null!;

    [ObservableProperty]
    public partial bool ErrorOccured { get; set; }

    public SolutionViewModel(SolutionExplorerViewModel solutionExplorer, SolutionManager solutionManager)
    {
        this.SolutionManager = solutionManager;
        SolutionExplorer = solutionExplorer;

        This.Projects.CollectionChanged += Projects_CollectionChanged;
    }

    partial void OnThisChanging(Solution oldValue, Solution newValue)
    {
        oldValue.Projects.CollectionChanged -= Projects_CollectionChanged;

        foreach (var project in oldValue.Projects)
            SolutionExplorer.RemoveProjectItem(project);
    }

    partial void OnThisChanged(Solution value)
    {
        value.Projects.CollectionChanged += Projects_CollectionChanged;

        foreach (var project in value.Projects)
            SolutionExplorer.AddProject(project);
    }

    private void Projects_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var removed in e.OldItems.OfType<VBSProject>())
                SolutionExplorer.RemoveProjectItem(removed);
        }

        if (e.NewItems is not null)
        {
            foreach (var added in e.NewItems.OfType<VBSProject>())
                SolutionExplorer.AddProject(added);
        }
    }

    private void OpenBlocksTab(ProjectFile file)
    {
        WeakReferenceMessenger.Default.Send(new OpenTabRequestMessage(new OpenTabRequestArgs
        {
            Title = file.FileName,
            TargetPageType = typeof(BlocksPage),
            Parameter = file.FilePath
        }));
    }
    private void OpenTextEditorTab(ProjectFile file)
    {
        WeakReferenceMessenger.Default.Send(new OpenTabRequestMessage(new OpenTabRequestArgs
        {
            Title = file.FileName,
            TargetPageType = typeof(TextEditor),
            Parameter = file.FilePath
        }));
    }

    public async Task LoadSolutionAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("The file path cannot be null.");

            var slnx = await Task.Run(() => SolutionManager.Load(path));

            This = slnx;

            SolutionName = This.FileName.Replace(".vbsslnx", "");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex switch
            {
                ArgumentException => "No file was selected. Please choose a valid solution file.",
                FileNotFoundException => "The solution file couldn't be found. It may have been moved or deleted.",
                UnauthorizedAccessException => "Access denied. Check the file's permissions and try again.",
                InvalidOperationException => "This file doesn't look like a valid solution. It may be corrupted.",
                _ => "Something went wrong while opening the solution."
            };
            ErrorOccured = true;
        }
    }

    public async Task CreateSolutionAsync()
    {
        var slnx = await SolutionManager.CreateSolution(Request);
        This = slnx;
        SolutionName = This.FileName.Replace(".vbsslnx", "");
    }

    public async Task LoadFileAsync(ProjectFile file)
    {
        if (file.Type == Enums.ProjectFileType.Block)
            OpenBlocksTab(file);
        else
            OpenTextEditorTab(file);
    }
}

public partial class SolutionExplorerViewModel : ObservableObject
{
    public ObservableCollection<NavigationViewItem> ProjectMenuItems { get; } = [];

    public void AddProject(VBSProject project)
    {
        MenuFlyout contextMenu = CreateContextMenu(project);

        NavigationViewItem navItem = new()
        {
            Content = project.Name,
            Icon = new FontIcon
            {
                Glyph = project.FileIconGlyph,
                Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 100, 255, 150)),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
            },
            SelectsOnInvoked = false,
            Tag = project,
            ContextFlyout = contextMenu,
        };

        AddProjectItems([.. project.LayoutFiles], navItem);

        ProjectMenuItems.Add(navItem);
    }

    private MenuFlyout CreateContextMenu(object item)
    {
        MenuFlyout contextMenu = new();
        MenuFlyoutItem deleteItem = new()
        {
            Text = "Delete",
            Tag = item
        };

        MenuFlyoutItem renameItem = new()
        {
            Text = "Rename",
            Tag = item
        };
        MenuFlyoutItem copyItem = new()
        {
            Text = "Copy",
            Tag = item
        };
        MenuFlyoutItem cutItem = new()
        {
            Text = "Cut",
            Tag = item
        };

        deleteItem.Click += DeleteItem_Click;
        renameItem.Click += RenameItem_Click;
        copyItem.Click += CopyItem_Click;
        cutItem.Click += CutItem_Click;

        contextMenu.Items.Add(deleteItem);
        contextMenu.Items.Add(renameItem);
        contextMenu.Items.Add(copyItem);
        contextMenu.Items.Add(cutItem);

        return contextMenu;
    }

    private void AddProjectItems(List<ProjectItem> items, NavigationViewItem projView)
    {
        foreach (var item in items)
        {
            if (item.Files.Count == 0)
                continue;

            var contextMenu = CreateContextMenu(item);

            NavigationViewItem navItem;

            if (item.Files.Count == 1)
            {
                navItem = new()
                {
                    Content = item.Name,
                    Icon = new FontIcon { Glyph = "\uE8B7" },
                    Tag = item,
                    ContextFlyout = contextMenu,
                };
            }
            else
            {
                navItem = new()
                {
                    Content = item.Name,
                    Icon = new FontIcon { Glyph = "\uEA86" },
                    Tag = item,
                    ContextFlyout = contextMenu,
                    SelectsOnInvoked = false,
                };

                foreach (var file in item.Files)
                {
                    NavigationViewItem fileItem = new()
                    {
                        Content = file.FileName,
                        Tag = file,
                        ContextFlyout = contextMenu
                    };

                    navItem.MenuItems.Add(fileItem);
                }
            }

            projView.MenuItems.Add(navItem);
        }
    }

    private void CutItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is VBSProject item)
        {
            // TODO: cut — stage the project for a subsequent paste/move.
        }
    }

    private void CopyItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is VBSProject item)
        {
            // TODO: copy — stage the project for a subsequent paste/duplicate.
        }
    }

    private void RenameItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is VBSProject item)
        {
            // TODO: rename — prompt for a new name and update the project's files.
        }
    }

    private void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is VBSProject item)
        {
            // TODO: delete — remove the project's files and drop it from the solution.
        }
    }

    public void RemoveProjectItem(VBSProject item)
    {
        var toRemove = ProjectMenuItems.FirstOrDefault(n => n.Tag == item);
        if (toRemove != null)
            ProjectMenuItems.Remove(toRemove);
    }
}