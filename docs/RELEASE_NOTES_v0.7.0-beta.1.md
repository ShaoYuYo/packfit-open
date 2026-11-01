# PackFit Open v0.7.0-beta.1 — First Public Beta

This is the first **public beta** of PackFit Open, an independent MIT-licensed project created from the practical needs of battery development engineering and everyday laptop ownership.

The browser app remains **v0.6.1**; the **repository beta tag** packages the native Windows engineering desktop alongside the web source. We intentionally do not equate a build with field validation.

## Choose your version

### Everyday Web App (no installation)

[Launch PackFit Open](https://shaoyuyo.github.io/packfit-open/) for runtime estimates, Windows battery report interpretation, charging costs, daily energy plans and battery pack education. Local-first, bilingual Chinese/English.

### Windows Engineering Desktop Beta (Windows 10/11 x64)

Download one of the attached release assets **only if the release workflow successfully built and uploaded it**:

- **PackFit-Engineering-win-x64.zip** — recommended: extract **all** files, then run \`PackFit.Engineering.exe\` inside the extracted directory. Do not copy only the EXE out.
- **PackFit-Engineering-portable-win-x64.exe** — optional single-file packaging; it may unpack runtime components on first launch. Published only when that optional build succeeds.
- **SHA256SUMS.txt** — hashes for verifying the downloaded assets; compare them with a locally calculated Windows PowerShell \`Get-FileHash\`.

The program is **unsigned** and is not a verified corporate deployment package. It does not need an account or cloud service. Do not bypass company security policy, SmartScreen or IT restrictions.

## What is included

- Native **C# / WinUI 3** desktop app with Fluent materials (Mica, TitleBar, NavigationView, InfoBar) and system light/dark theme behavior.
- Windows \`powercfg\` battery-report parsing and capacity-retention estimates.
- User-controlled, read-only sleep/wake and energy diagnostic shortcuts, plus guided troubleshooting for standby drain, slow charging, sudden shutdown, overheating and capacity decline.
- Battery telemetry analysis from CSV/TSV/semicolon data with explicit units, configurable limits, abnormal SOC/time sequences and **up to 12-log batch comparisons**.
- Validation matrices classifying PASS, FAIL and NOT RUN using the engineer's own thresholds.
- Nominal battery pack series/parallel, Wh, mass and energy-density calculations.
- Local Markdown review reports, synthetic demonstration files and guidance for optional Microsoft WPR/WPA/Procmon tools.

## Evidence and limitations

The release workflow is configured to require **web JavaScript checks, web unit tests, and the Windows C# engineering checks** before publishing the native binary. It publishes from the same checked-out commit as the release tag.

This is **Beta**, not an ISO/IEC/UL/UN safety test, regulatory approval, production battery engineering release, or root-cause diagnosis. The results are estimates and screening observations that require review by qualified engineers and validated source data.

**Not yet verified by GitHub CI alone:** physical Windows 10/11 runtime behavior on every machine, all graphics/DPI configurations, hardware-dependent \`powercfg\` reports, input variants for every supplier, accessibility across assistive technologies, signing and enterprise deployment.

No private supplier, employer or customer hardware parameters are provided or used as public test examples.

## Who it is for

- **Everyday users:** quick answers to battery runtime, capacity and charging questions without specialized knowledge.
- **Battery and laptop engineers:** reduce repetitive log normalization, threshold checks, validation tables and handoff reporting.
- **Open-source contributors:** improve formats, tests, user experience and maintenance in a reproducible public repository.

## Links

- [Project rationale and README](../README.md)
- [Native desktop instructions](../desktop/README.md)
- [Beginner troubleshooting and Windows tools](../desktop/docs/TROUBLESHOOTING.md)
- [Engineering formulas](METHODOLOGY.md)
- [Known limitations / actual adoption evidence](PROJECT_EVIDENCE.md)
- [Issue tracker](https://github.com/ShaoYuYo/packfit-open/issues)
- [Code and contribution guide](../CONTRIBUTING.md)

**Feedback is welcome, but please share only synthetic/public test data and remove machine names, battery serials and proprietary measurements from reports.**
