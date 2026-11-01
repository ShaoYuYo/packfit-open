namespace PackFit.Engineering.Core;

/// <summary>Local troubleshooting checklist; no diagnosis is asserted without evidence.</summary>
public sealed record InvestigationStep(string Title, string Why, string Action, string Evidence);
public sealed record InvestigationPlan(string Id, string DisplayName, string Summary,
    IReadOnlyList<InvestigationStep> Steps);

public static class InvestigationGuides
{
    public static IReadOnlyList<InvestigationPlan> All { get; } =
    [
        new("sleep-drain","合蓋或待機後掉電","先分辨正常待機耗電與異常喚醒，記錄待機前後電量、時間、AC 狀態。",
        [
            new("確認睡眠模式","不同平台可能採用 S3 或 Modern Standby，分析方法不同。",
                "執行 powercfg /a 查看支援的待機狀態。","powercfg /a 輸出"),
            new("確認電力要求","背景應用程式或驅動程式可能阻止系統進入低功耗狀態。",
                "執行 powercfg /requests 與 /waketimers。","電力要求／喚醒計時器清單"),
            new("檢查最近喚醒","先確認醒來的時間點與觸發來源，避免把系統喚醒誤判為電池本身失效。",
                "執行 powercfg /lastwake。","最後喚醒來源"),
            new("取得待機分析","支援 Modern Standby 的設備可用 SleepStudy 看活動時段與原因。",
                "由 Windows 工具箱建立 SleepStudy；若不支援，保留錯誤並確認 sleep states。","SleepStudy HTML 與時間範圍"),
            new("交叉比對","確認報告中的事件是否與問題時間、SOC 變化一致。",
                "整理重現步驟、睡眠前後電量、AC 狀態及韌體版本。","時間軸與待確認事項")
        ]),
        new("slow-charge","充電慢／無法充滿","核對 AC 供電、充電模式、功率限制與系統負載；不可單用電源供應器瓦數推估實際充電電流。",
        [
            new("先記錄條件","電池 SOC、溫度、電源變壓器與使用情況影響充電速度。",
                "記錄測試前 SOC、AC 規格、溫度、負載與充電模式。","前置條件表"),
            new("讀取電池報告","滿充容量與設計容量可以提供背景，但不足以證明充電慢的原因。",
                "執行 Windows Battery Report。","設計／滿充容量、循環次數"),
            new("比對時間序列","應依量測設備的電流正負號、功率與溫度判斷。",
                "匯入 Telemetry CSV，設定專案門檻。","電流／電壓／溫度／SOC 曲線證據"),
            new("列出排除項","電源連接、系統負載、韌體和充電策略均可能造成限制。",
                "記錄不同 AC / BIOS / load 條件下的比較。","A/B 測試結果與負責人")
        ]),
        new("shutdown","突然關機／重啟","先確定系統事件和觸發條件，再對照電池電壓、溫度與 SOC。",
        [
            new("固定重現步驟","單次重啟不代表電池異常。",
                "記錄時間、AC/DC、工作負載、溫度、SOC、BIOS/OS。","事件時間戳"),
            new("看 Windows 可靠性紀錄","事件可提供線索，但事件 ID 並不自動指向根因。",
                "開啟可靠性監視器及事件檢視器。","系統錯誤、BugCheck、WHEA 資料"),
            new("做電性對照","系統事件與供電異常可能相關，但需要時間軸比對。",
                "分析同時段的 V/I/Temp/SOC Log。","時間同步的異常範圍"),
            new("規劃跨團隊排查","如果沒有明確電池證據，不應直接判定供應商問題。",
                "分配 Power、System、BIOS、Battery 檢查項目。","Owner、下一步與待確認資料")
        ]),
        new("heat","高溫／風扇／效能下降","熱與效能限制常跟工作負載、冷卻及電源策略有關，應避免單因歸咎電池。",
        [
            new("記錄負載與環境","測試場景需可重現。",
                "紀錄 AC/DC、環境溫度、CPU/GPU 負載、風扇與機器姿態。","測試條件"),
            new("建立能源效率報告","Windows /energy 可找部分電力相關問題；不代表完整熱分析。",
                "在電腦閒置時由工具箱執行 /energy。","Energy Report HTML"),
            new("擷取進階效能追蹤","需要細部 CPU/系統熱點時，可使用 WPR/WPA。",
                "查看 Windows 工具箱中的 WPR/WPA 官方指南。","ETL trace 與分析結論"),
            new("比對感測器資料","不同感測器位置、時間和單位不能直接混用。",
                "匯入 Telemetry CSV，核對每個溫度欄位來源。","溫度閾值、電性與環境對照")
        ]),
        new("battery-health","容量下降／續航變短","辨別容量變化與系統耗電升高，兩者可能同時發生。",
        [
            new("確認容量","容量保留率是初步指標，不是安全或故障診斷。",
                "執行電池報告，比對 design/full-charge capacity。","容量保留率、cycle count"),
            new("記錄負載","相同電池在不同系統負載下續航會大幅變化。",
                "固定亮度、網路、負載及 AC/DC 條件。","比較條件"),
            new("檢查背景電源要求","背景程式可能提高平均負載。",
                "執行 /requests 與 /energy（閒置）。","功耗證據與警告"),
            new("確認是否需要升級調查","區分老化、電源管理與電池 pack 的差異。",
                "用工程報告建立跨團隊檢查清單。","Review owner 與下一步")
        ])
    ];

    public static InvestigationPlan Find(string id)
        => All.FirstOrDefault(x => x.Id == id) ?? All[0];

    public static string ToMarkdown(InvestigationPlan plan, string notes)
    {
        var lines=new List<string>
        {
            $"# Investigation — {plan.DisplayName}",
            "",
            $"Issue: {plan.Summary}",
            "Evidence status: UNVERIFIED; this is a checklist, not a root-cause conclusion.",
            ""
        };
        foreach(var step in plan.Steps)
        {
            lines.Add($"- [ ] {step.Title}");
            lines.Add($"  - Why: {step.Why}");
            lines.Add($"  - Action: {step.Action}");
            lines.Add($"  - Evidence: {step.Evidence}");
        }
        lines.Add("");
        lines.Add("## Engineer notes");
        lines.Add(notes.Replace("\r"," ").Replace("<","&lt;").Replace(">","&gt;"));
        lines.Add("");
        lines.Add("## Review");
        lines.Add("- [ ] Timestamp and units verified");
        lines.Add("- [ ] AC/DC, BIOS/EC and OS conditions captured");
        lines.Add("- [ ] System/Battery/Power owner and next action assigned");
        return string.Join("\n",lines)+"\n";
    }
}
