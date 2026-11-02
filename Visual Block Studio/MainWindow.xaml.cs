using Visual_Block_Studio.Models;
using Visual_Block_Studio.ViewModels.Shell;
using Visual_Block_Studio.Views;

namespace Visual_Block_Studio;

public sealed partial class MainWindow : Window
{
    public ShellViewModel ViewModel { get; set; }

    public MainWindow(XamlDesignFile file)
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<ShellViewModel>();

        ViewModel.Files.Add(file);
        ViewModel.Tabs.OpenNewPageTab(file.FileName, typeof(XamlVisualEditor), file.FileContent);

        SetTitleBar(titlebar);
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
    }

    private void EditorTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is not TabViewModel tabVm)
            return;

        if (tabVm.ContentFrame is Frame frame)
        {
            frame.BackStack.Clear();
            frame.ForwardStack.Clear();
            frame.Content = null;
            tabVm.ContentFrame = null;
        }

        ViewModel.Tabs.CloseTab(tabVm);
    }

    private void TabFrame_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Frame frame || frame.DataContext is not TabViewModel tab)
            return;

        tab.ContentFrame = frame;

        if (frame.Content is null)
            frame.Navigate(tab.TargetPageType, tab.Parameter);
    }
}
