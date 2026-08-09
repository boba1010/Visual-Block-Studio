using Visual_Block_Studio.Controls;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.Services;
using Visual_Block_Studio.ViewModels.Shell;

namespace Visual_Block_Studio.Views.Dashboard;

public sealed partial class CreateProjectPage : Page
{
    public SolutionViewModel ViewModel { get; set; }
    private RecentsService RecentsService { get; set; }

    public CreateProjectPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<SolutionViewModel>();
        RecentsService = App.Services.GetRequiredService<RecentsService>();
    }

    private async void OnOpenFilePickerClick(object sender, RoutedEventArgs e)
    {
        var dialog = new FolderPickerDialog() { XamlRoot = XamlRoot };
        await dialog.ShowAsync();

        ViewModel.Request.FilePath = dialog.SelectedPath;
    }

    private async void OnCreateSolutionClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.CreateSolutionAsync();

            RecentsService.Save(new() 
            { 
                Name = ViewModel.SolutionName,
                UpdatedAt = DateTime.Now,
                FilePath = ViewModel.This.FilePath,
                Type = Enums.RecentType.Solution,
            });

            await ViewModel.LoadSolutionAsync(ViewModel.This.FilePath);

            App.WorkspaceWindow = new MainWindow();
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
}
