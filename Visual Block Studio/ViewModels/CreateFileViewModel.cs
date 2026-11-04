using CommunityToolkit.Mvvm.ComponentModel;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Services;

namespace Visual_Block_Studio.ViewModels;

public sealed partial class CreateFileViewModel(RecentsService recents) : ObservableObject
{
    [ObservableProperty]
    public partial string FileName { get; set; } = "NewFile";

    public Recent CreateFile()
    {
        if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(FileName.Trim()))
            throw new InvalidDataException("Please enter a valid file name");

        var recent = new Recent()
        {
            Name = FileName,
            UpdatedAt = DateTime.Now,
            FileContent = "",
        };

        recents.Save(recent);

        return recent;
    }
}
