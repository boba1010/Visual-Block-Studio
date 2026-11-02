using CommunityToolkit.Mvvm.ComponentModel;
using Visual_Block_Studio.Services;

namespace Visual_Block_Studio.ViewModels;

public sealed partial class CreateFileViewModel(RecentsService recents) : ObservableObject
{
    [ObservableProperty]
    public partial string FileName { get; set; } = "NewFile";

    public void CreateFile()
    {
        recents.Save(new()
        {
            Name = FileName,
            UpdatedAt = DateTime.Now,
            FileContent = "",
        });
    }
}
