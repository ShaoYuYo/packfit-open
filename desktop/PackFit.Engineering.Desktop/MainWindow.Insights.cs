using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PackFit.Engineering.Core;

namespace PackFit.Engineering.Desktop;

public sealed partial class MainWindow
{
    private IReadOnlyList<CapacityHistoryPoint> _batteryHistory = [];
    private PackDesign? _packDesign;
    private StandbyDrainResult? _sleepDrain;

    /// <summary>CI-only native smoke test that exercises real chart construction and sample workflows.</summary>
    internal void RunVisualSmokeTest()
    {
        LoadDemoClick(this,new RoutedEventArgs());
        CalculateSleepClick(this,new RoutedEventArgs());
        if(_batteryRecords.Count!=1 || _batteryHistory.Count<2 ||
           _telemetry?.Trace.Count<2 || _matrix is null ||
           _pack is null || _sleepDrain is null ||
           VisualsTelemetry.Children.Count<4 ||
           VisualsPack.Children.Count==0 ||
           SleepGraphCanvas.Children.Count==0)
            throw new InvalidOperationException("Engineering visuals smoke test failed.");
        if(!CreateReport().Contains("SYNTHETIC DEMO DATA",StringComparison.Ordinal) ||
           !HasSyntheticData || DataModeText.Text != "合成示範模式 · 非實測數據")
            throw new InvalidOperationException("Synthetic data provenance contract failed.");
        ClearAnalysisData();
        if(HasSessionData || HasSyntheticData || _telemetry is not null ||
           TelemetryList.Items.Count != 0 || OverviewBattery.Text != "尚未分析")
            throw new InvalidOperationException("Workspace reset smoke test failed.");
    }

    private void InitializeEngineeringInsights()
    {
        // Default to Engineering so existing users retain access to every tool.
        ExperienceMode.SelectedIndex = 1;
        RefreshVisuals();
        RenderGuideFlow();
        EngineeringVisuals.Workflow(SleepFlowCanvas,
            InvestigationGuides.Find("sleep-drain").Steps.Select(x=>x.Title+" — "+x.Action).ToArray());
    }

    private void RefreshVisuals()
    {
        EngineeringVisuals.Battery(BatteryVisualCanvas, _batteryRecords, _batteryHistory);
        EngineeringVisuals.Battery(VisualsBattery, _batteryRecords, _batteryHistory);
        EngineeringVisuals.Telemetry(TelemetryChartCanvas, _telemetry);
        EngineeringVisuals.Telemetry(VisualsTelemetry, _telemetry);

        if (_pack is not null && _packDesign is not null)
        {
            EngineeringVisuals.Pack(PackDiagramCanvas, _packDesign, _pack);
            EngineeringVisuals.Pack(VisualsPack, _packDesign, _pack);
        }
        else
        {
            PackDiagramCanvas.Children.Clear();
            VisualsPack.Children.Clear();
            PackDiagramCanvas.Children.Add(new TextBlock
            {
                Text="按「計算設計」後將繪出 S/P 拓樸圖。",
                TextWrapping=TextWrapping.Wrap
            });
            VisualsPack.Children.Add(new TextBlock
            {
                Text="尚未計算 Pack，或按「載入合成示範資料」。",
                TextWrapping=TextWrapping.Wrap
            });
        }

        ValidationChartCanvas.Children.Clear();
        if (_matrix is not null)
        {
            var info=ValidationMatrix.Summarize(_matrix);
            EngineeringVisuals.Validation(ValidationChartCanvas,
                (info.Total,info.Passed,info.Failed,info.NotRun));
        }
        else ValidationChartCanvas.Children.Add(new TextBlock {Text="匯入驗證矩陣後顯示 PASS / FAIL / NOT RUN 比例。"});

        EngineeringVisuals.Workflow(WorkflowDiagram,
            EngineeringTriage.NextSteps(_telemetry,_battery,_matrix is not null));
    }

    private void RenderGuideFlow()
    {
        if (GuideFlowCanvas is null) return;
        EngineeringVisuals.Workflow(GuideFlowCanvas,
            _guide.Steps.Select(x=>x.Title+" → "+x.Action).ToArray());
    }

