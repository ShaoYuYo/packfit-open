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
        // Automated native XAML-load check: unlike a process-liveness test, this
        // fails if MainWindow falls back to RecoveryWindow. No user data is read.
        if (Environment.GetCommandLineArgs().Any(a =>
            a.Equals("--selftest-xaml", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                _mainWindow = new MainWindow();
                StartupDiagnostics.Record("XAML self-test: MAIN_WINDOW_OK");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                StartupDiagnostics.Record("XAML self-test: MAIN_WINDOW_FAILED", ex);
                Environment.Exit(23);
            }
            return;
        }
        try
        {
            _mainWindow = new MainWindow();
            StartupDiagnostics.Record("MainWindow constructed");
            _mainWindow.Activate();
            StartupDiagnostics.Record("MainWindow activated");
        }
        catch (Exception ex)
        {
            StartupDiagnostics.Record("MainWindow failed; attempting recovery window", ex);
            try
            {
                _mainWindow = new RecoveryWindow(ex);
                _mainWindow.Activate();
                StartupDiagnostics.Record("Recovery window activated");
            }
            catch (Exception recovery)
            {
                StartupDiagnostics.Fatal("Main window and recovery window failed", recovery);
                Exit();
            }
        }
    }
}
