# Changelog

## Source preview — cross-platform engineering comparison (PR #8)

- Add baseline/candidate A/B comparison from two local telemetry logs under one explicit threshold set, including full-log per-code counts, per-1,000-row rates, temperature/voltage deltas, and comparability warnings.
- Extend native WinUI Telemetry Screening with file selectors, native normalized-rate bars, and a Markdown comparison handoff; reset stale comparisons when the session or batch changes.
- Add a no-backend .NET 10 CLI for Windows, Linux and macOS with Markdown/JSON output, strictly bounded local input and create-only exports.
- Test synthetic before/after fixtures on Linux CI and native WinUI smoke tests on Windows CI. Publish test expectations and privacy limitations in docs/CLI_AND_COMPARISON.md.
- **This PR is a source change, not an updated v0.8.0-beta.1 release or hardware-validation certificate.**

## Unreleased native workbench usability improvements (merged PR #7)

- Add provenance-aware WinUI session summaries, clearer native typography, filtered telemetry finding previews and explicit whole-log code counts.
- Require confirmation before replacing active data with synthetic examples; preserve user settings on session reset.
- Mark synthetic samples in engineering reports; validate session reset through actual native chart/demo smoke tests.
- **The older published v0.8.0-beta.1 binary does not contain these changes.**

## Unreleased maintenance and privacy hardening (merged PR #6; after v0.8.0-beta.1)

- Correct a genuine telemetry-review edge case: anomaly types appearing after the first 1,000 visible findings now remain in complete per-code statistics, engineering next steps and Markdown exports.
- Enforce a bounded byte budget when the native Windows app reads local HTML and CSV/TSV reports, including files that grow during import; support Unicode BOMs.
- Remove optional third-party executable animation JavaScript and online Google Fonts requests from the default browser app. Preserve system typography, native Web Animations, reduced motion and offline use (browser v0.6.4).
- Introduce a maintainer change workflow, CODEOWNERS, more useful PR review checklist, accurate public evidence log and a candid application-readiness assessment.
- Add GitHub artifact provenance attestations for future Windows CI ZIP builds. Previously published v0.8.0-beta.1 release assets are **not retroactively attested**.
- Add reproducible tests for late telemetry anomalies, file size limits, local animation and third-party request prevention.

These changes passed source review gates and main-branch CI after PR #6 was merged, but are **not** retroactively part of the older tagged v0.8.0-beta.1 Windows binary.

## v0.8.0-beta.1 — 2026-11-02 (published)

- Add functional native WinUI capacity-retention bars, single-battery capacity-history trends, four-signal telemetry charts, S/P cell topology diagrams and standby SOC/hour/optional Wh estimates.
- Add Basic/Engineering navigation, symptom-first triage, evidence-based next steps and one-click fully synthetic demo scenarios.
- Extend XAML-independent compatibility workspace with native charts, standby estimation, synthetic sample data and handoff tools.
- Add a bounded and chronological telemetry chart trace; large log datasets do not create unbounded chart objects.
- Respect missing data, per-signal units and multi-battery ambiguity; avoid unverified health color bands and automatic root-cause claims.
- Add pure C# regression vectors for history, standby calculations and trace sampling, along with native smoke tests that build each diagram in full and compatibility modes.
- Document engineering formulas, workflow assumptions and official Microsoft diagnostics.
- **[Public prerelease](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.8.0-beta.1)** with full WinUI ZIP, original-name single-file EXE and SHA-256 hashes. Passing hosted-Windows checks does not certify behavior on every real device.

## v0.7.0-beta.4 — 2026-11-02 (published)

- Investigated an actual Windows startup log showing successful App.xaml initialization followed by a MainWindow.InitializeComponent XAML parse failure.
- Fixed release packaging to preserve `PackFit.Engineering.exe` on portable builds; prior renamed binaries could affect XAML resource lookup on some Windows App SDK versions.
- Rebuilt `--safe-ui` as a XAML-independent native WinUI workspace with useful battery, CSV/TSV, validation, Pack and report tools, rather than relying on the failing main XAML.
- Added Windows startup diagnostic context, exception HRESULT/resource data and release tests that refuse to count a recovery-only process as a normal successful launch.
- Published native ZIP, unrenamed single-file executable and SHA-256 hashes. [Download beta.4](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.4).
- Original user-device XAML failure remains open until the user confirms on-device startup.

