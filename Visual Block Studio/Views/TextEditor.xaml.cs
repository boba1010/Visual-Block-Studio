using Visual_Block_Studio.ViewModels;

namespace Visual_Block_Studio.Views;

public sealed partial class TextEditor : Page
{
    public TextEditorViewModel ViewModel { get; set; }

    public TextEditor()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<TextEditorViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string path)
        {
            await ViewModel.LoadTextAsync(path);
        }
    }
}
