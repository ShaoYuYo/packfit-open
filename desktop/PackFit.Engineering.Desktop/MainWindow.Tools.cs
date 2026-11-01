using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PackFit.Engineering.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;

namespace PackFit.Engineering.Desktop;

public sealed partial class MainWindow
{
    private InvestigationPlan _guide = InvestigationGuides.All[0];
    private readonly List<CheckBox> _guideChecks = [];

    private void SetupGuides()
    {
        GuideIssue.SelectedIndex=0;
        DiagnosticChoice.SelectedIndex=0;
        RenderGuide();
    }

    private void SelectPage(string tag)
    {
        foreach(var entry in Navigation.MenuItems)
        {
            if(entry is NavigationViewItem item && item.Tag?.ToString()==tag)
            {
                Navigation.SelectedItem=item;
                return;
            }
        }
    }
    private void AppTitleBar_PaneToggleRequested(TitleBar sender,object args)
        => Navigation.IsPaneOpen = !Navigation.IsPaneOpen;

    private void GoGuideClick(object sender,RoutedEventArgs e)=>SelectPage("guide");
    private void GoDiagnosticsClick(object sender,RoutedEventArgs e)=>SelectPage("diagnostics");
    private void GoResourcesClick(object sender,RoutedEventArgs e)=>SelectPage("resources");

    private void GuideIssueChanged(object sender,SelectionChangedEventArgs e)
    {
        if (GuideIssue?.SelectedItem is ComboBoxItem option)
        {
            _guide=InvestigationGuides.Find(option.Tag?.ToString()??"sleep-drain");
            RenderGuide();
        }
    }

    private void RenderGuide()
    {
        if(GuideSteps is null || GuideSummary is null)return;
        _guideChecks.Clear();
        GuideSteps.Children.Clear();
        GuideSummary.Text=_guide.Summary;
        foreach(var step in _guide.Steps)
        {
            var complete=new CheckBox {Content=step.Title,MinHeight=38};
            _guideChecks.Add(complete);
            var details=new StackPanel {Spacing=7,Padding=new Thickness(10)};
            details.Children.Add(new TextBlock{Text="為什麼："+step.Why,TextWrapping=TextWrapping.Wrap});
            details.Children.Add(new TextBlock{Text="建議操作："+step.Action,TextWrapping=TextWrapping.Wrap});
            details.Children.Add(new TextBlock{Text="保留證據："+step.Evidence,TextWrapping=TextWrapping.Wrap});
            GuideSteps.Children.Add(new Expander{
                Header=complete,Content=details,IsExpanded=false,
                HorizontalAlignment=HorizontalAlignment.Stretch
            });
        }
    }

