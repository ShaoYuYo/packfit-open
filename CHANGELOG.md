# Changelog

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

# Changelog

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