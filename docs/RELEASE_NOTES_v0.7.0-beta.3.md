# PackFit Open v0.7.0-beta.3 — Startup Diagnostics & Fluent UI Refresh

**This beta responds to a real user report: the previous WinUI desktop build did not start on their Windows computer.** The exact device-specific cause has **not yet been confirmed**. This release improves startup recovery, gives users actionable local logs, and refreshes the native interface. It does **not** claim that every startup issue is resolved.

## Download (Windows 10 build 19041+ / Windows 11, x64)

**Recommended:** `PackFit-Engineering-win-x64.zip`.

1. Download the ZIP and extract **every file and folder** to a normal local directory. Do not open the EXE from within the ZIP or move it out alone.
2. Double-click `Start-PackFit.cmd`, which starts the native WinUI app in the correct directory and shows an exit code if the process fails.
3. If the app fails to start, open a terminal in the extracted folder and run `./PackFit.Engineering.exe --safe-ui` (PowerShell uses `.\PackFit.Engineering.exe --safe-ui`), or run `./Diagnostics/Diagnose-PackFit.ps1` for a local report.
4. The private diagnostic log is at `%LOCALAPPDATA%\PackFitOpen\Logs\startup.log`. Do not post unredacted logs on public GitHub issues.

**Alternative:** `PackFit-Engineering-portable-win-x64.exe` is a single-file self-extracting distribution. According to Microsoft, its native dependencies are unpacked to temporary storage at startup. It is provided for convenience, not recommended for initial troubleshooting. A corporation's endpoint restrictions may block it.

`SHA256SUMS.txt` contains integrity hashes. The executable remains unsigned, so follow your organization's security rules and do not bypass SmartScreen or IT policies.

## Startup and deployment changes

- Made **folder-based self-contained WinUI 3** the default project output; the single-file executable remains an explicitly separate release option.
- Added a bounded local startup log, fatal error dialog and minimal **native recovery window** for errors thrown by main-window initialization.
- Added `--safe-ui`: skip custom title bar and optional page animation to help isolate UI initialization issues.
- Included beginner-friendly launcher and a PowerShell diagnostic inventory/event probe with no network upload, elevation or security-policy bypass.
- Published a [Traditional Chinese startup investigation guide](https://github.com/ShaoYuYo/packfit-open/blob/main/desktop/docs/STARTUP.zh-TW.md).
- Current GitHub CI confirms .NET core checks and compiles self-contained Windows distributions; **it does not constitute user-device launch verification**.

## Native visual refinement

- Use Windows' system Fluent typography: **Segoe UI Variable** for English and the OS-selected Traditional Chinese fallback (Microsoft JhengHei UI), with clearer 12 / 14 / 18 / 22 / 30 px hierarchy.
- Use a quiet native Mica application shell, grouped sections, more legible KPI values and better spacing.
- Responsively stack primary actions, metric cards and input groups when the content width narrows.
- Use a short, noncontinuous 190 ms page fade that respects Windows' system animation preference. If animation setup fails, the page still opens without effects.

## Known limitations and important safety notes

- **The reported startup failure is still being investigated** until the device error log or a confirmed test run is available.
- The user environment may fail before the managed C# entry point; in that case, a Windows Event Viewer faulting module/exception code is necessary.
- The app is not a battery safety, regulatory, release sign-off or root-cause certification tool.
- No telemetry, device logs or confidential files are sent to an API or vendor. The local startup log may contain private paths — review and redact it before sharing.
- Physical Windows 10/11 smoke-testing, code signing, nonstandard battery reports and screen-reader usability remain open acceptance tasks.

[Desktop help](https://github.com/ShaoYuYo/packfit-open/blob/main/desktop/README.md) · [Windows startup guide](https://github.com/ShaoYuYo/packfit-open/blob/main/desktop/docs/STARTUP.zh-TW.md) · [Open issues](https://github.com/ShaoYuYo/packfit-open/issues)
