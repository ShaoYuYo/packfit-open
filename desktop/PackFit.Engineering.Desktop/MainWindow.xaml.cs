using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PackFit.Engineering.Core;
using Windows.Storage.Pickers;

namespace PackFit.Engineering.Desktop;

public sealed partial class MainWindow : Window
{
    private BatteryHealth? _battery;
    private IReadOnlyList<BatteryHealth> _batteryRecords = [];
    private TelemetryAnalysis? _telemetry;
    private TelemetryThresholds? _limits;
    private IReadOnlyList<ValidationItem>? _matrix;
    private PackResult? _pack;
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    private static readonly bool SafeUiMode = Environment.GetCommandLineArgs()
        .Any(x => x.Equals("--safe-ui", StringComparison.OrdinalIgnoreCase));

    public MainWindow()
    {
        InitializeComponent();
        // A native title bar is preferred, but window startup must remain
        // possible on devices where custom nonclient UI initialization fails.
        try
        {
            if (SafeUiMode)
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
                StartupDiagnostics.Record("Safe UI mode: custom title bar disabled");
            }
            else
            {
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(AppTitleBar);
            }
        }
        catch (Exception ex)
        {
            StartupDiagnostics.Record("Custom Fluent title bar unavailable; using system title bar", ex);
            AppTitleBar.Visibility = Visibility.Collapsed;
            ExtendsContentIntoTitleBar = false;
        }

