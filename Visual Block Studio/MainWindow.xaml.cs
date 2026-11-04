using CommunityToolkit.Mvvm.Messaging;
using Visual_Block_Studio.Messages;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.ViewModels.Shell;
using Visual_Block_Studio.Views;
using Visual_Block_Studio.Views.Controls;

namespace Visual_Block_Studio;

public sealed partial class MainWindow : Window
{
    public ShellViewModel ViewModel { get; set; }

    public MainWindow(Recent file)
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<ShellViewModel>();

        ViewModel.Files.Add(new(file.Name, file.FileContent));
        ViewModel.Tabs.OpenNewPageTab(file.Name, typeof(XamlVisualEditor), file);

        SetTitleBar(titlebar);
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

        AppWindow.Closing += AppWindow_Closing;
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        args.Cancel = true;

        var message = new ClosingAllMessage();
        WeakReferenceMessenger.Default.Send(message);

        var statuses = await message.GetResponsesAsync();

        int unsavedCount = statuses.Count(isDirty => isDirty == true);

        if (unsavedCount == 0)
        {
            Close();
            return;
        }

        var dialog = new ContentDialog()
        {
            Title = "Save changes?",
            Content = $"You have {unsavedCount} tab(s) with unsaved changes. Do you want to save before exiting?",
            PrimaryButtonText = "Save All",
            SecondaryButtonText = "Don't Save",
            CloseButtonText = "Cancel",
            XamlRoot = Root.XamlRoot,
        };

        var result = await dialog.ShowAsync();

        if (result is ContentDialogResult.Primary)
        {
            ViewModel.SaveAll();
            Close();
        }
        else if (result is ContentDialogResult.Secondary)
            Close();

        void Close()
        {
            AppWindow.Closing -= AppWindow_Closing;
            App.Current.Exit();
        }
    }

    private async void EditorTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is not TabViewModel tabVm)
            return;

        if (tabVm.Parameter is Recent recent)
        {
            bool isReady = await WeakReferenceMessenger.Default.Send(new ClosingTabMessage(recent.Id));
            if (!isReady)
                return;
        }

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

    private async void AddNewFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateFileDialog()
        {
            XamlRoot = Root.XamlRoot,
        };

        var result = await dialog.ShowAsync();

        if (result is ContentDialogResult.None)
            return;

        var recent = dialog.Recent;
        ViewModel.Tabs.OpenNewPageTab(recent.Name, typeof(XamlVisualEditor), recent);
    }

    private async void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            XamlRoot = Root.XamlRoot,
        };

        var result = await dialog.ShowAsync();

        if (result is ContentDialogResult.None)
            return;

        var recent = dialog.Recent;
        ViewModel.Tabs.OpenNewPageTab(recent.Name, typeof(XamlVisualEditor), recent);
    }
}
