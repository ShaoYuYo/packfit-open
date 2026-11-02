using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PackFit.Engineering.Desktop;

/// <summary>
/// Provenance-aware session controls and bounded, searchable telemetry preview.
/// The complete per-code counts come from the analyzer, not the first 1,000 UI rows.
/// This never stores or sends imported reports beyond the user's local session.
/// </summary>
public sealed partial class MainWindow
{
    private bool _batterySynthetic;
    private bool _telemetrySynthetic;
    private bool _validationSynthetic;
    private bool _packSynthetic;

    private bool HasSyntheticData =>
        (_batteryRecords.Count > 0 && _batterySynthetic) ||
        (_telemetry is not null && _telemetrySynthetic) ||
        (_matrix is not null && _validationSynthetic) ||
        (_pack is not null && _packSynthetic);

    private bool HasSessionData =>
        _batteryRecords.Count > 0 || _telemetry is not null ||
        _matrix is not null || _pack is not null || _batches.Count > 0 ||
        _batchImportErrors.Count > 0 || _sleepDrain is not null;

    private static string SourceLabel(bool exists, bool demo) =>
        !exists ? "尚無資料" : demo ? "合成示範資料" : "本機資料 / 計算";

    private void UpdateSessionStatus()
    {
        if (DataModeText is null) return;
        var hasDemo = HasSyntheticData;
        var hasLocal =
            (_batteryRecords.Count > 0 && !_batterySynthetic) ||
            (_telemetry is not null && !_telemetrySynthetic) ||
            (_matrix is not null && !_validationSynthetic) ||
            (_pack is not null && !_packSynthetic) ||
            _batches.Count > 0 || _sleepDrain is not null;

        DataModeText.Text = !HasSessionData ? "尚未建立分析工作階段" :
            hasDemo && hasLocal ? "混合資料來源 · 需逐項核對" :
            hasDemo ? "合成示範模式 · 非實測數據" :
            "本機分析工作階段";
        DataModeDetails.Text = !HasSessionData
            ? "先匯入 Windows 電池報告或量測 CSV；也可使用完全虛構的示例熟悉流程。"
            : hasDemo
                ? "目前保留了合成範例。報告會明確標記每項資料來源；請勿把示例當成實測或安全驗證。"
                : "分析結果留在這台電腦。門檻和圖表僅供初步篩選，仍須確認來源與測試規格。";
        OverviewBatterySource.Text = SourceLabel(_batteryRecords.Count > 0, _batterySynthetic);
        OverviewTelemetrySource.Text = SourceLabel(_telemetry is not null, _telemetrySynthetic);
        if (_telemetry is null && _batches.Count > 0)
            OverviewTelemetrySource.Text = "本機批次量測資料";
        OverviewValidationSource.Text = SourceLabel(_matrix is not null, _validationSynthetic);
    }

    private void FindingFilter_TextChanged(object sender, TextChangedEventArgs e) =>
        RenderTelemetryFindings();

    private void RenderTelemetryFindings()
    {
        if (TelemetryList is null || FindingPreviewCaption is null) return;
        TelemetryList.Items.Clear();
        if (_telemetry is null)
        {
            FindingCodeSummary.Text = "匯入量測 CSV 後，顯示完整 Log 的分類統計。";
            FindingPreviewCaption.Text = "目前沒有異常明細。";
            return;
        }

        FindingCodeSummary.Text = _telemetry.FindingCodeCounts.Count == 0
            ? "全份量測資料未觸發目前啟用的門檻；不代表通過安全驗證。"
            : "完整 Log 的異常分類：" + string.Join(" · ",
                _telemetry.FindingCodeCounts
                    .OrderByDescending(item => item.Value)
                    .ThenBy(item => item.Key, StringComparer.Ordinal)
                    .Select(item => item.Key + " " +
                        item.Value.ToString("N0", CultureInfo.InvariantCulture)));
        string term = FindingFilterBox?.Text.Trim() ?? "";
        var matches = _telemetry.Findings.Where(item =>
            term.Length == 0 ||
            item.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            item.Severity.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            item.Message.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            item.Row.ToString(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase)
        ).ToList();
        const int maxVisible = 250;
        foreach (var item in matches.Take(maxVisible))
            TelemetryList.Items.Add(
                "Row " + item.Row.ToString("00000", CultureInfo.InvariantCulture) +
                " · [" + item.Severity + "] " + item.Code + " — " + item.Message);
        if (matches.Count == 0)
            TelemetryList.Items.Add(term.Length == 0
                ? "此輪未觸發篩選門檻；不等於安全審核通過。"
                : "沒有符合條件的預覽明細。請檢查上方完整分類統計。");
        FindingPreviewCaption.Text =
            "預覽顯示 " + Math.Min(matches.Count, maxVisible).ToString("N0", CultureInfo.InvariantCulture) +
            " / " + matches.Count.ToString("N0", CultureInfo.InvariantCulture) +
            " 筆符合條件的明細（只搜尋前 " +
            _telemetry.Findings.Count.ToString("N0", CultureInfo.InvariantCulture) +
            " 筆）。完整資料共 " +
            _telemetry.TotalFindings.ToString("N0", CultureInfo.InvariantCulture) +
            " 項旗標，另有 " +
            _telemetry.HiddenFindings.ToString("N0", CultureInfo.InvariantCulture) +
            " 筆未保留明細；完整分類統計包含全部旗標。";
    }

