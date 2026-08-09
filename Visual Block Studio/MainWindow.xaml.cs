using Visual_Block_Studio.Models.Explorer;
using Visual_Block_Studio.ViewModels.Shell;

namespace Visual_Block_Studio;

public sealed partial class MainWindow : Window
{
    public ShellViewModel ViewModel { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<ShellViewModel>();

        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

        SetTitleBar(titlebar);
        ExtendsContentIntoTitleBar = true;
    }
    private void EditorTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is TabViewModel tabVm)
        {
            ViewModel.Tabs.CloseTab(tabVm);
        }
    }

    private void TabFrame_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Frame frame ||
            frame.DataContext is not TabViewModel tab)
            return;

        if (frame.Content is null)
            frame.Navigate(tab.TargetPageType, tab.NavigationParameter);
    }

    private async void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is NavigationViewItem navItem && navItem.Tag is ProjectFile selectedFile)
        {
            try
            {
                await ViewModel.Solution.LoadFileAsync(selectedFile);
            }
            catch (Exception ex)
            {
                ErrorDetailsText.Text = $"Details: {ex.Message}";
                ErrorTeachingTip.IsOpen = true;
            }
        }
    }
}