    private string GuideMarkdown()
    {
        string result=InvestigationGuides.ToMarkdown(_guide,GuideNotes.Text??"");
        for(int n=0;n<_guide.Steps.Count&&n<_guideChecks.Count;n++)
            if(_guideChecks[n].IsChecked==true)
                result=result.Replace("- [ ] "+_guide.Steps[n].Title,
                    "- [x] "+_guide.Steps[n].Title,StringComparison.Ordinal);
        return result;
    }
    private static void CopyText(string text)
    {
        var data=new DataPackage();
        data.SetText(text);
        Clipboard.SetContent(data);
    }
    private void CopyGuideClick(object sender,RoutedEventArgs e)
    {
        CopyText(GuideMarkdown());
        Notice("排查清單已複製。貼入工作紀錄前請確認機密資料處理規範。");
    }
    private async void SaveGuideClick(object sender,RoutedEventArgs e)
    {
        try
        {
            var picker=new FileSavePicker{SuggestedStartLocation=PickerLocationId.DocumentsLibrary,
                SuggestedFileName="PackFit-Investigation"};
            picker.FileTypeChoices.Add("Markdown",new List<string>{".md"});
            WinRT.Interop.InitializeWithWindow.Initialize(picker,
                WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file=await picker.PickSaveFileAsync();
            if(file is null)return;
            await File.WriteAllTextAsync(file.Path,GuideMarkdown());
            Notice("排查清單已儲存為 Markdown。");
        }catch(Exception ex){Notice(ex.Message,true);}
    }

    private async void RunDiagnosticClick(object sender,RoutedEventArgs e)
    {
        if(DiagnosticChoice.SelectedItem is not ComboBoxItem selected)return;
        string id=selected.Tag?.ToString()??"";
        RunDiagnosticButton.IsEnabled=false;
        DiagnosticProgress.IsActive=true;
        DiagnosticProgress.Visibility=Visibility.Visible;
        DiagnosticOutput.Text="正在向 Windows 讀取資料，請勿關閉視窗…";
        try
        {
            var command=WindowsDiagnostics.BuiltIn.First(x=>x.Id==id);
            string result=await WindowsDiagnostics.ExecuteAsync(id);
            // Display is local only; the output may contain machine/software names.
            DiagnosticOutput.Text="$ "+string.Join(" ",new[]{"powercfg"}.Concat(command.Args))+
                "\n\n"+result;
            Notice("診斷已完成。報告屬參考資料，不代表找到根因。");
        }
        catch(Exception ex)
        {
            DiagnosticOutput.Text="無法完成診斷："+ex.Message+
                "\n部分 Windows 指令可能需要系統管理員權限；本工具不會自動提權。";
            Notice("Windows 診斷失敗，請閱讀原始訊息。",true);
        }
        finally
        {
            DiagnosticProgress.IsActive=false;
            DiagnosticProgress.Visibility=Visibility.Collapsed;
            RunDiagnosticButton.IsEnabled=true;
        }
    }

    private void CopyDiagnosticClick(object sender,RoutedEventArgs e)
    {
        CopyText(DiagnosticOutput.Text);
        Notice("結果已複製。對外分享前請移除裝置、程序、使用者等敏感資訊。");
    }
    private async Task SavePowercfgReportAsync(string type)
    {
        try
        {
            var picker=new FileSavePicker{
                SuggestedStartLocation=PickerLocationId.DocumentsLibrary,
                SuggestedFileName=type=="energy"?"PackFit-EnergyReport":"PackFit-SleepStudy"
            };
            picker.FileTypeChoices.Add("HTML report",new List<string>{".html"});
            WinRT.Interop.InitializeWithWindow.Initialize(picker,
                WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file=await picker.PickSaveFileAsync();
            if(file is null)return;
            Notice(type=="energy"?"正在分析系統耗電，請讓電腦保持閒置。":"正在建立待機報告。");
            string path=await WindowsDiagnostics.CreateHtmlReportAsync(type,file.Path);
            DiagnosticOutput.Text=$"報告已建立：{path}\n\nWindows 可能要求系統管理員權限；請檢查報告中時段是否與問題重現一致。";
            Notice("Windows HTML 診斷報告已儲存。");
        }
        catch(Exception ex)
        {
            Notice("無法建立報告："+ex.Message+
                "。可檢查權限、睡眠模式或是否已存在同名檔案。",true);
        }
    }
    private async void RunEnergyReportClick(object sender,RoutedEventArgs e)
        =>await SavePowercfgReportAsync("energy");
    private async void RunSleepStudyClick(object sender,RoutedEventArgs e)
        =>await SavePowercfgReportAsync("sleepstudy");

    private void LaunchWindowsTool(string executable,string? argument=null)
    {
        try
        {
            var info=new ProcessStartInfo(executable){UseShellExecute=true};
            if(argument is not null)info.ArgumentList.Add(argument);
            Process.Start(info);
            Notice("已交由 Windows 開啟工具。");
        }
        catch(Exception ex){Notice("無法開啟工具："+ex.Message,true);}
    }
    private static readonly string SampleTelemetry =
        "Timestamp,Voltage_V,Current_A,Temp_C,SOC_pct\n" +
        "2026-11-01T00:00:00Z,11.50,2.0,34,80\n" +
        "2026-11-01T00:00:10Z,11.45,2.1,35,79.8\n";
    private static readonly string SampleMatrix =
        "TestID,Category,Metric,Measured,Min,Max,Unit,Owner,Notes\n" +
        "THERM-001,Thermal,Surface temp,49,,60,C,EE,Synthetic example\n" +
        "ELEC-002,Electrical,Pack voltage,13.4,12.4,13.2,V,EE,Synthetic example\n" +
        "SYS-003,System,Sleep drop,,0,3,percent,System,Not measured\n";

    private async Task SaveExampleAsync(string name,string contents)
    {
        try
        {
            var picker=new FileSavePicker{
                SuggestedStartLocation=PickerLocationId.DocumentsLibrary,
                SuggestedFileName=name
            };
            picker.FileTypeChoices.Add("CSV",new List<string>{".csv"});
            WinRT.Interop.InitializeWithWindow.Initialize(picker,
                WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file=await picker.PickSaveFileAsync();
            if(file is null)return;
            await File.WriteAllTextAsync(file.Path,contents,new System.Text.UTF8Encoding(true));
            Notice("合成範例已儲存，可使用 Windows 記事本編輯，不一定需要 Excel。");
        }
        catch(Exception ex){Notice("無法儲存範例："+ex.Message,true);}
    }
    private async void SaveTelemetryExampleClick(object sender,RoutedEventArgs e)
        =>await SaveExampleAsync("PackFit-Telemetry-Example",SampleTelemetry);
    private async void SaveValidationExampleClick(object sender,RoutedEventArgs e)
        =>await SaveExampleAsync("PackFit-Validation-Example",SampleMatrix);
    private void OpenSystemInfoClick(object sender,RoutedEventArgs e)
        =>LaunchWindowsTool("msinfo32.exe");

    private void OpenReliabilityClick(object sender,RoutedEventArgs e)
        =>LaunchWindowsTool("perfmon.exe","/rel");
    private void OpenEventViewerClick(object sender,RoutedEventArgs e)
        =>LaunchWindowsTool("eventvwr.msc");
    private void OpenDxDiagClick(object sender,RoutedEventArgs e)
        =>LaunchWindowsTool("dxdiag.exe");

    private void CheckExternalToolsClick(object sender,RoutedEventArgs e)
    {
        string status(string exe)=>WindowsDiagnostics.IsAvailable(exe)
            ?"在 PATH 或 System32 找到":"未在常見路徑找到（不代表沒有安裝）";
        ToolAvailability.Text=
            "WPR（Windows Performance Recorder）："+status("wpr.exe")+"\n"+
            "WPA（Windows Performance Analyzer）："+status("wpa.exe")+"\n"+
            "Procmon（Process Monitor）："+status("procmon.exe")+
            "\n\n此檢查不會在電腦上安裝任何東西。";
    }

    private void OpenOfficialUrl(string url)
    {
        try
        {
            // Only the four fixed Microsoft Learn URLs below are exposed.
            Process.Start(new ProcessStartInfo(url){UseShellExecute=true});
        }catch(Exception ex){Notice("無法開啟瀏覽器："+ex.Message,true);}
    }
    private void OpenAdkDocsClick(object sender,RoutedEventArgs e)
        =>OpenOfficialUrl("https://learn.microsoft.com/en-us/windows-hardware/get-started/adk-install/");
    private void OpenWprDocsClick(object sender,RoutedEventArgs e)
        =>OpenOfficialUrl("https://learn.microsoft.com/en-us/windows-hardware/test/wpt/introduction-to-wpr");
    private void OpenProcmonDocsClick(object sender,RoutedEventArgs e)
        =>OpenOfficialUrl("https://learn.microsoft.com/en-us/sysinternals/downloads/procmon");
}