    private async Task<bool> ConfirmReplaceSessionAsync()
    {
        if (!HasSessionData) return true;
        var dialog = new ContentDialog
        {
            XamlRoot = WorkspaceRoot.XamlRoot,
            Title = "要以合成示例取代目前的分析嗎？",
            Content = "目前已載入或計算的資料將從畫面清除。請先輸出需要保留的報告；示例完全是虛構數據，不會自動上傳。",
            PrimaryButtonText = "載入示例並取代",
            CloseButtonText = "保留目前資料",
            DefaultButton = ContentDialogButton.Close
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private async void ClearWorkspaceClick(object sender, RoutedEventArgs e)
    {
        if (!HasSessionData)
        {
            Notice("目前沒有需要清除的分析資料。");
            return;
        }
        var dialog = new ContentDialog
        {
            XamlRoot = WorkspaceRoot.XamlRoot,
            Title = "清除目前分析資料？",
            Content = "這會清空本次工作階段的電池、量測、驗證、電池包估算及示範資料。輸入的專案名稱與門檻設定會保留；已匯出的檔案不受影響。",
            PrimaryButtonText = "清除資料",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        ClearAnalysisData();
        SelectPage("overview");
        Notice("分析資料已清除；設定及已匯出的檔案保留。");
    }

    private void ClearAnalysisData()
    {
        _battery = null;
        _batteryRecords = [];
        _batteryHistory = [];
        _telemetry = null;
        _limits = null;
        _matrix = null;
        _pack = null;
        _packDesign = null;
        _sleepDrain = null;
        _batterySynthetic = _telemetrySynthetic = _validationSynthetic = _packSynthetic = false;
        _batches.Clear();
        _batchImportErrors.Clear();
        ResetComparison();
        OverviewBattery.Text = "尚未分析";
        OverviewTelemetry.Text = "尚未匯入";
        OverviewValidation.Text = "尚未匯入";
        BatteryResult.Text = "尚未讀取。請先執行報告或匯入 HTML。";
        TelemetryResult.Text = "尚未匯入 CSV。";
        ValidationResult.Text = "尚未匯入驗證矩陣。";
        PackResultText.Text = "尚未計算電池包估算。";
        ReportPreview.Text = "";
        SleepCommandOutput.Text = "";
        SleepGraphCanvas.Children.Clear();
        ValidationList.Items.Clear();
        BatchTelemetryList.Items.Clear();
        FindingFilterBox.Text = "";
        RenderTelemetryFindings();
        RefreshVisuals();
        UpdateSessionStatus();
    }

    private string DataProvenanceMarkdown()
    {
        var body = new StringBuilder();
        body.AppendLine("## Data provenance");
        if (HasSyntheticData)
            body.AppendLine("> **SYNTHETIC DEMO DATA — NOT FIELD MEASUREMENTS OR VALIDATION.**");
        body.AppendLine("- Battery: " + SourceLabel(_batteryRecords.Count > 0, _batterySynthetic));
        body.AppendLine("- Telemetry: " + SourceLabel(_telemetry is not null, _telemetrySynthetic));
        body.AppendLine("- Validation: " + SourceLabel(_matrix is not null, _validationSynthetic));
        body.AppendLine("- Pack calculation: " + SourceLabel(_pack is not null, _packSynthetic));
        body.AppendLine("- Batch logs: " + (_batches.Count > 0
            ? _batches.Count.ToString(CultureInfo.InvariantCulture) + " local files"
            : "none"));
        body.AppendLine("- Standby: " + (_sleepDrain is null ? "not calculated" : "manual SOC inputs"));
        body.AppendLine();
        body.AppendLine("Threshold flags are screening observations, not a certified safety or release verdict.");
        return body.ToString();
    }
}
