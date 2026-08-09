using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml.Markup;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Visual_Block_Studio.Contracts;
using Visual_Block_Studio.Messages;
using Visual_Block_Studio.Models.Explorer;
using Visual_Block_Studio.Services;
using Visual_Block_Studio.Views;
using Windows.UI;

namespace Visual_Block_Studio.ViewModels.Shell;

public partial class SolutionViewModel : ObservableObject
{
    public SolutionExplorerViewModel SolutionExplorer { get; set; }
    //private VBSProjectManager projectManager;
    private readonly SolutionManager SolutionManager;

    [ObservableProperty]
    public partial Solution Solution { get; set; } = new();

    [ObservableProperty]
    public partial string SolutionName { get; set; } = null!;

    [ObservableProperty]
    public partial CreateSolutionRequest Request { get; set; } = new();

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = null!;

    [ObservableProperty]
    public partial bool ErrorOccured { get; set; }

    public SolutionViewModel(SolutionExplorerViewModel solutionExplorer,
        //VBSProjectManager projectManager, 
        SolutionManager solutionManager)
    {
        this.SolutionManager = solutionManager;
        //this.projectManager = projectManager;
        SolutionExplorer = solutionExplorer;

        // Wires the *initial* default Solution instance. Reassignments (from
        // Load/CreateSolution) are handled by OnSolutionChanging/OnSolutionChanged
        // below, since field initializers bypass the generated property setter.
        Solution.Projects.CollectionChanged += Projects_CollectionChanged;
    }

    partial void OnSolutionChanging(Solution oldValue, Solution newValue)
    {
        oldValue.Projects.CollectionChanged -= Projects_CollectionChanged;

        foreach (var project in oldValue.Projects)
            SolutionExplorer.RemoveProjectItem(project);
    }

