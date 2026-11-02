using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PackFit.Engineering.Core;
using Windows.Storage.Pickers;

namespace PackFit.Engineering.Desktop;

public sealed partial class MainWindow
{
    private readonly List<TelemetryBatchItem> _batches=[];
    private readonly List<string> _batchImportErrors=[];
    private TelemetryComparisonResult? _comparison;

    private async void ImportTelemetryBatchClick(object sender,RoutedEventArgs e)
    {
        TelemetryBatchButton.IsEnabled=false;
        try
        {
            var picker=new FileOpenPicker{SuggestedStartLocation=PickerLocationId.DocumentsLibrary};
            picker.FileTypeFilter.Add(".csv");
            picker.FileTypeFilter.Add(".tsv");
            picker.FileTypeFilter.Add(".txt");
            WinRT.Interop.InitializeWithWindow.Initialize(picker,
                WinRT.Interop.WindowNative.GetWindowHandle(this));
            var selected=await picker.PickMultipleFilesAsync();
            if(selected is null||selected.Count==0)return;
            if(selected.Count>12)throw new FormatException("一次最多分析 12 份 CSV。");
            var limits=ReadThresholds();
            _limits=limits;
            _batches.Clear();
            _batchImportErrors.Clear();
            BatchTelemetryList.Items.Clear();
            ResetComparison();
            long totalBytes=0;
            foreach(var file in selected)
            {
                try
                {
                    long size=new FileInfo(file.Path).Length;
                    totalBytes+=size;
                    if(size>12_000_000||totalBytes>50_000_000)
                        throw new FormatException("檔案超出單檔 12 MB / 總量 50 MB 限制。");
                    string batchCsv = await LocalDataFiles.ReadTextAsync(file.Path,12_000_000);
                    TelemetryAnalysis analysis=await Task.Run(()=>
                        TelemetryAnalyzer.Analyze(batchCsv,limits));
                    _batches.Add(new TelemetryBatchItem(file.Name,analysis));
                    BatchTelemetryList.Items.Add(
                        $"{file.Name}  ·  {analysis.Samples:N0} rows  ·  " +
                        $"{analysis.TotalFindings:N0} flags  ·  peak " +
                        $"{analysis.MaxTemperatureC?.ToString("0.##",CultureInfo.InvariantCulture)??"N/A"} °C");
                }
                catch(Exception error)
                {
                    string issue=$"{file.Name}: {error.Message}";
                    _batchImportErrors.Add(issue);
                    BatchTelemetryList.Items.Add("未匯入："+issue);
                }
            }
            PopulateComparisonChoices();
            var totalFlags=_batches.Sum(x=>x.Analysis.TotalFindings);
            OverviewTelemetry.Text=$"{_batches.Count} files / {totalFlags} flags";
            UpdateSessionStatus();
            Notice($"批次分析完成：成功 {_batches.Count} 份，需處理 {_batchImportErrors.Count} 份。" +
                " 每個檔案需確認時間範圍、單位與測試門檻。",_batches.Count==0);
        }
        catch(Exception ex){Notice(ex.Message,true);}
        finally{TelemetryBatchButton.IsEnabled=true;}
    }

    private string BatchReport()
    {
        if(_batches.Count==0 && _batchImportErrors.Count==0)return "";
        var markdown=TelemetryBatch.ToMarkdown(_batches);
        if(_batchImportErrors.Count!=0)
        {
            markdown+="\n### Files not analyzed\n\n";
            foreach(var issue in _batchImportErrors)
                markdown+="- "+issue.Replace("\r"," ").Replace("\n"," ")
                    .Replace("<","&lt;").Replace(">","&gt;")+"\n";
        }
        if (_comparison is not null)
            markdown += "\n\n" + _comparison.ToMarkdown();
        return markdown;
    }

    internal void RunComparisonSmokeTest()
    {
        if (_telemetry is null)
            throw new InvalidOperationException("Load the synthetic telemetry fixture first.");
        _batches.Clear();
        _batches.Add(new TelemetryBatchItem("synthetic-before.csv", _telemetry));
        _batches.Add(new TelemetryBatchItem("synthetic-after.csv", _telemetry));
        ResetComparison();
        PopulateComparisonChoices();
        CompareBatchClick(this, new RoutedEventArgs());
        if (_comparison is null || _comparison.Findings.Count == 0 ||
            !ComparisonResult.Text.Contains("synthetic-after.csv", StringComparison.Ordinal) ||
            !CreateReport().Contains("Baseline vs candidate", StringComparison.Ordinal))
            throw new InvalidOperationException("Native batch comparison rendering or report handoff failed.");
    }

    private void ResetComparison()
    {
        _comparison = null;
        if (BaselineChoice is null || CandidateChoice is null) return;
        BaselineChoice.Items.Clear();
        CandidateChoice.Items.Clear();
        ComparisonFlags.Children.Clear();
        ComparisonResult.Text = "請先批次匯入至少兩份 CSV / TSV。";
        CompareFilesButton.IsEnabled = false;
    }

    private void PopulateComparisonChoices()
    {
        for (int i = 0; i < _batches.Count; i++)
        {
            string label = (i + 1).ToString(CultureInfo.InvariantCulture) +
                " · " + _batches[i].FileName;
            BaselineChoice.Items.Add(label);
            CandidateChoice.Items.Add(label);
        }
        if (_batches.Count >= 2)
        {
            BaselineChoice.SelectedIndex = 0;
            CandidateChoice.SelectedIndex = 1;
            CompareFilesButton.IsEnabled = true;
            ComparisonResult.Text =
                "已準備 " + _batches.Count + " 份本機 Log。請選擇基準和候選，再按比較。";
        }
        else
        {
            ComparisonResult.Text = "需要至少兩份成功解析的 Log 才能比較。";
        }
    }

    private void ComparisonSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BaselineChoice is null || CandidateChoice is null) return;
        _comparison = null;
        ComparisonFlags.Children.Clear();
        if (BaselineChoice.SelectedIndex >= 0 && CandidateChoice.SelectedIndex >= 0)
            ComparisonResult.Text = "選擇已變更；請按「比較兩份量測」重新產生結果。";
    }

    private void CompareBatchClick(object sender, RoutedEventArgs e)
    {
        try
        {
            int before = BaselineChoice.SelectedIndex;
            int after = CandidateChoice.SelectedIndex;
            if (before < 0 || after < 0 || before >= _batches.Count || after >= _batches.Count)
                throw new InvalidOperationException("請選擇兩份已成功匯入的量測資料。");
            if (before == after)
                throw new InvalidOperationException("基準與候選必須是兩份不同的量測檔案。");
            _comparison = TelemetryComparison.Compare(_batches[before], _batches[after]);
            RenderComparison();
            Notice("已比較相同門檻下的兩份 Log；結論仍須核對負載、SOC 範圍與試驗條件。");
        }
        catch (Exception ex) { Notice("無法比較：" + ex.Message, true); }
    }

    private void RenderComparison()
    {
        if (_comparison is null) return;
        var result = _comparison;
        string f(double? n) => n?.ToString("0.##", CultureInfo.InvariantCulture) ?? "N/A";
        string delta(double? first, double? second) => first is null || second is null
            ? "N/A"
            : (second.Value-first.Value).ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture);
        ComparisonResult.Text =
            "Before: " + result.BaselineSource + "  ·  " + result.Baseline.Samples.ToString("N0", CultureInfo.InvariantCulture) + " samples\n" +
            "After: " + result.CandidateSource + "  ·  " + result.Candidate.Samples.ToString("N0", CultureInfo.InvariantCulture) + " samples\n" +
            "最高溫 " + f(result.Baseline.MaxTemperatureC) + " → " + f(result.Candidate.MaxTemperatureC) +
            " °C  (Δ " + delta(result.Baseline.MaxTemperatureC,result.Candidate.MaxTemperatureC) + " °C)\n" +
            "最低 Pack V " + f(result.Baseline.MinVoltageV) + " → " + f(result.Candidate.MinVoltageV) +
            " V  (Δ " + delta(result.Baseline.MinVoltageV,result.Candidate.MinVoltageV) + " V)\n" +
            "全部異常旗標 " + result.Baseline.TotalFindings.ToString("N0",CultureInfo.InvariantCulture) +
            " → " + result.Candidate.TotalFindings.ToString("N0",CultureInfo.InvariantCulture) +
            " (不代表 Pass/Fail)";
        ComparisonFlags.Children.Clear();
        ComparisonFlags.Children.Add(new TextBlock
        {
            Text = "依異常代碼比較（每 1,000 筆樣本）",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            FontSize = 14
        });
        foreach (var row in result.Findings.OrderByDescending(x=>Math.Max(
                     x.BaselinePerThousandSamples,x.CandidatePerThousandSamples)).Take(15))
        {
            double max = Math.Max(1,
                Math.Max(row.BaselinePerThousandSamples, row.CandidatePerThousandSamples));
            var column = new StackPanel { Spacing = 3 };
            column.Children.Add(new TextBlock
            {
                Text = row.Code + " · " + row.BaselineCount + " → " + row.CandidateCount +
                    "  (Δ " + row.CountDelta.ToString("+#;-#;0",CultureInfo.InvariantCulture) + ")",
                FontSize = 12, TextWrapping = TextWrapping.Wrap
            });
            column.Children.Add(new TextBlock
            {
                Text = "Baseline  " + f(row.BaselinePerThousandSamples) + " / 1k",
                FontSize = 11
            });
            column.Children.Add(new ProgressBar
            {
                Minimum = 0, Maximum = max, Value = row.BaselinePerThousandSamples,
                Height = 5
            });
            column.Children.Add(new TextBlock
            {
                Text = "Candidate  " + f(row.CandidatePerThousandSamples) + " / 1k",
                FontSize = 11
            });
            column.Children.Add(new ProgressBar
            {
                Minimum = 0, Maximum = max, Value = row.CandidatePerThousandSamples,
                Height = 5
            });
            ComparisonFlags.Children.Add(column);
        }
        if (result.Findings.Count == 0)
            ComparisonFlags.Children.Add(new TextBlock
            {
                Text = "兩份 Log 在目前門檻下沒有分類旗標；不代表安全驗證通過。",
                TextWrapping = TextWrapping.Wrap
            });
        ComparisonFlags.Children.Add(new TextBlock
        {
            Text = "比較前檢查：\n• " + string.Join("\n• ", result.Cautions),
            TextWrapping = TextWrapping.Wrap, FontSize = 12
        });
    }
}
