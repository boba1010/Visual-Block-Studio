using System.Collections.ObjectModel;
using Visual_Block_Studio.Controls;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Services;
using Visual_Block_Studio.ViewModels.Shell;

namespace Visual_Block_Studio.Views.Dashboard
{
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null && e.Parameter is string path)
            {
                if (!File.Exists(path))
                {
                    errorTeachingTip.Content = "This Project/Solution doesn't exist.";
                    errorTeachingTip.IsOpen = true;
                    return;
                }

                await ViewModel.Solution.LoadSolutionAsync(path);

                App.WorkspaceWindow = new MainWindow();
                App.WorkspaceWindow.Activate();

                App.StartWorkspaceWindow?.Close();
                App.StartWorkspaceWindow = null;
            }
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
                {
                    recent.UpdatedAt = DateTime.Now;
                    RecentsService.Update(recent);
                }

                await ViewModel.Solution.LoadSolutionAsync(filePath);

                RecentsService.Save(new()
                {
                    Name = ViewModel.Solution.SolutionName,
                    UpdatedAt = DateTime.Now,
                    FilePath = ViewModel.Solution.This.FilePath,
                    Type = Enums.RecentType.Solution,
                });

                App.WorkspaceWindow = new MainWindow();
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

        private async void OnOpenFolderClick(object sender, RoutedEventArgs e)
        {
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is Recent selectedItem)
            {
                Recent recent = selectedItem;
                if (!File.Exists(selectedItem.FilePath))
                {
                    errorTeachingTip.Content = "This Project/Solution doesn't exist.";
                    errorTeachingTip.IsOpen = true;
                    return;
                }

                selectedItem.UpdatedAt = DateTime.Now;
                RecentsService.Update(recent);

                await ViewModel.Solution.LoadSolutionAsync(selectedItem.FilePath);

                RecentsService.Save(new()
                {
                    Name = ViewModel.Solution.SolutionName,
                    UpdatedAt = DateTime.Now,
                    FilePath = ViewModel.Solution.This.FilePath,
                    Type = Enums.RecentType.Solution,
                });

                App.WorkspaceWindow = new MainWindow();
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
                if (!File.Exists(selectedItem.FilePath))
                {
                    errorTeachingTip.Content = "This Project/Solution doesn't exist.";
                    errorTeachingTip.IsOpen = true;
                    return;
                }

                selectedItem.UpdatedAt = DateTime.Now;
                RecentsService.Update(selectedItem);

                await ViewModel.Solution.LoadSolutionAsync(selectedItem.FilePath);

                App.WorkspaceWindow = new MainWindow();
                App.WorkspaceWindow.Activate();

                App.StartWorkspaceWindow?.Close();
                App.StartWorkspaceWindow = null;
            }
        }
    }
}