        SetupGuides();
        InitializeEngineeringInsights();
        Navigation.SelectedItem = Navigation.MenuItems[0];
        ShowPage("overview");
    }

    private void Navigation_SelectionChanged(
        NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item)
            ShowPage(item.Tag?.ToString() ?? "overview");
    }

    private string? _activePage;

    private void WorkspaceRoot_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        // The content width excludes the NavigationView sidebar.
        bool compact = args.NewSize.Width < 760;
        QuickActions.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        SymptomActions.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        BatteryActions.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        TelemetryFileActions.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        ValidationFileActions.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        SleepToolActions.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        TelemetryLimitsPrimary.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        TelemetryLimitsSecondary.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        PackParametersPrimary.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        PackParametersSecondary.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;
        PackParametersDimensions.Orientation = compact ? Orientation.Vertical : Orientation.Horizontal;

        for (int i = 0; i < OverviewStatsGrid.ColumnDefinitions.Count; i++)
        {
            OverviewStatsGrid.ColumnDefinitions[i].Width = compact
                ? (i == 0 ? new GridLength(1, GridUnitType.Star) : new GridLength(0))
                : new GridLength(1, GridUnitType.Star);
        }

        var cards = new Microsoft.UI.Xaml.Controls.Border[]
        {
            OverviewStats1, OverviewStats2, OverviewStats3
        };
        for (int i = 0; i < cards.Length; i++)
        {
            Grid.SetColumn(cards[i], compact ? 0 : i);
            Grid.SetRow(cards[i], compact ? i : 0);
        }
    }

    private void AnimatePage(Microsoft.UI.Xaml.Controls.StackPanel page)
    {
        try
        {
            if (SafeUiMode) return;
            // Obey Windows "Show animations in Windows" accessibility setting.
            if (!new Windows.UI.ViewManagement.UISettings().AnimationsEnabled)
                return;

            page.Opacity = 1;
            var fade = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = 0.30,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(190)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase
                {
                    EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut
                }
            };
            var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard
            {
                FillBehavior = Microsoft.UI.Xaml.Media.Animation.FillBehavior.Stop
            };
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(fade, page);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(fade, "Opacity");
            storyboard.Children.Add(fade);
            storyboard.Begin();
        }
        catch (Exception ex)
        {
            // Motion must never prevent an engineering screen from opening.
            page.Opacity = 1;
            StartupDiagnostics.Record("Optional page transition disabled", ex);
        }
    }

    private void ShowPage(string name)
    {
        OverviewPanel.Visibility = name == "overview" ? Visibility.Visible : Visibility.Collapsed;
        BatteryPanel.Visibility = name == "battery" ? Visibility.Visible : Visibility.Collapsed;
        VisualsPanel.Visibility = name == "visuals" ? Visibility.Visible : Visibility.Collapsed;
        SleepPanel.Visibility = name == "sleep" ? Visibility.Visible : Visibility.Collapsed;
        TelemetryPanel.Visibility = name == "telemetry" ? Visibility.Visible : Visibility.Collapsed;
        ValidationPanel.Visibility = name == "validation" ? Visibility.Visible : Visibility.Collapsed;
        PackPanel.Visibility = name == "pack" ? Visibility.Visible : Visibility.Collapsed;
        ReportPanel.Visibility = name == "report" ? Visibility.Visible : Visibility.Collapsed;
        GuidePanel.Visibility = name == "guide" ? Visibility.Visible : Visibility.Collapsed;
        DiagnosticsPanel.Visibility = name == "diagnostics" ? Visibility.Visible : Visibility.Collapsed;
        ResourcesPanel.Visibility = name == "resources" ? Visibility.Visible : Visibility.Collapsed;
        if (_activePage == name) return;
        _activePage = name;
        var page = name switch
        {
            "overview" => OverviewPanel,
            "battery" => BatteryPanel,
            "visuals" => VisualsPanel,
            "sleep" => SleepPanel,
            "telemetry" => TelemetryPanel,
            "validation" => ValidationPanel,
            "pack" => PackPanel,
            "report" => ReportPanel,
            "guide" => GuidePanel,
            "diagnostics" => DiagnosticsPanel,
            "resources" => ResourcesPanel,
            _ => OverviewPanel
        };
        AnimatePage(page);
    }

    private void Notice(string message, bool error = false)
    {
        NoticeBar.Severity = error ? InfoBarSeverity.Error : InfoBarSeverity.Success;
        NoticeBar.Title = error ? "需要確認" : "已完成";
        NoticeBar.Message = message;
        NoticeBar.IsOpen = true;
    }

    private static double ReadNumber(TextBox box)
    {
        if (!double.TryParse(box.Text.Trim(), NumberStyles.Float, Culture, out double value)
            || !double.IsFinite(value))
            throw new FormatException($"「{box.Header}」需要有效數字。");
        return value;
    }

    private static int ReadInteger(TextBox box)
    {
        double n = ReadNumber(box);
        if (n != Math.Truncate(n) || n < 1 || n > 32)
            throw new FormatException($"「{box.Header}」需要 1–32 的整數。");
        return checked((int)n);
    }

    private static double? ReadOptional(TextBox box)
        => string.IsNullOrWhiteSpace(box.Text) ? null : ReadNumber(box);

    private async Task<string?> PickFileAsync(params string[] extensions)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        foreach (var extension in extensions) picker.FileTypeFilter.Add(extension);
        WinRT.Interop.InitializeWithWindow.Initialize(picker,
            WinRT.Interop.WindowNative.GetWindowHandle(this));
        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }

    private async void GenerateBatteryClick(object sender, RoutedEventArgs e)
    {
        string reportPath = Path.Combine(Path.GetTempPath(),
            $"packfit-battery-{Guid.NewGuid():N}.html");
        try
        {
            var info = new ProcessStartInfo("powercfg.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            info.ArgumentList.Add("/batteryreport");
            info.ArgumentList.Add("/output");
            info.ArgumentList.Add(reportPath);
            using var process = Process.Start(info) ??
                throw new InvalidOperationException("無法啟動 Windows powercfg。");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                throw new TimeoutException("Windows 電池報告超過執行時間限制。");
            }
            if (process.ExitCode != 0)
                throw new InvalidOperationException(
                    $"powercfg 失敗（結束碼：{process.ExitCode}）。部分桌機沒有可用電池。");
            string html = await LocalDataFiles.ReadTextAsync(reportPath,5_000_000);
            _batteryRecords = await Task.Run(() => BatteryReportParser.ParseAll(html));
            _batteryHistory = await Task.Run(() => BatteryHistoryParser.Parse(html));
            _battery = _batteryRecords.Count == 1 ? _batteryRecords[0] : null;
            UpdateBattery();
            Notice("Windows 電池報告已在本機建立並分析。");
        }
        catch (Exception ex)
        {
            Notice(ex.Message, true);
        }
        finally
        {
            try { if (File.Exists(reportPath)) File.Delete(reportPath); }
            catch { /* A temporary report file may be locked by security software. */ }
        }
    }

    private async void ImportBatteryClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = await PickFileAsync(".html", ".htm");
            if (path is null) return;
            string html = await LocalDataFiles.ReadTextAsync(path,5_000_000);
            (_batteryRecords, _batteryHistory) = await Task.Run(() =>
                ((IReadOnlyList<BatteryHealth>)BatteryReportParser.ParseAll(html),
                 (IReadOnlyList<CapacityHistoryPoint>)BatteryHistoryParser.Parse(html)));
            _battery = _batteryRecords.Count == 1 ? _batteryRecords[0] : null;
            UpdateBattery();
            Notice("已匯入並分析電池容量報告。");
        }
        catch (Exception ex) { Notice(ex.Message, true); }
    }

    private void UpdateBattery()
    {
        if (_batteryRecords.Count == 0) return;
        var lines = new List<string>();
        for (int i = 0; i < _batteryRecords.Count; i++)
        {
            var b = _batteryRecords[i];
            lines.Add(string.Format(Culture,
                "電池 {0}\n設計容量 {1:0.##} Wh · 滿充容量 {2:0.##} Wh\n容量保留率 {3:0.0}% · 循環次數 {4}",
                i + 1, b.DesignWh, b.FullChargeWh, b.RetentionPercent,
                b.Cycles?.ToString(Culture) ?? "未提供"));
        }
        BatteryResult.Text = string.Join("\n\n", lines) +
            "\n\n容量保留率僅為參考，個別電池不會自動合併。";
        OverviewBattery.Text = _batteryRecords.Count == 1
            ? _batteryRecords[0].RetentionPercent.ToString("0.0", Culture) + "%"
            : $"{_batteryRecords.Count} batteries";
        RefreshVisuals();
    }

    private TelemetryThresholds ReadThresholds() => new(
        MinimumTemperatureC: ReadNumber(MinTempBox),
        MaximumTemperatureC: ReadNumber(MaxTempBox),
        MinimumVoltageV: ReadOptional(MinVoltBox),
        MaximumVoltageV: ReadOptional(MaxVoltBox),
        MaximumAbsCurrentA: ReadOptional(MaxCurrentBox),
        MaxSocRatePctPerMinute: ReadNumber(MaxSocRateBox),
        MaximumGapSeconds: ReadNumber(MaxGapBox),
        PositiveCurrentMeansDischarge: DischargeSignCheck.IsChecked == true);

    private async void ImportTelemetryClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = await PickFileAsync(".csv", ".tsv", ".txt");
            if (path is null) return;
            var thresholds = ReadThresholds();
            string text = await LocalDataFiles.ReadTextAsync(path,12_000_000);
            _telemetry = await Task.Run(() => TelemetryAnalyzer.Analyze(text, thresholds));
            _limits = thresholds;
            TelemetryResult.Text = string.Format(Culture,
                "{0:N0} 筆量測 · {1:N0} 項異常或提示 · 期間 {2:g}\n溫度 {3}～{4} °C · 放電能量估算 {5} Wh",
                _telemetry.Samples, _telemetry.TotalFindings, _telemetry.Duration,
                _telemetry.MinTemperatureC?.ToString("0.##",Culture) ?? "N/A",
                _telemetry.MaxTemperatureC?.ToString("0.##",Culture) ?? "N/A",
                _telemetry.EstimatedDischargedWh?.ToString("0.###",Culture) ?? "N/A");
            TelemetryList.Items.Clear();
            foreach (var f in _telemetry.Findings.Take(1000))
                TelemetryList.Items.Add(
                    $"Row {f.Row:00000}   [{f.Severity}] {f.Code} — {f.Message}");
            if (_telemetry.HiddenFindings > 0)
                TelemetryList.Items.Add($"...另有 {_telemetry.HiddenFindings} 筆未列於畫面。");
            if (_telemetry.TotalFindings == 0)
                TelemetryList.Items.Add("此輪門檻下未發現異常；不等於安全驗證通過。");
            OverviewTelemetry.Text = _telemetry.TotalFindings.ToString("N0",Culture) + " flags";
            RefreshVisuals();
            Notice("量測 CSV 分析完成。請確認門檻、電流正負號及原始資料。");
        }
        catch (Exception ex) { Notice(ex.Message, true); }
    }

    private async void ImportValidationClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = await PickFileAsync(".csv");
            if (path is null) return;
            string matrixCsv = await LocalDataFiles.ReadTextAsync(path,4_000_000);
            _matrix = await Task.Run(() => ValidationMatrix.Parse(matrixCsv));
            var summary = ValidationMatrix.Summarize(_matrix);
            ValidationResult.Text = string.Format(Culture,
                "Total {0:N0}   PASS {1:N0}   FAIL {2:N0}   NOT RUN {3:N0}   完成項目通過率 {4}%",
                summary.Total, summary.Passed, summary.Failed, summary.NotRun,
                summary.PassRatePercent?.ToString("0.0",Culture) ?? "N/A");
            ValidationList.Items.Clear();
            foreach (var item in _matrix.Take(400))
                ValidationList.Items.Add(
                    $"{item.Verdict,-8}  {item.TestId} · {item.Metric} " +
                    $"{item.Measured?.ToString("0.###",Culture) ?? "—"} {item.Unit} " +
                    $"[{item.Minimum?.ToString(Culture) ?? "—"}～{item.Maximum?.ToString(Culture) ?? "—"}]");
            if (_matrix.Count > 400)
                ValidationList.Items.Add($"另外 {_matrix.Count - 400} 筆未顯示；報告會包含全部案例。");
            OverviewValidation.Text = $"{summary.Failed} FAIL / {summary.NotRun} OPEN";
            RefreshVisuals();
            Notice("驗證矩陣已完成 PASS/FAIL/NOT RUN 整理。");
        }
        catch (Exception ex) { Notice(ex.Message, true); }
    }

    private void CalculatePackClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var design = new PackDesign(
                Series: ReadInteger(SeriesBox),
                Parallel: ReadInteger(ParallelBox),
                CellVoltageV: ReadNumber(CellVoltageBox),
                CellCapacityAh: ReadNumber(CellCapacityBox),
                CellMassG: ReadNumber(CellMassBox),
                NonCellMassG: ReadNumber(OverheadBox),
                LengthMm: ReadNumber(LengthBox),
                WidthMm: ReadNumber(WidthBox),
                HeightMm: ReadNumber(HeightBox),
                LoadW: ReadNumber(LoadBox));
            _packDesign = design;
            _pack = PackCalculator.Calculate(design);
            PackResultText.Text = string.Format(Culture,
                "電芯 {0} 顆 · 電壓 {1:0.##} V · 容量 {2:0.##} Ah\n" +
                "名目能量 {3:0.##} Wh · 重量 {4:0.##} g · 包絡體積 {5:0.####} L\n" +
                "重量 ED {6:0.#} Wh/kg · 體積 ED {7:0.#} Wh/L\n" +
                "固定負載理想續航 {8:0.##} 小時（未扣除損耗與預留）",
                _pack.CellCount, _pack.NominalVoltageV, _pack.CapacityAh,
                _pack.EnergyWh, _pack.TotalMassG, _pack.VolumeL,
                _pack.GravimetricWhKg, _pack.VolumetricWhL,
                _pack.IdealRuntimeHours);
            RefreshVisuals();
            Notice("電池包名目數據已計算。不是機構可行性或安規簽核。");
        }
        catch (Exception ex) { Notice(ex.Message, true); }
    }

    private string CreateReport()
    {
        string review=EngineeringReport.Build(ProjectBox.Text, AuthorBox.Text, _battery,
            _telemetry, _matrix, _pack, _limits);
        string batch=BatchReport();
        if (_batteryRecords.Count > 1)
        {
            review += "\n\n## Multiple installed batteries (separate readings)\n\n";
            for (int i = 0; i < _batteryRecords.Count; i++)
            {
                var b = _batteryRecords[i];
                review += string.Format(Culture,
                    "- Battery {0}: design {1:0.###} Wh; full charge {2:0.###} Wh; retention {3:0.0}%; cycles {4}\n",
                    i + 1, b.DesignWh, b.FullChargeWh, b.RetentionPercent,
                    b.Cycles?.ToString(Culture) ?? "not reported");
            }
            review += "\nThese batteries are not combined or treated as a single pack.\n";
        }
        if(_sleepDrain is not null)
            review += $"\n\n## Standby drain (entered values)\n" +
                $"SOC start {_sleepDrain.StartSoc:0.##}%, end {_sleepDrain.EndSoc:0.##}%, " +
                $"duration {_sleepDrain.DurationHours:0.##} hours, " +
                $"loss {_sleepDrain.LossPercentPerHour:0.###}%/h, " +
                $"estimated average power {_sleepDrain.AveragePowerW?.ToString("0.###", Culture) ?? "N/A"} W. " +
                "This is a manual SOC estimate, not power-meter evidence.\n";
        review += "\n\n## Next investigation steps\n";
        foreach(var step in EngineeringTriage.NextSteps(_telemetry,_battery,_matrix is not null))
            review += "- [ ] "+step+"\n";
        return string.IsNullOrWhiteSpace(batch)?review:review+"\n\n"+batch;
    }

    private void PreviewReportClick(object sender, RoutedEventArgs e)
    {
        ReportPreview.Text = CreateReport();
        Notice("工程摘要預覽已更新。");
    }

    private async void ExportReportClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "PackFit-Engineering-Review"
            };
            picker.FileTypeChoices.Add("Markdown", new List<string>{".md"});
            WinRT.Interop.InitializeWithWindow.Initialize(picker,
                WinRT.Interop.WindowNative.GetWindowHandle(this));
            var selected = await picker.PickSaveFileAsync();
            if (selected is null) return;
            string report = CreateReport();
            await File.WriteAllTextAsync(selected.Path, report,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            ReportPreview.Text = report;
            Notice("Markdown 工程報告已儲存。");
        }
        catch (Exception ex) { Notice(ex.Message, true); }
    }
}
