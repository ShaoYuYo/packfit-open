# PackFit Engineering Desktop

**Native Windows engineering workbench · C# / WinUI 3 · MIT open source**

PackFit Engineering Desktop is a companion to the [browser edition](https://shaoyuyo.github.io/packfit-open/). The web app helps consumers estimate runtime; the desktop app is intended for **laptop battery, power, validation, reliability and system engineers** who need to turn scattered test measurements into clear, reproducible follow-up items.

> **Public beta available:** [v0.7.0-beta.2](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.2). Automated browser and C# tests passed; GitHub Actions successfully built both Windows distributions. **Real Windows hardware smoke-testing and enterprise code signing remain unverified.** No engineering safety or regulatory certification is implied.

## Windows 11 Fluent UI and guided workflow refresh

The native window now follows Microsoft's [modern WinUI 3 app structure](https://learn.microsoft.com/en-us/windows/apps/develop/ui/windows-app-sdk-app-structure): **Mica** background (with Windows 10 fallback), a built-in **TitleBar** integrated with the system caption buttons, icon-based **NavigationView**, Fluent cards, **InfoBar** status messages, **Expander** help sections and native keyboard-focus behavior. This is a real WinUI UI, not a WebView theme.

**Three new workflows for engineers who do not want to memorize commands:**

| Workspace | Purpose | Needs another installation? |
| --- | --- | --- |
| **問題排查引導 / Guided investigation** | Step-by-step evidence checklists for sleep-drain, slow charging, unexpected shutdown, heat/throttling and capacity decline; copy or save a reviewable Markdown investigation handoff. | **No** |
| **Windows 診斷工具箱 / Built-in diagnostics** | One-click read-only \`powercfg /a\`, \`/requests\`, \`/lastwake\`, \`/waketimers\`, \`/devicequery wake_armed\`. Generate Energy Report or SleepStudy to a path chosen by the user; open Reliability Monitor, Event Viewer or DxDiag. | **No**; some reports require administrator permissions or supported sleep modes |
| **工具與安裝指南 / Official tools** | Check whether \`wpr.exe\`, \`wpa.exe\`, and \`procmon.exe\` are on common executable search paths; explain when ETW or process-level analysis is necessary and link to official Microsoft resources. | Optional, **manual** installation for WPA/Procmon |

Read-only diagnostics never change power scheme settings and do not trigger automatic elevation or install third-party software. Reports stay on the local machine; users must remove sensitive details before sharing.

### Optional Microsoft tools

- **Windows Performance Recorder (WPR):** command-line version usually ships with Windows; captures ETW traces and may require administrator privileges. See [WPR official documentation](https://learn.microsoft.com/en-us/windows-hardware/test/wpt/introduction-to-wpr).
- **Windows Performance Analyzer (WPA):** used to inspect ETL traces, CPU scheduling, power/performance timelines. The graphical tool is included in the **Windows Performance Toolkit** from the [Windows ADK official download page](https://learn.microsoft.com/en-us/windows-hardware/get-started/adk-install/).
- **Process Monitor (Procmon):** Microsoft Sysinternals utility useful for file, registry and process/driver activity investigations. [Official download and guide](https://learn.microsoft.com/en-us/sysinternals/downloads/procmon).

The app opens these Microsoft pages on user request. **It does not auto-download, install or bypass enterprise IT policy.**

### Compare many supplier and system logs without Excel preprocessing

The **Telemetry Screening** screen can import **up to 12 local log files** at once, applying the same explicitly selected limits to each. A compact comparison lists sample counts, peak temperatures and flag counts. All successful file results are included in the exported Markdown report, along with any failed file imports.

The analyzer also recognizes \`CSV\`, **TSV (tab-separated)** and **semicolon-separated** rows. Headers explicitly declaring \`BatteryVoltage_mV\`, \`BattCurrent_mA\`, \`BatteryTemp_C\`, \`BatterySOC_pct\` and similar supported aliases are accepted without manually converting units in Excel. Unitless electrical values are **not silently guessed**. Time zone and current sign still require confirmation.

The comparison and troubleshooting guides are **screening aids**, not automatic root-cause analysis, verified SVTP compliance, or a replacement for independent engineering judgment.


Start with the [practical beginner troubleshooting guide](docs/TROUBLESHOOTING.md) if you are unsure which Windows tool to use. The [Fluent implementation notes](docs/FLUENT.md) explain how the native app shell follows Microsoft design guidance.

## Why a laptop engineer needs it

Repeated manual effort is often not in a complex equation. It is importing five supplier log files, normalizing mV and mA, checking unusual temperature and SOC behavior, reconciling test status from different teams, and copying consistent results into a review email.

This desktop edition targets those repetitive steps:

| Workspace | What it automates | Why this helps |
| --- | --- | --- |
| **Overview** | Local project context and consolidated issue counts | Stop checking multiple source spreadsheets just to see what still needs investigation |
| **Windows Battery Health** | Run `powercfg /batteryreport` directly; parse design capacity, full-charge capacity and cycle count | First-pass capacity/aging evidence with fewer manual steps |
| **Telemetry Screening** | Import timestamped CSV, convert mV/mA to V/A, flag thermal/voltage/current/SOC/time-gap anomalies | Find candidate suspect ranges in long logs |
| **Validation Matrix** | Read test cases with explicit min/max requirements; classify PASS/FAIL/NOT RUN | Review and track outstanding SVTP-like test cases without recreating formulas |
| **Pack Estimation** | Compute S/P energy, cell+overhead mass, envelope volume, Wh/kg, Wh/L and ideal runtime | Quickly sanity-check competing public or approved design assumptions |
| **Engineering Report** | Generate auditable Markdown reports with flags, test verdicts, batch comparisons and review checklists | Prepare a consistent handoff instead of copying results across tools |

**Important:** Findings are *screening flags*, not root-cause conclusions. Team reviews, battery manufacturer specifications, IEC/UL/UN transportation requirements and approvals remain necessary. You must independently validate thresholds and interpretations.

## Download and run the latest public beta

**[Open GitHub Releases — v0.7.0-beta.2](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.2)**

The initial beta release has three verified attached assets:

1. [**PackFit-Engineering-win-x64.zip**](https://github.com/ShaoYuYo/packfit-open/releases/download/v0.7.0-beta.2/PackFit-Engineering-win-x64.zip) — recommended. Download, extract every file, then run `PackFit.Engineering.exe` within the extracted folder.
2. [**PackFit-Engineering-portable-win-x64.exe**](https://github.com/ShaoYuYo/packfit-open/releases/download/v0.7.0-beta.2/PackFit-Engineering-portable-win-x64.exe) — a larger, optional single-file build. It may extract runtime files at startup; do not assume it is zero-extraction.
3. [**SHA256SUMS.txt**](https://github.com/ShaoYuYo/packfit-open/releases/download/v0.7.0-beta.2/SHA256SUMS.txt) — checksums for confirming the downloaded file matches the published artifact.

The release workflow performed web tests, engineering core tests and native WinUI compilation, then published the binaries from the **same commit**. Compilation and artifact presence are verified; **manual Windows 10/11 launch, hardware diagnostics, accessibility and performance are not yet independently validated**.

This beta is unsigned. Windows SmartScreen may warn about unsigned executables. Only download artifacts you trust; do **not** bypass company security policies. An enterprise deployment would require code signing and managed endpoint testing. [Individual Actions build artifacts](https://github.com/ShaoYuYo/packfit-open/actions/workflows/desktop-winui.yml) are available for developer investigation but are not the preferred user downloads.

### Build it yourself (Windows 10/11 x64)

Required: Windows machine with Windows App SDK-compatible developer tools, Visual Studio Windows desktop/WinUI workloads and .NET 10 SDK. WinUI project uses **Microsoft.WindowsAppSDK 2.6.4**.

```powershell
# 1. Download or clone the repository, then run the pure math checks:
dotnet run --project desktop/PackFit.Engineering.Checks/PackFit.Engineering.Checks.csproj -c Release

# 2. Publish the Windows app folder including the runtime:
dotnet publish desktop/PackFit.Engineering.Desktop/PackFit.Engineering.Desktop.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:Platform=x64 -p:WindowsPackageType=None `
  -p:WindowsAppSDKSelfContained=true -p:PublishSingleFile=false `
  -o ./dist/PackFit-Engineering-WinUI

# 3. Start the executable from the published folder:
./dist/PackFit-Engineering-WinUI/PackFit.Engineering.exe
```

The [Windows build workflow](../.github/workflows/desktop-winui.yml) repeats these steps and additionally **attempts** Microsoft's single-file publish mode. A folder-based self-contained app is the reliable fallback; a single-file EXE should only be advertised after that optional step passes.

## Practical workflow (繁體中文)

1. 開啟 **總覽**，填入專案名稱（例如公開測試用名稱）和工程師代號。
2. 到 **電池健康度**，點「在本機建立報告」。程式會呼叫 Windows 內建 `powercfg`、讀取容量數據，再刪除產生的暫存報告。也可以匯入已有的 HTML。
3. 在 **量測 CSV 分析** 設定專案測試門檻，然後匯入資料。先核對是否以正電流表示放電，並確認電壓是**電池包電壓**而非單顆電芯電壓。
4. 在 **驗證矩陣** 匯入 PASS/FAIL/NOT RUN 測試案例。
5. 用 **電池包估算** 快速對照串並聯、容量、體積密度和重量。
6. 最後在 **工程報告** 產生預覽並儲存 `.md`，即可放進受控工作文件或版本管理系統中，由真正負責的人審查。

### Telemetry CSV example

Download [synthetic telemetry data](samples/telemetry-sample.csv).

```csv
Timestamp,Voltage_V,Current_A,Temp_C,SOC_pct
2026-11-01T00:00:00Z,11.5,2.0,34,80
2026-11-01T00:00:10Z,11.48,2.02,36,79.9
```

- **Timestamp** requires an explicit ISO-8601 time zone (such as `Z` or `+08:00`). Clock order and sample gaps are checked.
- Supply at least one of `Voltage_V`/`Voltage_mV`, `Current_A`/`Current_mA`, `Temp_C`, `SOC_pct`.
- Current convention defaults to **positive means discharge**; change the checkbox if your logger uses negative discharge current.
- Pack voltage range, maximum absolute current, temperature thresholds, SOC slope and sample-gap threshold are user-configurable.
- A **disabled voltage/current limit** is not a PASS; it means this rule is not checked.
- Energy integration uses approximate trapezoidal voltage×current, for periods where both are available, and is not a calibrated capacity measurement.
- Max CSV input: 12 MB or 200,000 rows per file; batch mode accepts up to 12 files and 50 MB total. Only first 1,000 findings are expanded in reports; the full count is retained.

**No universal battery limit:** the default temperatures (−20°C / 60°C) are *illustrative*, not specifications for a particular chemistry, model, OEM or safety standard. Consult your approved design and test plan.

### Validation matrix CSV example

Download [synthetic test matrix](samples/validation-sample.csv).

```csv
TestID,Category,Metric,Measured,Min,Max,Unit,Owner,Notes
THERM-001,Thermal,Surface peak,49,,60,C,EE,Example only
ELEC-002,Electrical,Charge voltage,13.4,12.4,13.2,V,EE,Example only
SYS-003,System,Sleep drop,,0,3,percent,System,Pending
```

When measured is empty, verdict is **NOT RUN**, not PASS. Minimum/maximum values are not guessed from the test name. CSV fields support single-line quoted values and doubled quotation marks, but not multiline fields.

## Architecture

```text
desktop/
  PackFit.Engineering.Core/        Pure .NET 10 computation, parser and report models
    PackModels.cs                  Series/parallel, mass, volume and Wh
    BatteryHealth.cs               Offline Windows battery report parsing
    Telemetry.cs                   Unit-aware time-series screening
    Validation.cs                  Explicit thresholds and verdicts
    EngineeringReport.cs           Traceable Markdown review output
  PackFit.Engineering.Checks/      Independent C# regression runner
  PackFit.Engineering.Desktop/     C# + WinUI 3 native application
    App.xaml(.cs)                  Native app lifecycle
    MainWindow.xaml(.cs)           Fluent desktop navigation and workflows
  samples/                         Synthetic CSVs; no vendor/customer data
.github/workflows/desktop-winui.yml
```

### Privacy & confidentiality

- Files are opened **only at the user's request**. Analyses stay in desktop memory; there is no network backend, telemetry service, API key or automated uploads.
- The `powercfg` report is created in a randomly named temp file and deleted after parsing when possible.
- Some real CSV/HTML reports can contain system names, serial numbers or supplier intellectual property. **Never submit confidential files to public GitHub Issues, samples or PRs.**
- Public example data and limits are intentionally synthetic.
- Generated reports are **drafts**, not release sign-offs, certified safety data or verified root-cause analyses.

## What comes next

After the initial WinUI build is proven on real hardware: richer multi-file time-series alignment, supplier issue tracking, real-time notebook power telemetry (if safe, permissioned APIs are available), test spec versioning and revision history, export to structured XLSX/PDF, automated regression plots and engineer approval workflow. These are **proposals, not completed features**.

## References

- [Microsoft: distribute an unpackaged WinUI 3 app](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/unpackage-winui-app)
- [Windows App SDK self-contained deployment](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/self-contained-deploy/deploy-self-contained-apps)
- [Build workflow](../.github/workflows/desktop-winui.yml)

MIT licensed. See [repository license](../LICENSE).
