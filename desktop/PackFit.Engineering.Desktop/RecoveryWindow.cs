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
    private IReadOnlyList<CapacityHistoryPoint> _capacityHistory = [];
    private PackDesign? _packDesign;
    private StandbyDrainResult? _standby;
    private readonly StackPanel _visuals = new() {Spacing=15};
    private readonly StackPanel _standbyCanvas = new() {Spacing=10};
    private readonly TextBox _sleepBefore = new() {Header="待機前 SOC (%)", Text="85"};
    private readonly TextBox _sleepAfter = new() {Header="待機後 SOC (%)", Text="77"};
    private readonly TextBox _sleepHours = new() {Header="待機時間（小時）", Text="8"};
    private readonly TextBox _sleepCapacity = new() {Header="滿充容量 (Wh，可留白)", Text=""};

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
        root.Children.Add(Action("一鍵載入合成資料與示意圖", LoadDemoAsync));
        root.Children.Add(Heading("電池容量與量測曲線（僅示意／匯入資料）"));
        root.Children.Add(_visuals);
        root.Children.Add(Heading("電池包快速估算"));
        var fields = new StackPanel { Spacing = 8 };
        fields.Children.Add(_series);
        fields.Children.Add(_parallel);
        fields.Children.Add(_cellVoltage);
        fields.Children.Add(_cellCapacity);
        root.Children.Add(fields);
        root.Children.Add(Action("計算示例 Pack（其他尺寸/重量使用示例值）", CalculatePackAsync));
        root.Children.Add(Heading("待機掉電試算（手動讀值）"));
        root.Children.Add(_sleepBefore);
        root.Children.Add(_sleepAfter);
        root.Children.Add(_sleepHours);
        root.Children.Add(_sleepCapacity);
        root.Children.Add(Action("計算待機 SOC/hr", CalculateStandbyAsync));
        root.Children.Add(_standbyCanvas);
        root.Children.Add(Heading("系統與報告"));
        root.Children.Add(Action("Windows 電池睡眠狀態 (powercfg /a)", CheckSleepAsync));
        root.Children.Add(Action("Windows 電力阻擋因素 (powercfg /requests)", async () =>
            _output.Text = await WindowsDiagnostics.ExecuteAsync("requests")));
        root.Children.Add(Action("產生 Markdown 工程摘要", SaveReportAsync));
        root.Children.Add(new TextBlock
        {
            Text = "限制：相容模式有圖表與核心工具，仍不包含完整介面的批次篩選、全部門檻及工作區。所有示例數值不代表安全規格。",
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
        UpdateVisuals();
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
        string html = await File.ReadAllTextAsync(path);
        _batteryRecords = await Task.Run(() => BatteryReportParser.ParseAll(html));
        _capacityHistory = await Task.Run(() => BatteryHistoryParser.Parse(html));
        _battery = _batteryRecords.Count == 1 ? _batteryRecords[0] : null;
        _output.Text = string.Join("\n\n",
            _batteryRecords.Select((b, index) => string.Format(CultureInfo.InvariantCulture,
                "電池 {0} · 設計 {1:0.##} Wh · 滿充 {2:0.##} Wh · 保留率 {3:0.0}% · 循環次數 {4}",
                index + 1, b.DesignWh, b.FullChargeWh, b.RetentionPercent,
                b.Cycles?.ToString(CultureInfo.InvariantCulture) ?? "未提供")));
        UpdateVisuals();
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
        UpdateVisuals();
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
        UpdateVisuals();
    }

    private Task CalculatePackAsync()
    {
        int series = int.Parse(_series.Text, CultureInfo.InvariantCulture);
        int parallel = int.Parse(_parallel.Text, CultureInfo.InvariantCulture);
        double voltage = double.Parse(_cellVoltage.Text, CultureInfo.InvariantCulture);
        double capacity = double.Parse(_cellCapacity.Text, CultureInfo.InvariantCulture);
        _packDesign = new PackDesign(series, parallel, voltage, capacity, 45, 110, 240, 70, 10, 15);
        _pack = PackCalculator.Calculate(_packDesign);
        _output.Text = string.Format(CultureInfo.InvariantCulture,
            "{0}S{1}P · 電芯 {2} 顆\n額定 {3:0.##} V · {4:0.##} Ah · {5:0.##} Wh\n" +
            "示例總重 {6:0.##} g · {7:0.##} Wh/kg · 理論續航 {8:0.##} h",
            series, parallel, _pack.CellCount, _pack.NominalVoltageV,
            _pack.CapacityAh, _pack.EnergyWh, _pack.TotalMassG,
            _pack.GravimetricWhKg, _pack.IdealRuntimeHours);
        UpdateVisuals();
        return Task.CompletedTask;
    }

    private void UpdateVisuals()
    {
        _visuals.Children.Clear();
        var batteryVisual=new StackPanel {Spacing=10};
        EngineeringVisuals.Battery(batteryVisual,_batteryRecords,_capacityHistory);
        _visuals.Children.Add(batteryVisual);
        var telemetryVisual=new StackPanel {Spacing=10};
        EngineeringVisuals.Telemetry(telemetryVisual,_telemetry);
        _visuals.Children.Add(telemetryVisual);
        if(_packDesign is not null && _pack is not null)
        {
            var packVisual=new StackPanel {Spacing=10};
            EngineeringVisuals.Pack(packVisual,_packDesign,_pack);
            _visuals.Children.Add(packVisual);
        }
        var flow = new StackPanel {Spacing=9};
        EngineeringVisuals.Workflow(flow,
            EngineeringTriage.NextSteps(_telemetry,_battery,_matrix is not null));
        _visuals.Children.Add(Heading("下一步建議（非根因判定）"));
        _visuals.Children.Add(flow);
    }

    private Task CalculateStandbyAsync()
    {
        double n(string input)=>double.Parse(input,CultureInfo.InvariantCulture);
        double? capacity=string.IsNullOrWhiteSpace(_sleepCapacity.Text)
            ? null:n(_sleepCapacity.Text);
        _standby=StandbyDrainEstimator.Estimate(
            n(_sleepBefore.Text),n(_sleepAfter.Text),n(_sleepHours.Text),capacity);
        EngineeringVisuals.Standby(_standbyCanvas,_standby);
        return Task.CompletedTask;
    }

    internal void RunVisualSmokeTest()
    {
        LoadDemoAsync().GetAwaiter().GetResult();
        CalculateStandbyAsync().GetAwaiter().GetResult();
        if(_batteryRecords.Count!=1 || _capacityHistory.Count<2 ||
           _telemetry?.Trace.Count<2 || _matrix is null ||
           _pack is null || _standby is null || _visuals.Children.Count<4 ||
           _standbyCanvas.Children.Count==0)
            throw new InvalidOperationException("Compatibility native charts smoke test failed.");
    }

    private Task LoadDemoAsync()
    {
        const string batteryHtml = "<h2>Installed batteries</h2><table>" +
            "<tr><td>DESIGN CAPACITY</td><td>56,000 mWh</td></tr>" +
            "<tr><td>FULL CHARGE CAPACITY</td><td>49,000 mWh</td></tr></table>" +
            "<h2>Battery capacity history</h2><table>" +
            "<tr><td>2026-08</td><td>52,000 mWh</td><td>56,000 mWh</td></tr>" +
            "<tr><td>2026-09</td><td>51,000 mWh</td><td>56,000 mWh</td></tr>" +
            "<tr><td>2026-10</td><td>49,000 mWh</td><td>56,000 mWh</td></tr></table>";
        const string telemetryCsv = "Timestamp,Voltage_V,Current_A,Temp_C,SOC_pct\n" +
            "2026-11-01T00:00:00Z,11.60,1.20,32,80\n" +
            "2026-11-01T00:00:30Z,11.55,1.25,35,79\n" +
            "2026-11-01T00:01:00Z,11.49,1.40,39,78\n" +
            "2026-11-01T00:01:30Z,11.42,1.80,45,77\n" +
            "2026-11-01T00:02:00Z,11.36,2.00,64,76\n" +
            "2026-11-01T00:02:30Z,11.31,1.85,53,75\n" +
            "2026-11-01T00:03:00Z,11.28,1.62,46,74\n";
        _batteryRecords=BatteryReportParser.ParseAll(batteryHtml);
        _batteryHistory=BatteryHistoryParser.Parse(batteryHtml);
        _battery=_batteryRecords[0];
        _telemetry=TelemetryAnalyzer.Analyze(telemetryCsv,_limits);
        _matrix=ValidationMatrix.Parse("TestID,Category,Metric,Measured,Min,Max,Unit,Owner,Notes\n" +
            "THERM-01,Thermal,Temperature,64,,60,C,EE,Synthetic\n" +
            "VOLT-01,Electrical,Voltage,11.4,10,13,V,EE,Synthetic\n" +
            "SLEEP-01,System,Sleep loss,,0,3,percent,System,Not measured\n");
        _packDesign=new PackDesign(3,2,3.7,3.0,45,110,240,70,10,15);
        _pack=PackCalculator.Calculate(_packDesign);
        UpdateVisuals();
        _output.Text="已載入虛構資料：56/49 Wh、7 筆電性 Log、3 項測試案例及 3S2P Pack。";
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
        if(_standby is not null)
            report += $"\n\n## Manual standby SOC estimate\n{_standby.LossPercentPerHour:0.###} %/h; " +
                $"{_standby.AveragePowerW?.ToString("0.###",CultureInfo.InvariantCulture)??"N/A"} W estimated. " +
                "Not power-meter data.\n";
        await File.WriteAllTextAsync(file.Path, report);
        _output.Text = "工程摘要已儲存（本機）：\n" + file.Path;
    }
}
