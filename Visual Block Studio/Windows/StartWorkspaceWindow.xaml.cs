using Microsoft.UI.Windowing;
using Visual_Block_Studio.Views.Dashboard;
using Windows.Graphics;

namespace Visual_Block_Studio.Windows
{
    public sealed partial class StartWorkspaceWindow : Window
    {
        public StartWorkspaceWindow(string? targetPath = null)
        {
            InitializeComponent();

            var windowSize = new SizeInt32(1200, 800);
            AppWindow.Resize(windowSize);

            CenterWindowOnScreen(windowSize);

            ExtendsContentIntoTitleBar = true;

            SetTitleBar(titlebar);

            rootFrame.Navigate(typeof(DashboardPage), targetPath);

            rootFrame.Navigated += RootFrame_Navigated;
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            titlebar.IsBackButtonVisible = rootFrame.CanGoBack;
        }

        private void CenterWindowOnScreen(SizeInt32 windowSize)
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);

            if (displayArea != null)
            {
                int centerX = displayArea.WorkArea.X + (displayArea.WorkArea.Width - windowSize.Width) / 2;
                int centerY = displayArea.WorkArea.Y + (displayArea.WorkArea.Height - windowSize.Height) / 2;
                
                AppWindow.Move(new PointInt32(centerX, centerY));
            }
        }

        private void OnBackRequested(TitleBar sender, object args)
        {
            if (rootFrame.CanGoBack)
                rootFrame.GoBack();
        }
    }
}