## v0.7.0-beta.3 — 2026-11-02 (published)

- Fix desktop deployment defaults: prefer a complete self-contained WinUI folder and include an easy-to-use CMD launcher and PowerShell diagnostics.
- Add bounded local startup logging, visible managed startup errors, a native recovery window, and `--safe-ui` mode to isolate custom title bar and motion failures.
- Improve Windows 11 Fluent visual hierarchy with Segoe UI Variable, OS CJK font fallback, roomier cards and responsive input/action layouts.
- Use a 190 ms page transition that honors Windows animation preferences and gracefully disables if unsupported.
- Extend the release pipeline with bundled dependency checks and an exploratory hosted Windows startup probe. **The original device failure is not yet confirmed resolved.**
- Publish ZIP, optional self-extracting EXE and SHA-256 hashes in [beta.3](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.3).

## Web v0.6.3 — 2026-11-02

- Keep browser battery-report parsing scoped to installed batteries; do not mistake capacity-history rows for current capacity.
- Support selecting one of several physical batteries without silently combining Wh values.
- Improve error handling for malformed or oversized battery-report imports by removing stale displayed results.
- Add bilingual selector text, visibility overrides, pure parser test cases and UI wiring checks.
- Refresh the service-worker cache version so updated scripts replace cached older versions.
- This is a web patch, separate from the already published Windows beta.2 binary.

## v0.7.0-beta.2 — 2026-11-02 (published)

- Fix native Windows battery-report parsing: restrict installed battery data, separate multiple batteries, reject ambiguous/malformed values.
- Add synthetic dual-battery/history fixture, independent C# regression vectors and an update to issue #2 documenting remaining supported-format questions.
- Replace the long README with task-based getting-started examples, accurate testing status and user onboarding.
- Add 15-minute privacy-safe browser/Windows trial plus first-time feedback issue template.
- Refresh the engineering release checklist, adoption evidence and Codex for OSS application text.
- **Status:** [Published prerelease](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.2), Windows ZIP/EXE and hashes built from the verified release workflow; physical Windows smoke-testing remains open.


## v0.7.0-beta.1 — 2026-11-02

- First public **repository beta**, combining the v0.6.1 browser app and the WinUI 3 engineering desktop preview.
- Introduced a clearer README explaining the battery engineer's motivation, everyday laptop use cases, responsible AI-assisted development and the limits of an early open-source project.
- Added release notes, publication automation requiring both web tests and Windows engineering core tests, Windows binaries with SHA-256 checksums, and a transparent beta designation.
- Fixed a stale service-worker cache assertion that had been causing website CI failures despite 72/73 assertions passing.
- Added a complete, current Codex for Open Source application pack with only substantiated claims.
- Actual desktop hardware smoke-testing and third-party adoption evidence remain open.

## Desktop Fluent + diagnostics workflow update — 2026-11-01

- Replaced the original plain WinUI shell with Mica, native TitleBar/pane toggle, Fluent navigation icons, layered cards and pinned InfoBar.
- Added guided investigation checklists for Modern Standby drain, charging issues, sudden shutdown, heat/throttling and battery capacity decline, with editable handoff notes.
- Added read-only Windows powercfg diagnostics, user-selected Energy/SleepStudy HTML reports, native shortcuts to Reliability Monitor/Event Viewer/DxDiag/MSInfo32.
- Added an official-tools guide for Windows Performance Recorder/Analyzer and Sysinternals Process Monitor, with optional local presence detection and no automatic installation.
- Added batch screening of up to 12 local telemetry logs, integrated batch summaries in Markdown reports, plus TSV/semicolon-delimited and explicit mV/mA column alias support.
- Added first-use synthetic CSV templates, source-level regression tests and documentation. Current updated Windows EXE build and launch require validation against a successful workflow run.

