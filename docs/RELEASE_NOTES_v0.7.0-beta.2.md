# PackFit Open v0.7.0-beta.2 — Battery Report Reliability Update

This second public beta focuses on **correctness, reproducible tests, and a clearer first-time user experience**. It is not a claim of independent production adoption or completed real-device certification.

## Fixes since beta.1

- **Installed-battery section parsing:** capacity-history data elsewhere in a Windows powercfg battery report can no longer be mistaken for the current installed battery when the standard section heading exists.
- **Multiple physical batteries:** supported repeated battery groups and separate tables are shown as **distinct readings** in the Windows application and Markdown handoff. Values are not silently merged into one battery.
- **Safer failure modes:** malformed capacity units, incomplete records and files above the HTML size limit are rejected.
- **Regression coverage:** added synthetic tests for English/Chinese headings, nested heading markup, historical tables, multiple batteries and unsupported formats.
- **Hands-on reproducibility:** added an independently reproducible [15-minute trial guide](https://github.com/ShaoYuYo/packfit-open/blob/main/docs/TRY_PACKFIT.md), [dual-battery synthetic fixture](https://github.com/ShaoYuYo/packfit-open/blob/main/desktop/samples/battery-report-two-batteries.html), and [first-time feedback template](https://github.com/ShaoYuYo/packfit-open/issues/new?template=trial_feedback.md).
- **Clearer README:** practical examples, project motivation, test evidence, limitations and contribution paths without excessive promotional text.

## Download (Windows 10/11 x64)

- **PackFit-Engineering-win-x64.zip** (recommended): extract all files; launch the native PackFit.Engineering.exe from the extracted folder.
- **PackFit-Engineering-portable-win-x64.exe** (optional): single-file packaging that may extract runtime files on launch.
- **SHA256SUMS.txt**: verify the downloaded assets.

Release artifacts are published only after successful browser checks, .NET core tests and Windows WinUI compilation, built from the same source commit.

## Important limitations

This is **developer beta software**, not a validated battery safety tester or regulatory test program. The Windows application is unsigned. Battery report formats differ by Windows version, language and hardware; **multi-column report layouts still need tests**. Real physical-device startup, Windows hardware diagnostics, accessibility, and broader independent user adoption remain open verification tasks. Use only approved/fictional data and do not publish identifying or employer/supplier reports.

Want to help? Try [the 15-minute checklist](https://github.com/ShaoYuYo/packfit-open/blob/main/docs/TRY_PACKFIT.md) and send a reproducible report with synthetic or sanitized data.
