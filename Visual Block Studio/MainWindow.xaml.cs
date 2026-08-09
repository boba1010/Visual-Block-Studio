using System.Diagnostics;
using Visual_Block_Studio.Models.Explorer;
using Visual_Block_Studio.ViewModels.Shell;

namespace Visual_Block_Studio;

public sealed partial class MainWindow : Window
{
    public ShellViewModel ViewModel { get; set; }

    private readonly string ProjectPath = "";

    public MainWindow(string projectPath)
    {
        InitializeComponent();

        if (!string.IsNullOrEmpty(projectPath))
            ProjectPath = projectPath;

        ViewModel = App.Services.GetRequiredService<ShellViewModel>();

        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

        SetTitleBar(titlebar);
        ExtendsContentIntoTitleBar = true;

        if (Content is FrameworkElement element)
        {
            element.Loading += Element_Loading;
        }
    }

    private async void Element_Loading(FrameworkElement sender, object args)
    {
        sender.Loading -= Element_Loading;

        await ViewModel.Solution.LoadSolutionAsync(ProjectPath);
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
