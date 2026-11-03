using ABI.Windows.Foundation;
using System.Collections.ObjectModel;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Services;
using Visual_Block_Studio.ViewModels.Shell;

namespace Visual_Block_Studio.Views.Dashboard;

public sealed partial class DashboardPage : Page
{
    public ObservableCollection<Recent> Recents { get; private set; } = [];
    public RecentsService RecentsService { get; set; }
    public ShellViewModel ViewModel { get; set; }

    public DashboardPage()
    {
        InitializeComponent();

        RecentsService = App.Services.GetRequiredService<RecentsService>();
        ViewModel = App.Services.GetRequiredService<ShellViewModel>();

        Loading += DashboardPage_Loading;
    }

    private void DashboardPage_Loading(FrameworkElement sender, object args)
    {
        foreach (var item in RecentsService.Get())
            Recents.Add(item);
    }

    private void OnCreateFileClick(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(CreateFilePage));
    }

    private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is Recent selectedItem)
        {
            Recent recent = selectedItem;

            selectedItem.UpdatedAt = DateTime.Now;
            RecentsService.Update(recent);

            App.WorkspaceWindow = new MainWindow(recent);
            App.WorkspaceWindow.Activate();

            listView.SelectedItem = null;

            App.StartWorkspaceWindow?.Close();
            App.StartWorkspaceWindow = null;
        }
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suitableItems = Recents
                .Where(item => item.Name.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();

            sender.ItemsSource = suitableItems;
        }
    }

    private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null)
        {
            var selectedItem = (Recent)args.ChosenSuggestion;

            selectedItem.UpdatedAt = DateTime.Now;
            RecentsService.Update(selectedItem);

            App.WorkspaceWindow = new MainWindow(selectedItem);
            App.WorkspaceWindow.Activate();

            App.StartWorkspaceWindow?.Close();
            App.StartWorkspaceWindow = null;
        }
    }
}
