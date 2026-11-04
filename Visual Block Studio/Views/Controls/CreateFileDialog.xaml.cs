using Visual_Block_Studio.Models;
using Visual_Block_Studio.ViewModels;

namespace Visual_Block_Studio.Views.Controls;

public sealed partial class CreateFileDialog : ContentDialog
{
    public Recent Recent { get; private set; } = null!;
    public CreateFileViewModel ViewModel { get; }

    public CreateFileDialog()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<CreateFileViewModel>();
    }

    private void FileName_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        ViewModel.FileName = textBox.Text;
    }

    private void OnCreateFileClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        try
        {
            Recent = ViewModel.CreateFile();
        }
        catch (InvalidDataException ex)
        {
            errorTeachingTip.Content = ex.Message;
            errorTeachingTip.IsOpen = true;
        }
    }
}
