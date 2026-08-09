using Microsoft.UI;

namespace Visual_Block_Studio.Controls;

internal class SidebarItem
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Icon { get; set; } = "";
}

// Represents a row in the main grid: either a folder to navigate into,
// or a selectable file matching AllowedExtensions.
internal class FileSystemEntry
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool IsDirectory { get; set; }
    public string IconGlyph { get; set; } = "";
    public SolidColorBrush IconColor { get; set; } = new(Colors.SteelBlue);
}

public sealed partial class FilePickerDialog : ContentDialog
{
    private static readonly string[] AllowedExtensions = { ".vbsproj", ".slnx", ".vbsslnx", ".vcxproj" };

    private static readonly SolidColorBrush FolderColor = new(Colors.Goldenrod);
    private static readonly SolidColorBrush FileColor = new(Colors.SteelBlue);

    public string SelectedPath { get; private set; } = @"C:\repo\source";
    private Stack<string> _history = new();

    public FilePickerDialog()
    {
        InitializeComponent();

        LoadSidebar();
        LoadDirectory(SelectedPath, saveToHistory: false);
    }

    // -- File picker code ------------------------------------

    private void LoadSidebar()
    {
        var items = new List<SidebarItem>
        {
            new() { Name = "This PC", Path = "THIS_PC", Icon = "\xE9A1" },
            new() { Name = "Local Disk (C:)", Path = @"C:\", Icon = "\xE770" }
        };

        foreach (var drive in DriveInfo.GetDrives().Where(d => d.Name != @"C:\"))
        {
            items.Add(new() { Name = $"Drive ({drive.Name.Replace("\\", "")})", Path = drive.Name, Icon = "\xE770" });
        }
        sidebarListView.ItemsSource = items;
    }

    private void LoadDirectory(string path, bool saveToHistory = true)
    {
        try
        {
            if (path == "THIS_PC") { DisplayDrives(); return; }
            if (!Directory.Exists(path)) return;

            if (saveToHistory && SelectedPath != path)
                _history.Push(SelectedPath);
            SelectedPath = path;
            backButton.IsEnabled = _history.Count > 0;

            // Update Address Breadcrumb
            var segments = path.Split([Path.DirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries).ToList();
            if (path.StartsWith(@"C:\")) segments.Insert(0, "Local Disk (C:)");
            pathBreadcrumb.ItemsSource = segments;

            selectButton.IsEnabled = false;
            folderGridView.Items.Clear();

            // Folders first, so the user can keep navigating
            var directories = Directory.GetDirectories(path)
                                       .Select(p => Path.GetFileName(p))
                                       .Where(name => !name.StartsWith("$") && !name.StartsWith("."))
                                       .OrderBy(name => name)
                                       .Select(name => new FileSystemEntry
                                       {
                                           Name = name,
                                           FullPath = Path.Combine(path, name),
                                           IsDirectory = true,
                                           IconGlyph = "\xED25",
                                           IconColor = FolderColor
                                       });

            // Then files matching the allowed extensions
            var files = Directory.GetFiles(path)
                                 .Where(f => AllowedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                                 .OrderBy(f => Path.GetFileName(f))
                                 .Select(f => new FileSystemEntry
                                 {
                                     Name = Path.GetFileName(f),
                                     FullPath = f,
                                     IsDirectory = false,
                                     IconGlyph = "\xE7C3",
                                     IconColor = FileColor
                                 });

            foreach (var entry in directories) folderGridView.Items.Add(entry);
            foreach (var entry in files) folderGridView.Items.Add(entry);
        }
        catch (UnauthorizedAccessException)
        {

        }
    }

    private void DisplayDrives()
    {
        SelectedPath = "THIS_PC";
        pathBreadcrumb.ItemsSource = new List<string> { "This PC" };
        selectButton.IsEnabled = false;
        folderGridView.Items.Clear();
        foreach (var drive in DriveInfo.GetDrives())
        {
            folderGridView.Items.Add(new FileSystemEntry
            {
                Name = drive.Name,
                FullPath = drive.Name,
                IsDirectory = true,
                IconGlyph = "\xE770;",
                IconColor = FolderColor
            });
        }
    }

    // -- EVENTS ----------------------------------------------------
    private void FolderGridView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (folderGridView.SelectedItem is not FileSystemEntry entry) return;

        if (entry.IsDirectory)
        {
            LoadDirectory(entry.FullPath);
        }
        else
        {
            // Double-clicking a valid file confirms the pick immediately
            SelectedPath = entry.FullPath;
            Hide();
        }
    }

    private void FolderGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Only enable Select when a *file* (not a folder) is selected
        selectButton.IsEnabled = folderGridView.SelectedItem is FileSystemEntry { IsDirectory: false };
    }

    private void UpButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPath == "THIS_PC") return;
        var parent = Directory.GetParent(SelectedPath);
        LoadDirectory(parent != null ? parent.FullName : "THIS_PC");
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (_history.Count > 0) LoadDirectory(_history.Pop(), saveToHistory: false);
    }

    private void PathBreadcrumb_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (SelectedPath == "THIS_PC") return;

        var segments = SelectedPath.Split([Path.DirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        var target = string.Join(Path.DirectorySeparatorChar, segments.Take(args.Index));
        if (!target.Contains(":") && SelectedPath.StartsWith(@"C:\"))
            target = @"C:\" + target;
        LoadDirectory(target);
    }

    private void SidebarListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sidebarListView.SelectedItem is SidebarItem item)
            LoadDirectory(item.Path);
    }

    private void OnPrimaryButtonClick(object sender, RoutedEventArgs e)
    {
        if (folderGridView.SelectedItem is FileSystemEntry { IsDirectory: false } entry)
        {
            SelectedPath = entry.FullPath;
            Hide();
        }
        // If nothing valid is selected, SelectButton is disabled anyway,
        // so this branch is just a safety net.
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        SelectedPath = null;
        Hide();
    }
}
