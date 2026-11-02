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
        StartupDiagnostics.RecordEnvironment();
        var commandLine = Environment.GetCommandLineArgs();
        if (commandLine.Any(a => a.Equals("--selftest-visuals", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var window = new MainWindow();
                window.RunVisualSmokeTest();
                _mainWindow = window;
                StartupDiagnostics.Record("Engineering visuals self-test: VISUALS_OK");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                StartupDiagnostics.Record("Engineering visuals self-test: VISUALS_FAILED",ex);
                Environment.Exit(25);
            }
            return;
        }
        if (commandLine.Any(a => a.Equals("--selftest-compat-visuals", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var window = new RecoveryWindow(null);
                window.RunVisualSmokeTest();
                _mainWindow = window;
                StartupDiagnostics.Record("Compatibility visuals self-test: COMPAT_VISUALS_OK");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                StartupDiagnostics.Record("Compatibility visuals self-test: COMPAT_VISUALS_FAILED",ex);
                Environment.Exit(26);
            }
            return;
        }
        if (commandLine.Any(a => a.Equals("--selftest-compat", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                _mainWindow = new RecoveryWindow(null);
                StartupDiagnostics.Record("Compatibility self-test: COMPAT_WINDOW_OK");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                StartupDiagnostics.Record("Compatibility self-test: COMPAT_WINDOW_FAILED", ex);
                Environment.Exit(24);
            }
            return;
        }
        if (commandLine.Any(a => a.Equals("--safe-ui", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                _mainWindow = new RecoveryWindow(null);
                _mainWindow.Activate();
                StartupDiagnostics.Record("Safe compatibility workspace activated");
            }
            catch (Exception ex)
            {
                StartupDiagnostics.Fatal("Safe compatibility workspace failed", ex);
                Exit();
            }
            return;
        }
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
