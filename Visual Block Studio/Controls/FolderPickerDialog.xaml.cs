namespace Visual_Block_Studio.Controls;

public sealed partial class FolderPickerDialog : ContentDialog
{
    public string SelectedPath { get; private set; } = @"C:\repos\source";
    private Stack<string> _history = new();

    public FolderPickerDialog()
    {
        InitializeComponent();

        LoadSidebar();
        LoadDirectory(SelectedPath, saveToHistory: false);
    }

    private void LoadSidebar()
    {
        var items = new List<SidebarItem>
            {
                new SidebarItem { Name = "This PC", Path = "THIS_PC", Icon = "\xE9A1;" },
                new SidebarItem { Name = "Local Disk (C:)", Path = @"C:\", Icon = "\xE770;" }
            };

        // Safely list external storage or alternate drives if they exist
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.Name != @"C:\"))
        {
            items.Add(new SidebarItem { Name = $"Drive ({drive.Name.Replace("\\", "")})", Path = drive.Name, Icon = "\xE770;" });
        }
        SidebarListView.ItemsSource = items;
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
            BackButton.IsEnabled = _history.Count > 0;

            // Update Address Breadcrumb
            var segments = path.Split([Path.DirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries).ToList();
            if (path.StartsWith(@"C:\")) segments.Insert(0, "Local Disk (C:)");
            PathBreadcrumb.ItemsSource = segments;

            FolderGridView.Items.Clear();
            var directories = Directory.GetDirectories(path)
                                       .Select(p => Path.GetFileName(p))
                                       .Where(name => !name.StartsWith("$") && !name.StartsWith("."));

            foreach (var dir in directories) FolderGridView.Items.Add(dir);
        }
        catch (UnauthorizedAccessException)
        {

        }
    }

    private void DisplayDrives()
    {
        SelectedPath = "THIS_PC";
        PathBreadcrumb.ItemsSource = new List<string> { "This PC" };
        FolderGridView.Items.Clear();
        foreach (var drive in DriveInfo.GetDrives()) FolderGridView.Items.Add(drive.Name);
    }

    private void FolderGridView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (FolderGridView.SelectedItem is string selection)
        {
            string target = SelectedPath == "THIS_PC" ? selection : Path.Combine(SelectedPath, selection);
            LoadDirectory(target);
        }
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
        if (SidebarListView.SelectedItem is SidebarItem item)
            LoadDirectory(item.Path);
    }

    private void OnPrimaryButtonClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }
    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        SelectedPath = null;
        Hide();
    }
}