    private void ExperienceModeChanged(object sender,SelectionChangedEventArgs e)
    {
        if (Navigation is null || ExperienceMode?.SelectedItem is not ComboBoxItem mode)
            return;
        bool basic=string.Equals(mode.Tag?.ToString(),"basic",StringComparison.Ordinal);
        var advanced=new HashSet<string>(StringComparer.Ordinal)
            {"telemetry","validation","pack","report"};
        foreach (var entry in Navigation.MenuItems)
        {
            if (entry is NavigationViewItem item)
            {
                string tag=item.Tag?.ToString()??"";
                item.Visibility=basic && advanced.Contains(tag)
                    ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        if (basic && _activePage is not null && advanced.Contains(_activePage))
            SelectPage("overview");
        Notice(basic
            ? "Basic：顯示電池、圖表、睡眠與問題排查；工程數據不會被刪除。"
            : "Engineering：已顯示全部測試、設計與報告工作區。");
    }

    private void GoSymptomClick(object sender,RoutedEventArgs e)
    {
        string id=(sender as Button)?.Tag?.ToString() ?? "battery-health";
        for(int i=0;i<GuideIssue.Items.Count;i++)
        {
            if(GuideIssue.Items[i] is ComboBoxItem item && item.Tag?.ToString()==id)
            {
                GuideIssue.SelectedIndex=i;
                break;
            }
        }
        RenderGuideFlow();
        SelectPage("guide");
    }

    private async void LoadDemoClick(object sender,RoutedEventArgs e)
    {
        try
        {
            if (!await ConfirmReplaceSessionAsync()) return;
            ClearAnalysisData();
            const string report = "<html><h2>Installed batteries</h2><table>" +
                "<tr><td>NAME</td><td>Synthetic Battery A</td></tr>" +
                "<tr><td>DESIGN CAPACITY</td><td>56,000 mWh</td></tr>" +
                "<tr><td>FULL CHARGE CAPACITY</td><td>49,000 mWh</td></tr>" +
                "<tr><td>CYCLE COUNT</td><td>234</td></tr></table>" +
                "<h2>Battery capacity history</h2><table>" +
                "<tr><th>PERIOD</th><th>FULL CHARGE CAPACITY</th><th>DESIGN CAPACITY</th></tr>" +
                "<tr><td>2026-08</td><td>52,000 mWh</td><td>56,000 mWh</td></tr>" +
                "<tr><td>2026-09</td><td>51,000 mWh</td><td>56,000 mWh</td></tr>" +
                "<tr><td>2026-10</td><td>49,000 mWh</td><td>56,000 mWh</td></tr></table></html>";
            const string sample = "Timestamp,Voltage_V,Current_A,Temp_C,SOC_pct\n" +
                "2026-11-01T00:00:00Z,11.60,1.10,33,80\n" +
                "2026-11-01T00:00:30Z,11.55,1.20,35,79\n" +
                "2026-11-01T00:01:00Z,11.49,1.32,36,78\n" +
                "2026-11-01T00:01:30Z,11.42,1.65,38,77\n" +
                "2026-11-01T00:02:00Z,11.38,1.75,41,76\n" +
                "2026-11-01T00:02:30Z,11.30,1.80,45,75\n" +
                "2026-11-01T00:03:00Z,11.21,2.10,54,74\n" +
                "2026-11-01T00:03:30Z,11.15,2.15,64,73\n" +
                "2026-11-01T00:04:00Z,11.08,2.08,57,72\n" +
                "2026-11-01T00:04:30Z,11.00,1.93,48,71\n" +
                "2026-11-01T00:05:00Z,10.95,1.83,42,70\n";
            _batteryRecords=BatteryReportParser.ParseAll(report);
            _batteryHistory=BatteryHistoryParser.Parse(report);
            _battery=_batteryRecords.Count==1?_batteryRecords[0]:null;
            _limits=ReadThresholds();
            _telemetry=TelemetryAnalyzer.Analyze(sample,_limits);
            _matrix=ValidationMatrix.Parse(SampleMatrix);
            _batterySynthetic = _telemetrySynthetic = _validationSynthetic = true;
            UpdateBattery();
            TelemetryResult.Text=$"合成示範 Log：{_telemetry.Samples} 筆 · {_telemetry.TotalFindings} 個篩選旗標";
            OverviewTelemetry.Text=$"{_telemetry.TotalFindings} sample flags";
            RenderTelemetryFindings();
            var val=ValidationMatrix.Summarize(_matrix);
            ValidationResult.Text=$"合成範例：{val.Passed} PASS · {val.Failed} FAIL · {val.NotRun} NOT RUN";
            OverviewValidation.Text=$"{val.Failed} FAIL / {val.NotRun} NOT RUN";
            ValidationList.Items.Clear();
            foreach(var item in _matrix)
                ValidationList.Items.Add($"{item.Verdict} · {item.TestId} · {item.Metric}");
            CalculatePackClick(sender,e);
            _packSynthetic = _pack is not null;
            RefreshVisuals();
            UpdateSessionStatus();
            Notice("已載入完全虛構的示範資料。圖表和旗標可供試用，不代表實測或安全驗證。");
            SelectPage("visuals");
        }
        catch(Exception ex)
        {
            Notice("示範資料載入失敗："+ex.Message,true);
        }
    }

    private void CalculateSleepClick(object sender,RoutedEventArgs e)
    {
        try
        {
            double? wh=ReadOptional(SleepCapacityWh);
            _sleepDrain=StandbyDrainEstimator.Estimate(
                ReadNumber(SleepStartSoc),ReadNumber(SleepEndSoc),
                ReadNumber(SleepHours),wh);
            EngineeringVisuals.Standby(SleepGraphCanvas,_sleepDrain);
            UpdateSessionStatus();
            Notice("待機掉電估算完成。輸入為手動 SOC，不是實測待機電功率。");
        }
        catch(Exception ex){Notice("待機分析資料有誤："+ex.Message,true);}
    }

    private async void RunSleepToolClick(object sender,RoutedEventArgs e)
    {
        if(sender is not Button button)return;
        string command=button.Tag?.ToString()??"";
        button.IsEnabled=false;
        SleepCommandOutput.Text="正在讀取 Windows 原始資訊...";
        try
        {
            string result=await WindowsDiagnostics.ExecuteAsync(command);
            SleepCommandOutput.Text=$"powercfg /{command} (local)\n\n"+result;
            Notice("Windows 診斷成功。請核對發生問題的時間點與電腦條件。");
        }
        catch(Exception ex)
        {
            SleepCommandOutput.Text="Windows 診斷未完成："+ex.Message;
            Notice("此指令不支援或缺少權限："+ex.Message,true);
        }
        finally {button.IsEnabled=true;}
    }
}
