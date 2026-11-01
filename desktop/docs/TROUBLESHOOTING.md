# PackFit Engineering — Beginner & Engineer Troubleshooting

This guide assumes you have downloaded and extracted the **entire** successful WinUI distribution ZIP. It is not necessary to install Excel, Python or a third-party tool for the core features.

## 1. Which page should I use?

- **Battery capacity, aging, available charge** → 電池健康度 → 建立報告. Uses built-in Windows \`powercfg /batteryreport\`.
- **Laptop loses charge while sleeping** → 問題排查引導 → 合蓋或待機後掉電. Then use Windows 診斷工具箱 for \`/a\`, \`/requests\`, \`/lastwake\` and \`/waketimers\`.
- **Laptop unexpectedly shuts down** → 問題排查引導 → 突然關機, and use Windows Reliability Monitor/Event Viewer.
- **Test data from multiple suppliers** → 量測 CSV 分析 → 批次匯入. Use the same limits and current-sign convention for the files being compared.
- **Need a review-ready issue summary** → 問題排查引導 → tick completed steps → 複製／儲存 Markdown.
- **Need to track test completion** → 驗證矩陣 → import CSV → PASS/FAIL/NOT RUN. To start from scratch, use the built-in synthetic sample generator.
- **Need a more complete power/performance trace** → 工具與安裝指南 → WPR/WPA links.

## 2. Which tools are already included in Windows?

| Windows tool | Purpose | How PackFit opens it | Extra installation? |
| --- | --- | --- | --- |
| powercfg | Battery report, available sleep states, power requests, wake events | Native allowlisted command runner | No |
| Reliability Monitor | Historical app/system reliability and failures | perfmon /rel shortcut | No |
| Event Viewer | Event traces, BugCheck, WHEA and device errors | Windows Event Viewer shortcut | No |
| DxDiag | CPU, GPU, system and DirectX diagnosis | DxDiag shortcut | No |
| MSInfo32 | System inventory | System Information shortcut | No |
| WPR.exe | Advanced ETW capture | Optional command-line tool already included with many Windows versions | Usually no for command-line |

Commands that need administrative rights **remain subject to Windows permissions**. PackFit will not auto-elevate or change Windows power schemes.

## 3. What software might I install later?

- **Windows Performance Analyzer (WPA)**: analyzing ETL traces and power/CPU timelines. Download the **Windows Performance Toolkit** component from [Microsoft Windows ADK](https://learn.microsoft.com/en-us/windows-hardware/get-started/adk-install/).
- **Process Monitor (Procmon)**: advanced process, filesystem, registry, and driver events. Only download from the [official Microsoft Sysinternals page](https://learn.microsoft.com/en-us/sysinternals/downloads/procmon).

No software is installed automatically. Follow corporate permissions and security controls.

## 4. Files do not import

Telemetry uses time-stamped \`CSV\`, \`TSV\`, or semicolon-separated \`txt\` data. Required: an ISO-8601 \`Timestamp\` containing a time-zone offset (e.g. \`2026-11-01T09:15:00+08:00\`) and at least one recognized measurement column. Examples:

\`\`\`
Timestamp,Voltage_V,Current_A,Temp_C,SOC_pct
Timestamp,Voltage_mV,Current_mA,Temperature_C,SOC_percent
Timestamp\tBattVoltage_mV\tBattCurrent_mA\tBatteryTemp_C\tBatterySOC_pct
\`\`\`

The app does not guess electrical units for ambiguous, unitless headers. Columns with unknown names must be explicitly renamed to documented ones. Multiline quoted fields are not supported. The app handles a bounded number of rows and clearly reports failures.

**Multiple-source comparison requires comparable units, timestamps, instrument conditions and thresholds.** Do not interpret a larger warning count as an automatically worse battery pack.

## 5. SleepStudy fails

SleepStudy is intended for supported Modern Standby systems, and some operations require elevation. First run the built-in \`powercfg /a\` shortcut to see available sleep states; refer to [Microsoft SleepStudy guidance](https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/modern-standby-sleepstudy). PackFit does not bypass access restrictions or change sleep settings.

## 6. Energy Report fails

Microsoft recommends the \`/energy\` diagnostic when the computer is **idle**. The app runs a 30-second capture only after you explicitly start it. Save it to a new HTML path; reports may contain private process names and identifiers.

## 7. A report says FAIL

The **test matrix** shows FAIL only against your explicit Min/Max. A result is NOT RUN when the measurement is missing. If a limit is absent, this check is not evaluated. The software cannot certify a laptop battery or infer the applicable supplier cell specification from product name.

## 8. Privacy and safe sharing

Battery reports, event logs, ETL traces, DxDiag and telemetry may include serial numbers, filenames, system names and proprietary company information. Keep original data on authorized storage, redact before sharing, and do not upload internal test data to public GitHub issues. All PackFit analyses execute locally.

## 9. Build or launch errors

- Check the [WinUI build workflow](../../.github/workflows/desktop-winui.yml) actually passed on the version you downloaded.
- Extract the **complete** self-contained artifact; do not copy only its executable out of the folder.
- The build is unsigned. Do not bypass organization-managed SmartScreen or installer rules.
- On Windows 10, Mica gracefully falls back to a solid backdrop.
- Report reproducible issues using only synthetic examples and the exact compiler errors from GitHub Actions.
