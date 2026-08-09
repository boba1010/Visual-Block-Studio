using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Visual_Block_Studio.Enums;

namespace Visual_Block_Studio.Models.Explorer;

public partial class ProjectItem : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = null!;

    [ObservableProperty]
    public partial ProjectItemType Type { get; set; }

    public ObservableCollection<ProjectFile> Files { get; set; } = [];
}
