# Changelog


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