## Desktop engineering developer preview — 2026-11-01

- Added a separate native **WinUI 3 / C#** Windows application alongside the existing browser app (which remains v0.6.1).
- Introduced pure .NET engineering calculations and a standalone regression runner.
- Implemented Windows `powercfg` battery capacity reading, unit-aware telemetry CSV analysis, explicit validation matrix PASS/FAIL/NOT RUN classification, nominal pack design estimates, and auditable Markdown report generation.
- Added synthetic sample CSVs, extensive desktop instructions, and a Windows GitHub Actions workflow that builds self-contained `PackFit.Engineering.exe` distribution artifacts on success.
- Added an optional single-file EXE publishing attempt; **a successful build and actual Windows execution still require verification**, and the desktop remains a preview rather than a certified engineering tool.

## 0.6.1 — 2026-11-01

- Added optional, locally parsed Windows DxDiag text import for CPU model, RAM, GPU labels and OS where browser hints are insufficient.
- Added synthetic tests and responsive UI layout for the report import; original diagnostic data is never uploaded or stored.

## 0.6.0 — 2026-11-01

- Automatic OS-driven light/dark theme, updated live on system preference changes, plus explicit theme override selector and safe settings migration.
- Unified all seven workspaces with a responsive visual/layout system, consistent forms and card hierarchy.
- New Device Insights workspace: browser-exposed CPU thread count, coarse device memory, platform and graphics API availability.
- Optional user-triggered WebGL GPU identification and local Worker JavaScript computation probe; no telemetry or hardware fingerprint persistence.
- Fixed a previous navigation bug that only selected the first panel instead of all panels.
- Added tests for theme resolution, device hint boundaries, UI wiring and offline resources.

## 0.5.0 — 2026-11-01

- New warm ivory, graphite, muted plum and champagne palette in both themes, replacing green- and blue-heavy elements.
- Recolored battery ring, sample battery, input states, active navigation, chart series and result highlights consistently.
- Integrated optional MIT-licensed Motion animations for brief panel and calculation updates; native Web Animations API remains the offline fallback.
- Preserved reduced-motion behavior, pinned the optional external module version and extended regression checks.

## 0.4.0 — 2026-11-01

- Replaced abstract promotional headings with plain-language, task-first bilingual copy.
- Updated typography to Inter and Noto Sans TC with local system fallbacks and improved form readability.
- Simplified the palette, card surfaces, sidebar hierarchy and demo data illustration for a calmer interface.
- Removed persistent floating/glowing animations; added short motion feedback for changed calculation values, honoring reduced-motion settings.
- Updated offline CSS precache, manifest and documentation for the visual release.

## 0.3.0 — 2026-11-01

- Daily energy planning: editable usage duration and power by activity, available energy above reserve, charge estimate and energy shortfall.
- Validated and versioned portable JSON backup/restore for up to four editable scenarios, plus synthetic example files.
- Same-origin offline application caching with Service Worker and PWA manifest (browser support varies).
- Expanded mathematical, data interchange and offline asset-integrity tests.
- Bilingual quick-start documentation, contribution and adoption evidence guidance.

## 0.2.0 — 2026-11-01

- New My Battery workspace: opt-in browser status, offline Windows batteryreport and manual battery capacity retention.
- New Everyday Tools: charge cost, power-bank energy and approximate recharge time.
- Premium UI with refined light theme, original styling, micro-animation and reduced-motion support.
- Pure calculation and report parsing module with additional tests; no runtime dependencies or telemetry.

## 0.1.0 — 2026-11-01

- First public release: bilingual runtime and engineering calculators.
- Four-workspace responsive UI with dark/light theme.
- Bounded, validated S/P and envelope density calculations.
- Side-by-side scenario comparison, safe CSV export and local browser persistence.
- Modular calculation engine, automated unit tests and GitHub Actions CI.
- Transparent methodology and contribution/security policies.