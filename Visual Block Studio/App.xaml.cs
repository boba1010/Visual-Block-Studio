using Visual_Block_Studio.Services;
using Visual_Block_Studio.ViewModels;
using Visual_Block_Studio.ViewModels.Shell;
using Visual_Block_Studio.Windows;

namespace Visual_Block_Studio;

public partial class App : Application
{
    public static Window? StartWorkspaceWindow { get; set; }
    public static Window? WorkspaceWindow { get; set; }

    public static IServiceProvider Services { get; set; } = null!;

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

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<TabsFactoryViewModel>();
        services.AddTransient<CreateFileViewModel>();
        services.AddSingleton<DashboardViewModel>();

        services.AddSingleton<RecentsService>();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        StartWorkspaceWindow = new StartWorkspaceWindow();
        StartWorkspaceWindow.Activate();
    }
}