    partial void OnSolutionChanged(Solution value)
    {
        value.Projects.CollectionChanged += Projects_CollectionChanged;

        // CollectionChanged only fires on *future* mutations, so the projects
        // already present right after a load/create need to be added explicitly.
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

            Solution = slnx;
            SolutionName = Solution.FileName.Replace(".vbsslnx", "");
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
        var slnx = await Task.Run(() => SolutionManager.CreateSolution(Request));
        Solution = slnx;
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
        MenuFlyout contextMenu = new();
        MenuFlyoutItem deleteItem = new()
        {
            Text = "Delete",
            Tag = project
        };

        MenuFlyoutItem renameItem = new()
        {
            Text = "Rename",
            Tag = project
        };
        MenuFlyoutItem copyItem = new()
        {
            Text = "Copy",
            Tag = project
        };
        MenuFlyoutItem cutItem = new()
        {
            Text = "Cut",
            Tag = project
        };

        deleteItem.Click += DeleteItem_Click;
        renameItem.Click += RenameItem_Click;
        copyItem.Click += CopyItem_Click;
        cutItem.Click += CutItem_Click;

        contextMenu.Items.Add(deleteItem);
        contextMenu.Items.Add(renameItem);
        contextMenu.Items.Add(copyItem);
        contextMenu.Items.Add(cutItem);

        NavigationViewItem navItem = new()
        {
            Content = project.Name,
            Icon = new FontIcon
            {
                Glyph = project.FileIconGlyph,
                Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 155, 79, 150)),
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

    private void AddProjectItems(List<ProjectItem> items, NavigationViewItem projView)
    {
        foreach (var item in items)
        {
            if (item.Files.Count == 0)
                continue;

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

            NavigationViewItem navItem;

            if (item.Files.Count == 1)
            {
                IconElement icon;

                switch (item.Type)
                {
                    case Enums.ProjectItemType.Folder:
                        icon = new FontIcon { Glyph = "\uE8B7" };
                        break;
                    case Enums.ProjectItemType.SingleXamlFile:
                        {
                            string pathData = "M 37 44 L 23 64 L 37 84 " +
                                          "M 91 44 L 105 64 L 91 84 " +
                                          "M 64 42 a 22 22 0 1 0 0.001 0 Z";

                            icon = new PathIcon()
                            {
                                Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathData),
                                Foreground = new SolidColorBrush(Color.FromArgb(255, 142, 142, 147))
                            };
                        }
                        break;
                    case Enums.ProjectItemType.SingleCppFile:
                        {
                            string pathData = "M63.443 0c-1.782 0-3.564.39-4.916 1.172L11.594 28.27C8.89 29.828 6.68 33.66 6.68 36.78v54.197c0 1.562.55 3.298 1.441 4.841l-.002.002c.89 1.543 2.123 2.89 3.475 3.672l46.931 27.094c2.703 1.562 7.13 1.562 9.832 0h.002l46.934-27.094c1.352-.78 2.582-2.129 3.473-3.672.89-1.543 1.441-3.28 1.441-4.843V36.779c0-1.557-.55-3.295-1.441-4.838v-.002c-.891-1.545-2.121-2.893-3.473-3.67L68.359 1.173C67.008.39 65.226 0 63.443 0zm.002 26.033c13.465 0 26.02 7.246 32.77 18.91l-16.38 9.479c-3.372-5.836-9.66-9.467-16.39-9.467-10.432 0-18.922 8.49-18.922 18.924S53.013 82.8 63.445 82.8c6.735 0 13.015-3.625 16.395-9.465l16.375 9.477c-6.746 11.662-19.305 18.91-32.77 18.91-20.867 0-37.843-16.977-37.843-37.844s16.976-37.844 37.843-37.844v-.002zM92.881 57.57h4.201v4.207h4.203v4.203h-4.203v4.207h-4.201V65.98h-4.207v-4.203h4.207V57.57zm15.765 0h4.208v4.207h4.203v4.203h-4.203v4.207h-4.208V65.98h-4.205v-4.203h4.205V57.57z";
                            icon = new PathIcon()
                            {
                                Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathData),
                                Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 68, 130))
                            };
                        }
                        break;
                    case Enums.ProjectItemType.SingleHeaderFile:
                        {
                            string pathData = "M 28 8 C 21.373 8 16 13.373 16 20 L 16 108 C 16 114.627 21.373 120 28 120 L 100 120 C 106.627 120 112 114.627 112 108 L 112 40 L 80 8 Z " +
                            "M 44 44 L 56 44 L 56 60 L 72 60 L 72 44 L 84 44 L 84 84 L 72 84 L 72 68 L 56 68 L 56 84 L 44 84 Z";

                            icon = new PathIcon()
                            {
                                Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathData),
                                Foreground = new SolidColorBrush(Color.FromArgb(255, 104, 33, 122))
                            };
                        }
                        break;
                    case Enums.ProjectItemType.SingleIdlFile:
                        icon = new FontIcon { Glyph = "\uE7C3" };
                        break;
                    default:
                        {
                            icon = new PathIcon()
                            {

                            };
                        }
                        break;
                }

                navItem = new()
                {
                    Content = item.Name,
                    Icon = icon,
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
                    IconElement icon;

                    switch (file.Type)
                    {
                        case Enums.ProjectFileType.Xaml:
                            {
                                string pathData = "M 37 44 L 23 64 L 37 84 " +
                                          "M 91 44 L 105 64 L 91 84 " +
                                          "M 64 42 a 22 22 0 1 0 0.001 0 Z";
                                icon = new PathIcon()
                                {
                                    Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathData),
                                    Foreground = new SolidColorBrush(Color.FromArgb(255, 142, 142, 147))
                                };
                            }
                            break;
                        case Enums.ProjectFileType.Cpp:
                            {
                                string pathData = "M63.443 0c-1.782 0-3.564.39-4.916 1.172L11.594 28.27C8.89 29.828 6.68 33.66 6.68 36.78v54.197c0 1.562.55 3.298 1.441 4.841l-.002.002c.89 1.543 2.123 2.89 3.475 3.672l46.931 27.094c2.703 1.562 7.13 1.562 9.832 0h.002l46.934-27.094c1.352-.78 2.582-2.129 3.473-3.672.89-1.543 1.441-3.28 1.441-4.843V36.779c0-1.557-.55-3.295-1.441-4.838v-.002c-.891-1.545-2.121-2.893-3.473-3.67L68.359 1.173C67.008.39 65.226 0 63.443 0zm.002 26.033c13.465 0 26.02 7.246 32.77 18.91l-16.38 9.479c-3.372-5.836-9.66-9.467-16.39-9.467-10.432 0-18.922 8.49-18.922 18.924S53.013 82.8 63.445 82.8c6.735 0 13.015-3.625 16.395-9.465l16.375 9.477c-6.746 11.662-19.305 18.91-32.77 18.91-20.867 0-37.843-16.977-37.843-37.844s16.976-37.844 37.843-37.844v-.002zM92.881 57.57h4.201v4.207h4.203v4.203h-4.203v4.207h-4.201V65.98h-4.207v-4.203h4.207V57.57zm15.765 0h4.208v4.207h4.203v4.203h-4.203v4.207h-4.208V65.98h-4.205v-4.203h4.205V57.57z";
                                icon = new PathIcon()
                                {
                                    Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathData),
                                    Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 68, 130))
                                };
                            }
                            break;
                        case Enums.ProjectFileType.Header:
                            {
                                string pathData = "M 28 8 C 21.373 8 16 13.373 16 20 L 16 108 C 16 114.627 21.373 120 28 120 L 100 120 C 106.627 120 112 114.627 112 108 L 112 40 L 80 8 Z " +
                                    "M 44 44 L 56 44 L 56 60 L 72 60 L 72 44 L 84 44 L 84 84 L 72 84 L 72 68 L 56 68 L 56 84 L 44 84 Z";

                                icon = new PathIcon()
                                {
                                    Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathData),
                                    Foreground = new SolidColorBrush(Color.FromArgb(255, 104, 33, 122))
                                };
                            }
                            break;
                        case Enums.ProjectFileType.Idl:
                            icon = new FontIcon { Glyph = "\uE7C3" };
                            break;
                        default:
                            {
                                icon = new PathIcon()
                                {

                                };
                            }
                            break;
                    }

                    NavigationViewItem fileItem = new()
                    {
                        Content = file.FileName,
                        Icon = icon,
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