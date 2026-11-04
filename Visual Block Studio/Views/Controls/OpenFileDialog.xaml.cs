using Visual_Block_Studio.Models;
using Visual_Block_Studio.ViewModels;

namespace Visual_Block_Studio.Views.Controls;

public sealed partial class OpenFileDialog : ContentDialog
{
    public Recent Recent { get; private set; } = null!;
    public RecentsViewModel ViewModel { get; }

    public OpenFileDialog()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<RecentsViewModel>();

        Opened += OpenFileDialog_Opened;
    }

    private void OpenFileDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        ViewModel.LoadRecents();
    }

    private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is Recent selectedItem)
        {
            Recent = selectedItem;
            ViewModel.UpdateRecent(Recent);
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
            Recent = (Recent)args.ChosenSuggestion;

            ViewModel.UpdateRecent(Recent);
        }
    }
}
