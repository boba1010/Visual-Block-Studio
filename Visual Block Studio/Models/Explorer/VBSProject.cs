using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Visual_Block_Studio.Models.Explorer;

public partial class VBSProject : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = null!;

    [ObservableProperty]
    public partial string TargetArchitecture { get; set; } = null!;

    [ObservableProperty]
    public partial uint MemoryLimitBytes { get; set; }

    [ObservableProperty]
    public partial string OutputPath { get; set; } = null!;

    // For example 'Visual Block Studio.vbsproj'
    [ObservableProperty]
    public partial string FileName { get; set; } = null!;

    [ObservableProperty]
    public partial string FilePath { get; set; } = null!;

    public string FileIconGlyph => "C++";

    // For example 'Visual Block Studio.vbsproj.g.cs'
    [ObservableProperty]
    public partial string GeneratedFileName { get; set; } = null!;

    [ObservableProperty]
    public partial string GeneratedFilePath { get; set; } = null!;

    [ObservableProperty]
    public partial string GeneratedFileIconGlyph { get; set; } = null!;

    [ObservableProperty]
    public partial string VcxProjFileName { get; set; } = null!;

    [ObservableProperty]
    public partial string VcxProjFilePath { get; set; } = null!;

    [ObservableProperty]
    public partial string VcxProjFileIconGlyph { get; set; } = null!;

    public ObservableCollection<ProjectItem> LayoutFiles { get; init; } = [];
}
