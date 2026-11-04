using Visual_Block_Studio.Models;
using Visual_Block_Studio.ViewModels;
using Visual_Block_Studio.Views.Controls;

namespace Visual_Block_Studio.Views.Dashboard;

public sealed partial class DashboardPage : Page
{
    public RecentsViewModel ViewModel { get; }

    public DashboardPage()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<RecentsViewModel>();

        Loading += DashboardPage_Loading;
    }

    private void DashboardPage_Loading(FrameworkElement sender, object args)
    {
        ViewModel.LoadRecents();   
    }

    private async void OnCreateFileClick(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateFileDialog
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();

        if (result is ContentDialogResult.None)
            return;

        var recent = dialog.Recent;

        OpenWorkspaceWindow(recent);
    }

    private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is Recent selectedItem)
        {
            Recent recent = selectedItem;

            ViewModel.UpdateRecent(recent);

            OpenWorkspaceWindow(recent);

            listView.SelectedItem = null;
        }
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suitableItems = ViewModel.Recents
                .Where(item => item.Name.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();

            sender.ItemsSource = suitableItems;
        }
    }

    private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null)
        {
            var recent = (Recent)args.ChosenSuggestion;

            ViewModel.UpdateRecent(recent);

            OpenWorkspaceWindow(recent);
        }
    }

    private void OpenWorkspaceWindow(Recent recent)
    {
        App.WorkspaceWindow = new MainWindow(recent);
        App.WorkspaceWindow.Activate();

        App.StartWorkspaceWindow?.Close();
        App.StartWorkspaceWindow = null;
    }
}
