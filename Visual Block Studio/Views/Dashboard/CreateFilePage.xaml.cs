using Visual_Block_Studio.ViewModels;
using Visual_Block_Studio.Views.Controls;

namespace Visual_Block_Studio.Views.Dashboard;

public sealed partial class CreateFilePage : Page
{
    public CreateFileViewModel ViewModel { get; }

    public CreateFilePage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<CreateFileViewModel>();
    }

    private void OnCreateFileClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var recent = ViewModel.CreateFile();

            App.WorkspaceWindow = new MainWindow(recent);
            App.WorkspaceWindow.Activate();

            App.StartWorkspaceWindow?.Close();
            App.StartWorkspaceWindow = null;
        }
        catch (Exception ex)
        {
            errorTeachingTip.Content = ex switch
            {
                ArgumentException argEx => argEx.Message, // your own validation messages are already user-safe
                UnauthorizedAccessException => "Access denied. VBS can't write to that folder.",
                IOException => "Couldn't create the solution. Check that the target folder is accessible.",
                _ => "Something went wrong while creating the solution."
            };

            errorTeachingTip.IsOpen = true;
        }
    }

    private void FileName_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        ViewModel.FileName = textBox.Text;
    }
}
