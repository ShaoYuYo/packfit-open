# PackFit Engineering v0.7.0-beta.4 — Native startup compatibility

This beta addresses an actual user-supplied startup log:

- **App.xaml loaded successfully.**
- **MainWindow.InitializeComponent() threw a WinUI XAML parsing exception.**
- The recovery window appeared, so earlier GitHub process-liveness checks were insufficient.

The log does **not** identify the exact XAML node. The problem was not reproduced on the hosted Windows runner; its actual cause on the user's computer remains under investigation.

## Important packaging fix

Previous optional portable releases **renamed** a published self-contained executable from `PackFit.Engineering.exe` to `PackFit-Engineering-portable-win-x64.exe`. A corresponding [Microsoft WinUI bug report](https://github.com/microsoft/microsoft-ui-xaml/issues/10966) describes XAML parse errors after executable renaming in some self-contained apps. This may be relevant, but is **not yet confirmed as the cause of this user's failure**.

**The portable executable in this beta retains the exact original name: `PackFit.Engineering.exe`. Do not rename it.**

## Choose your download

- **`PackFit-Engineering-win-x64.zip` (recommended):** unzip *all* files to a new local folder. Launch `Start-PackFit.cmd` or `PackFit.Engineering.exe` from within the extracted directory.
- **`PackFit.Engineering.exe` (portable alternative):** separate self-extracting executable with its original publish name. Keep that name unchanged.
- **`SHA256SUMS.txt`:** SHA-256 hashes of the attached files.

The beta remains unsigned. Do not bypass SmartScreen or an organization's security restrictions.

## Startup resilience

- **`--safe-ui` now completely bypasses `MainWindow.xaml`** and opens an independent WinUI workbench created in C#, not the static failure screen from beta.3.
- The compatibility workbench provides real functionality: Windows battery HTML parsing, CSV/TSV screening, CSV validation-matrix summaries, illustrative battery-pack estimates, read-only Windows sleep-state diagnostic and Markdown reports.
- When the full MainWindow cannot load, the app automatically falls back to this compatible workbench instead of exposing an inert error-only window.
- Local startup logs now include Windows version, architecture, executable filename, HRESULT, exception details and safe/normal launch stage. Logs stay on your PC; review path and user information before sharing.
- Windows CI now has **two hard smoke tests**: full MainWindow XAML instantiation and independent compatibility workbench construction. It rejects builds that display only a recovery screen.

## Limits

Compatibility mode is **not** the full nine-workspace UI. Its thresholds and pack geometry are illustrative; nothing in PackFit constitutes battery safety certification, root-cause determination or official testing approval. The issue is open until a confirmed physical-device launch and reproducer are available.

[Traditional Chinese startup guide](https://github.com/ShaoYuYo/packfit-open/blob/main/desktop/docs/STARTUP.zh-TW.md) · [Open issue](https://github.com/ShaoYuYo/packfit-open/issues/4) · [Source code](https://github.com/ShaoYuYo/packfit-open/tree/main/desktop)
