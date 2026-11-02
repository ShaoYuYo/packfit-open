using Microsoft.UI.Xaml;

namespace PackFit.Engineering.Desktop;

public partial class App : Application
{
    private Window? _mainWindow;

    public App()
    {
        StartupDiagnostics.Record("App constructor");
        try
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                StartupDiagnostics.Record("Unhandled AppDomain exception", args.ExceptionObject as Exception);
            StartupDiagnostics.Record("XAML application resources initialized");
        }
        catch (Exception ex)
        {
            StartupDiagnostics.Fatal("App.InitializeComponent failed", ex);
            throw;
        }
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        StartupDiagnostics.Record("Unhandled WinUI XAML exception", e.Exception);
        // Do not mask an unknown fatal state by setting e.Handled = true.
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        StartupDiagnostics.Record("OnLaunched");
        try
        {
            _mainWindow = new MainWindow();
            StartupDiagnostics.Record("MainWindow constructed");
            _mainWindow.Activate();
            StartupDiagnostics.Record("MainWindow activated");
        }
        catch (Exception ex)
        {
            StartupDiagnostics.Fatal("MainWindow initialization or activation failed", ex);
            Exit();
        }
    }
}
