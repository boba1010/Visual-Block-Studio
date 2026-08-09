using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using Visual_Block_Studio.Services;

namespace Visual_Block_Studio.ViewModels;

public partial class TextEditorViewModel(TextEditorService textEditorService) : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = null!;

    public async Task LoadTextAsync(string path)
    {
        Text = await Task.Run(() => textEditorService.LoadText(path));
    }
}
