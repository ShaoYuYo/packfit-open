# PackFit Engineering v0.8.0-beta.1 — Visual Battery Engineering

**This release adds functioning native WinUI diagrams and connects them to real local data workflows.** A user request for practical engineering visualizations motivated the update; it is not a safety certification or a claim of independently verified notebook hardware measurements.

## New functionality

1. **Battery Visuals** — per-battery capacity retention bars and capacity history if a Windows battery report provides an unambiguous history for one battery. The app will not silently attach a history series to a multi-battery report.
2. **Telemetry Charts** — four separate trend panels for V, A, °C and SOC, drawn directly with WinUI rather than a WebView. A bounded chart trace is retained, while the full source remains available for engineering review.
3. **Pack Diagram Studio** — cell-block topology view driven by S/P and nominal V/Ah/Wh calculations. Large S/P arrays are shown as an abbreviated sketch without losing the actual calculated cell count.
4. **Sleep & Drain** — before/after SOC bars and manually entered standby duration/Wh estimates, plus built-in Windows powercfg /a, /requests and /lastwake shortcuts.
5. **Issue-first troubleshooting** — symptom entry on the dashboard and native visual next-step flows connected to engineer notes and the report generator.
6. **Basic / Engineering modes** — Basic shows approachable tools, Engineering reveals telemetry, validation matrices, pack design, reporting and advanced resources.
7. **Demo mode** — a single click loads entirely fictional battery history, V/I/Temp/SOC data, a validation matrix and a Pack estimate so users can explore the charts without private data.
8. **Safe-mode parity** — the XAML-independent compatibility workspace includes native charts, demo input, standby estimation and handoff guidance, even when a device has trouble loading MainWindow.xaml.

## Download and first run

- **Recommended: `PackFit-Engineering-win-x64.zip`** — extract **all** files to a clean local folder and run `Start-PackFit.cmd` or `PackFit.Engineering.exe`.
- **Optional: `PackFit.Engineering.exe`** — original filename preserved for the self-extracting single-file distribution. Do **not rename it** after downloading.
- **`SHA256SUMS.txt`** — compare integrity hashes for the published assets.

Windows 10 (build 19041+) / Windows 11, x64. Native WinUI 3 and .NET 10; no spreadsheet editor, Python or graphing runtime required. Still unsigned: obey Windows and company security restrictions.

If the main UI cannot open, run `PackFit.Engineering.exe --safe-ui` inside the extracted folder. See [Windows startup help](https://github.com/ShaoYuYo/packfit-open/blob/main/desktop/docs/STARTUP.zh-TW.md).

## Verification gates

This beta must pass:

- Browser integrity and unit tests.
- .NET engineering regression checks (battery history, standby SOC/Wh math, bounded trace sampling and next-step hints).
- Windows native self-contained build.
- Real native `MainWindow` XAML construction and XAML-independent compatibility construction.
- **Additional hosted-Windows smoke tests** that execute all synthetic example workflows and create the native diagrams in both full and compatible workspaces.
- Asset filename checks, SHA-256 generation, and release tag tied to the exact built commit.

A successful hosted runner test does **not** prove every visual layout, driver diagnostic or assistive technology works on a physical PC. Actual user-device acceptance is still requested.

## Engineering integrity

- Default example thresholds are **demonstration values**, not OEM/supplier policies.
- Capacity history may include service/replacement discontinuities; the chart is not an aging model.
- Standby SOC delta is an estimate, not a calibrated power-meter result.
- The pack diagram is a nominal electrical topology, not PCB/BMS/thermal/mechanical design or a compliance sign-off.
- Engineering advice is a review checklist, not automatic root-cause identification.
- Personal Windows reports and proprietary supplier data stay local. Never share unredacted logs in public issues.

[Visual workflows and assumptions](https://github.com/ShaoYuYo/packfit-open/blob/main/desktop/docs/ENGINEERING_VISUALS.md) · [Engineering documentation](https://github.com/ShaoYuYo/packfit-open/tree/main/desktop) · [Feedback](https://github.com/ShaoYuYo/packfit-open/issues)
