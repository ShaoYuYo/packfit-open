using System.Globalization;
using Microsoft.UI.Xaml;
using PackFit.Engineering.Core;
using Windows.Storage.Pickers;

namespace PackFit.Engineering.Desktop;

public sealed partial class MainWindow
{
    private readonly List<TelemetryBatchItem> _batches=[];
    private readonly List<string> _batchImportErrors=[];

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
            long totalBytes=0;
            foreach(var file in selected)
            {
                try
                {
                    long size=new FileInfo(file.Path).Length;
                    totalBytes+=size;
                    if(size>12_000_000||totalBytes>50_000_000)
                        throw new FormatException("檔案超出單檔 12 MB / 總量 50 MB 限制。");
                    TelemetryAnalysis analysis=await Task.Run(()=>
                        TelemetryAnalyzer.Analyze(File.ReadAllText(file.Path),limits));
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
            var totalFlags=_batches.Sum(x=>x.Analysis.TotalFindings);
            OverviewTelemetry.Text=$"{_batches.Count} files / {totalFlags} flags";
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
        return markdown;
    }
}
