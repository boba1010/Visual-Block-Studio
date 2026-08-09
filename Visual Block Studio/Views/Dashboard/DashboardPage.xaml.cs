using Microsoft.UI.Xaml.Shapes;
using System.Collections.ObjectModel;
using Visual_Block_Studio.Controls;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Services;

namespace Visual_Block_Studio.Views.Dashboard
{
    public sealed partial class DashboardPage : Page
    {
        public ObservableCollection<Recent> Recents { get; private set; } = [];
        public RecentsService RecentsService { get; set; }

        public DashboardPage()
        {
            InitializeComponent();

            RecentsService = App.Services.GetRequiredService<RecentsService>();

            Loading += DashboardPage_Loading;
        }

        private void DashboardPage_Loading(FrameworkElement sender, object args)
        {
            foreach (var item in RecentsService.Get())
                Recents.Add(item);
        }

        private void OnCreateProjectClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateProjectPage));
        }

        private bool _isDialogOpen;
        private async void OnOpenProjectOrSolutionClick(object sender, RoutedEventArgs e)
        {
            if (_isDialogOpen)
                return;

            _isDialogOpen = true;

            try
            {
                var dialog = new FilePickerDialog { XamlRoot = XamlRoot };

                await dialog.ShowAsync();

                if (string.IsNullOrEmpty(dialog.SelectedPath))
                    return;

                string filePath = dialog.SelectedPath;

                if (!File.Exists(filePath))
                {
                    errorTeachingTip.Content = "This Project/Solution doesn't exist.";
                    errorTeachingTip.IsOpen = true;
                    return;
                }

                Recent? recent = Recents.FirstOrDefault(r => r.FilePath == filePath);
                if (recent != null)
                    RecentsService.Update(recent);

                App.WorkspaceWindow = new MainWindow(filePath);
                App.WorkspaceWindow.Activate();

                App.StartWorkspaceWindow?.Close();
                App.StartWorkspaceWindow = null;
            }
            catch (Exception ex)
            {
                errorTeachingTip.Content = ex switch
                {
                    ArgumentException argEx => argEx.Message,
                    UnauthorizedAccessException => "Access denied. VBS can't write to that folder.",
                    IOException => "Couldn't create the solution. Check that the target folder is accessible.",
                    _ => "Something went wrong while creating the solution."
                };

                errorTeachingTip.IsOpen = true;
            }
            finally
            {
                _isDialogOpen = false;
            }
        }

        private void OnOpenFolderClick(object sender, RoutedEventArgs e)
        {
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is Recent selectedItem)
            {
                Recent recent = selectedItem;
                RecentsService.Update(recent);

                App.WorkspaceWindow = new MainWindow(selectedItem.FilePath);
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

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                var selectedItem = (Recent)args.ChosenSuggestion;
                RecentsService.Update(selectedItem);

                App.WorkspaceWindow = new MainWindow(selectedItem.FilePath);
                App.WorkspaceWindow.Activate();

                App.StartWorkspaceWindow?.Close();
                App.StartWorkspaceWindow = null;
            }
        }
    }
}
