using CommunityToolkit.Mvvm.ComponentModel;

namespace Visual_Block_Studio.Contracts;

public partial class CreateSolutionRequest : ObservableObject
{
    [ObservableProperty]
    public partial string FileName { get; set; } = null!;
    
    [ObservableProperty]
    public partial string FilePath { get; set; } = null!;

    [ObservableProperty]
    public partial string? ProjectName { get; set; }
    
    [ObservableProperty]
    public partial bool PlaceProjInSlnx { get; set; } = false;
}
