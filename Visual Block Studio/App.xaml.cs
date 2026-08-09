using Visual_Block_Studio.Services;
using Visual_Block_Studio.ViewModels;
using Visual_Block_Studio.ViewModels.Shell;
using Visual_Block_Studio.Windows;

namespace Visual_Block_Studio
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window? StartWorkspaceWindow { get; set; }
        public static Window? WorkspaceWindow { get; set; }

        public static IServiceProvider Services { get; set; } = null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            ServiceCollection services = new();

            ConfigureServices(services);

            Services = services.BuildServiceProvider();

            UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            //e.Handled = true;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // -- Main Window viewmodels --------------------
            services.AddSingleton<ShellViewModel>();

            services.AddSingleton<ToolBoxViewModel>();
            services.AddSingleton<ToolBoxExplorerViewModel>();
            
            services.AddSingleton<TabsFactoryViewModel>();

            services.AddSingleton<SolutionViewModel>();
            services.AddSingleton<SolutionManager>();
            services.AddSingleton<SolutionExplorerViewModel>();
            
            // -- Blocks ------------------------------------
            services.AddTransient<BlocksViewModel>();
            services.AddSingleton<XamlCodeBlockService>();
            services.AddSingleton<CodeBlockService>();

            // -- Text Editor -------------------------------
            services.AddTransient<TextEditorViewModel>();

            // -- Services -----------------------------------
            services.AddSingleton<VBSProjectManager>();
            services.AddSingleton<TextEditorService>();
            services.AddSingleton<TemplateProjectService>();
            services.AddSingleton<RecentsService>();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();

            // Check if a file path was passed to the app via double-click
            if (commandLineArgs.Length > 1 && File.Exists(commandLineArgs[1]))
            {
                string targetFile = commandLineArgs[1];

                // Bypass the start window and open the main workspace window directly
                WorkspaceWindow = new MainWindow(targetFile);
                WorkspaceWindow.Activate();
            }
            else
            {
                // Open normal startup dashboard window (Visual Studio style)
                StartWorkspaceWindow = new StartWorkspaceWindow();
                StartWorkspaceWindow.Activate();
            }
        }
    }
}
