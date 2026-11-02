using System.Runtime.InteropServices;
using System.Text;

namespace PackFit.Engineering.Desktop;

/// <summary>
/// Local-only, best-effort startup trace. Does not collect hardware identifiers,
/// network information, telemetry, Windows event logs or imported documents.
/// The exception text may contain local paths; users must review before sharing.
/// </summary>
internal static class StartupDiagnostics
{
    private static readonly object Sync = new();
    private const long MaxBytes = 512_000;

    internal static string LogPath
    {
        get
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(root)) root = Path.GetTempPath();
            return Path.Combine(root, "PackFitOpen", "Logs", "startup.log");
        }
    }

    internal static void Record(string stage, Exception? exception = null)
    {
        try
        {
            lock (Sync)
            {
                string file = LogPath;
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                if (File.Exists(file) && new FileInfo(file).Length > MaxBytes)
                {
                    File.Move(file, file + ".previous", overwrite: true);
                }

                var line = new StringBuilder()
                    .Append(DateTimeOffset.UtcNow.ToString("O"))
                    .Append(" | ").Append(stage)
                    .Append(" | PackFit Engineering ")
                    .Append(typeof(StartupDiagnostics).Assembly.GetName().Version)
                    .AppendLine();
                if (exception is not null)
                    line.AppendLine(exception.ToString());

                File.AppendAllText(file, line.ToString(), Encoding.UTF8);
            }
        }
        catch
        {
            // Logging must not introduce an additional startup failure.
        }
    }

    internal static void Fatal(string stage, Exception error)
    {
        Record(stage, error);
        try
        {
            string detail = error.GetType().Name + ": " + error.Message;
            if (detail.Length > 700) detail = detail[..700] + "…";
            MessageBoxW(IntPtr.Zero,
                "PackFit Engineering 無法完成啟動。\n\n" +
                detail + "\n\n本機診斷紀錄：\n" + LogPath +
                "\n\n請先試完整 ZIP 解壓版。對外分享記錄前，請檢查並移除檔案路徑或其他個人資訊。",
                "PackFit — Startup Error", 0x10);
        }
        catch
        {
            // An early or native runtime failure may prevent showing a window.
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "MessageBoxW")]
    private static extern int MessageBoxW(IntPtr hwnd, string text, string caption, uint type);
}
