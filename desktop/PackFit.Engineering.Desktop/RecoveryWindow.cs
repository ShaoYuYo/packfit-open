using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PackFit.Engineering.Core;
using Windows.Storage.Pickers;

namespace PackFit.Engineering.Desktop;

/// <summary>
/// Native compatibility workbench built entirely in C#. It deliberately never
/// loads MainWindow.xaml, so a machine-specific XAML resource failure cannot
/// block every engineering feature or the --safe-ui troubleshooting path.
/// </summary>
internal sealed class RecoveryWindow : Window
{
    private readonly TextBlock _output = new()
    {
        Text = "請選擇上方的工具；所有檔案都只在本機分析。",
        TextWrapping = TextWrapping.Wrap,
        IsTextSelectionEnabled = true,
        FontSize = 14
    };
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap, FontSize = 13 };
    private readonly TextBox _series = new() { Header = "串聯 S", Text = "3", MinWidth = 80 };
    private readonly TextBox _parallel = new() { Header = "並聯 P", Text = "2", MinWidth = 80 };
    private readonly TextBox _cellVoltage = new() { Header = "單顆電壓 V", Text = "3.7", MinWidth = 100 };
    private readonly TextBox _cellCapacity = new() { Header = "單顆容量 Ah", Text = "3.0", MinWidth = 100 };
    private BatteryHealth? _battery;
    private IReadOnlyList<BatteryHealth> _batteryRecords = [];
    private TelemetryAnalysis? _telemetry;
    private IReadOnlyList<ValidationItem>? _matrix;
    private PackResult? _pack;
    private readonly TelemetryThresholds _limits = new();

    internal RecoveryWindow(Exception? failure)
    {
        Title = "PackFit Engineering — Compatible Workspace";
        var root = new StackPanel { Padding = new Thickness(28), Spacing = 15 };
        root.Children.Add(new TextBlock
        {
            Text = "PackFit Engineering",
            FontSize = 30,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });
        root.Children.Add(new TextBlock
        {
            Text = "相容工程工作台 · Native WinUI · Local only",
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap
        });
        root.Children.Add(new TextBlock
        {
            Text = failure is null
                ? "安全模式：不載入主要 XAML、自訂標題列或動畫。"
                : "主視窗 XAML 在這台電腦上載入失敗，已自動切換到可操作的相容模式。",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 15
        });
        if (failure is not null)
        {
            root.Children.Add(new TextBlock
            {
                Text = "錯誤：" + failure.GetType().Name + " · HRESULT 0x" +
                    failure.HResult.ToString("X8") + "\n診斷紀錄：" + StartupDiagnostics.LogPath,
                TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true,
                FontSize = 12
            });
        }

        root.Children.Add(Heading("資料分析與工作流程"));
        root.Children.Add(Action("匯入 Windows 電池 HTML 報告", ImportBatteryAsync));
        root.Children.Add(Action("匯入量測 CSV / TSV", ImportTelemetryAsync));
        root.Children.Add(Action("匯入驗證矩陣 CSV", ImportValidationAsync));
        root.Children.Add(Heading("電池包快速估算"));
        var fields = new StackPanel { Spacing = 8 };
        fields.Children.Add(_series);
        fields.Children.Add(_parallel);
        fields.Children.Add(_cellVoltage);
        fields.Children.Add(_cellCapacity);
        root.Children.Add(fields);
        root.Children.Add(Action("計算示例 Pack（其他尺寸/重量使用示例值）", CalculatePackAsync));
        root.Children.Add(Heading("系統與報告"));
        root.Children.Add(Action("Windows 電池睡眠狀態 (powercfg /a)", CheckSleepAsync));
        root.Children.Add(Action("產生 Markdown 工程摘要", SaveReportAsync));
        root.Children.Add(new TextBlock
        {
            Text = "限制：相容模式提供核心分析，進階門檻、批次圖表及全部工作區仍在完整介面內。所有預設限制與尺寸均為示例，不代表安全規格。",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12
        });
        root.Children.Add(_status);
        root.Children.Add(new Border { Padding = new Thickness(18), CornerRadius = new CornerRadius(12), Child = _output });
        Content = new ScrollViewer
        {
            Content = root,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        StartupDiagnostics.Record(failure is null ? "Safe compatibility workspace created" : "Recovery compatibility workspace created");
    }

    private static TextBlock Heading(string name) => new()
    {
        Text = name, FontSize = 19, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
        Margin = new Thickness(0, 14, 0, 0)
    };

    private Button Action(string name, Func<Task> operation)
    {
        var button = new Button
        {
            Content = name,
            MinHeight = 40,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left
        };
        button.Click += async (_, _) =>
        {
            button.IsEnabled = false;
            _status.Text = "正在處理：" + name;
            try
            {
                await operation();
                _status.Text = "完成：" + name;
            }
            catch (Exception ex)
            {
                _status.Text = "無法完成：" + ex.Message;
                StartupDiagnostics.Record("Compatibility action failed: " + name, ex);
            }
            finally { button.IsEnabled = true; }
        };
        return button;
    }

    private async Task<string?> OpenFileAsync(params string[] extensions)
    {
        var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        foreach (var extension in extensions) picker.FileTypeFilter.Add(extension);
        WinRT.Interop.InitializeWithWindow.Initialize(picker,
            WinRT.Interop.WindowNative.GetWindowHandle(this));
        return (await picker.PickSingleFileAsync())?.Path;
    }

    private async Task ImportBatteryAsync()
    {
        var path = await OpenFileAsync(".html", ".htm");
        if (path is null) return;
        if (new FileInfo(path).Length > 5_000_000)
            throw new FormatException("電池報告超過 5 MB。");
        _batteryRecords = await Task.Run(() => BatteryReportParser.ParseAll(File.ReadAllText(path)));
        _battery = _batteryRecords.Count == 1 ? _batteryRecords[0] : null;
        _output.Text = string.Join("\n\n",
            _batteryRecords.Select((b, index) => string.Format(CultureInfo.InvariantCulture,
                "電池 {0} · 設計 {1:0.##} Wh · 滿充 {2:0.##} Wh · 保留率 {3:0.0}% · 循環次數 {4}",
                index + 1, b.DesignWh, b.FullChargeWh, b.RetentionPercent,
                b.Cycles?.ToString(CultureInfo.InvariantCulture) ?? "未提供")));
    }

    private async Task ImportTelemetryAsync()
    {
        var path = await OpenFileAsync(".csv", ".tsv", ".txt");
        if (path is null) return;
        if (new FileInfo(path).Length > 12_000_000)
            throw new FormatException("檔案超過 12 MB。");
        _telemetry = await Task.Run(() =>
            TelemetryAnalyzer.Analyze(File.ReadAllText(path), _limits));
        _output.Text = "量測筆數：" + _telemetry.Samples.ToString("N0", CultureInfo.InvariantCulture) +
            "\n異常旗標：" + _telemetry.TotalFindings +
            "\n最高溫：" + _telemetry.MaxTemperatureC?.ToString("0.##", CultureInfo.InvariantCulture) +
            " °C\n電壓最小值：" + _telemetry.MinVoltageV?.ToString("0.###", CultureInfo.InvariantCulture) +
            " V\n示例門檻：溫度 -20 至 60 °C；所有結果須對照專案核准規格。";
    }

    private async Task ImportValidationAsync()
    {
        var path = await OpenFileAsync(".csv");
        if (path is null) return;
        if (new FileInfo(path).Length > 4_000_000)
            throw new FormatException("檔案超過 4 MB。");
        _matrix = await Task.Run(() => ValidationMatrix.Parse(File.ReadAllText(path)));
        var sum = ValidationMatrix.Summarize(_matrix);
        _output.Text = "驗證案例 " + sum.Total +
            "\nPASS " + sum.Passed + " · FAIL " + sum.Failed +
            " · NOT RUN " + sum.NotRun;
    }

    private Task CalculatePackAsync()
    {
        int series = int.Parse(_series.Text, CultureInfo.InvariantCulture);
        int parallel = int.Parse(_parallel.Text, CultureInfo.InvariantCulture);
        double voltage = double.Parse(_cellVoltage.Text, CultureInfo.InvariantCulture);
        double capacity = double.Parse(_cellCapacity.Text, CultureInfo.InvariantCulture);
        _pack = PackCalculator.Calculate(new PackDesign(
            series, parallel, voltage, capacity, 45, 110, 240, 70, 10, 15));
        _output.Text = string.Format(CultureInfo.InvariantCulture,
            "{0}S{1}P · 電芯 {2} 顆\n額定 {3:0.##} V · {4:0.##} Ah · {5:0.##} Wh\n" +
            "示例總重 {6:0.##} g · {7:0.##} Wh/kg · 理論續航 {8:0.##} h",
            series, parallel, _pack.CellCount, _pack.NominalVoltageV,
            _pack.CapacityAh, _pack.EnergyWh, _pack.TotalMassG,
            _pack.GravimetricWhKg, _pack.IdealRuntimeHours);
        return Task.CompletedTask;
    }

    private async Task CheckSleepAsync()
    {
        _output.Text = await WindowsDiagnostics.ExecuteAsync("sleep");
    }

    private async Task SaveReportAsync()
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = "PackFit-Compatibility-Report"
        };
        picker.FileTypeChoices.Add("Markdown", new List<string> { ".md" });
        WinRT.Interop.InitializeWithWindow.Initialize(picker,
            WinRT.Interop.WindowNative.GetWindowHandle(this));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        string report = EngineeringReport.Build("Compatible WinUI mode", "",
            _battery, _telemetry, _matrix, _pack, _limits);
        if (_batteryRecords.Count > 1)
            report += "\n## Separate installed batteries\n" +
                string.Join("\n", _batteryRecords.Select((b, i) =>
                    $"- Battery {i + 1}: {b.DesignWh:0.###} Wh design, {b.FullChargeWh:0.###} Wh full"));
        await File.WriteAllTextAsync(file.Path, report);
        _output.Text = "工程摘要已儲存（本機）：\n" + file.Path;
    }
}
