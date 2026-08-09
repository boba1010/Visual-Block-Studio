using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Visual_Block_Studio.Models.Explorer;

public partial class Solution : ObservableObject
{
    [ObservableProperty]
    public partial string FileName { get; set; } = null!;

    [ObservableProperty]
    public partial string FilePath { get; set; } = null!;

    [ObservableProperty]
    public partial string SlnxFileName { get; set; } = null!;

    [ObservableProperty]
    public partial string SlnxFilePath { get; set; } = null!;

    public string IconGlyph => "\uE737";
    public ObservableCollection<VBSProject> Projects { get; set; } = [];
}
