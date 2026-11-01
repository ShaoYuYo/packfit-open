# Agent and contributor instructions for PackFit Open

This repository is a **battery math and education tool**, not a battery safety, charging or regulatory validator.

## Product and engineering priorities

1. Keep core math pure, explicit about units and independently testable.
2. Keep browser-only operation, offline compatibility and user privacy.
3. Never use confidential customer, supplier or employer specifications as examples or fixtures.
4. Treat imported HTML and JSON as untrusted input; do not execute or interpolate it into innerHTML.
5. Preserve Traditional Chinese and English translations, keyboard use and reduced-motion support.
6. Update assumptions, tests, docs and versioned cache assets when changing features.
7. Do not fabricate user numbers, Stars, contributors, endorsements, certifications or benchmarks.

## Module ownership

- src/calc.js: nominal electrical feasibility formulas.
- src/consumer.js: daily consumer formulas and Windows batteryreport parser.
- src/planner.js: constant-average-power energy schedule simulation.
- src/scenario-transfer.js: strict, versioned input interchange.
- src/storage.js: validated in-browser persistence.
- src/format.js: locale-aware presentation formatting.
- src/app.js, src/consumer-ui.js and src/planner-ui.js: DOM and behavior only.
- src/i18n.js and src/consumer-i18n.js: bilingual user terminology.
- sw.js and src/pwa.js: scope-limited offline caching.
- tests/: Node.js built-in test runner; public or synthetic inputs only.

## Required verification for a code change

- npm run check
- npm test
- Manual check in desktop and mobile browsers for UI changes
- Offline revisit if changing service-worker asset paths
- Browser accessibility / keyboard checks if changing controls
- Update docs/METHODOLOGY.md for changed equations and units

GitHub Actions passing is useful but cannot replace browser tests. Document validation that was **actually performed**, not intended or inferred.

## Code quality standards

Prefer small functions, validated numeric ranges, stable record schemas, no runtime dependencies and readable documentation. Avoid needless fallback branches that hide erroneous data. Always distinguish measured data from illustrative values. Never imply browser battery APIs can expose health/cycle counts universally.

## Native engineering desktop (developer preview)

The native Windows app lives under `desktop/`. Keep the browser interface and native C# engineering model distinct, with explicit units and separate tests.

- `PackFit.Engineering.Core` must remain .NET-only, with no WinUI dependency, so calculations can be tested headlessly.
- `PackFit.Engineering.Checks` is an independent regression executable. Run `dotnet run --project desktop/PackFit.Engineering.Checks/PackFit.Engineering.Checks.csproj -c Release`.
- `PackFit.Engineering.Desktop` is a WinUI 3 frontend. Event handlers should only orchestrate local file pickers and core methods; run expensive log analyses off the UI thread.
- CSV telemetry is reviewed using **user-supplied** thresholds, never hidden OEM or generalized "safe" limits. Voltage and current unit/sign conventions must be explicit.
- Tests and examples must be invented/public, never derived from private OEM, supplier or workplace measurements.
- Avoid writing raw Windows battery reports or log files to unprotected persistent storage; generated temp files should be cleaned up.
- Do not publish source results or a GitHub Action artifact as a verified signed executable until the Windows build is green and the EXE has been manually smoke-tested on Windows.
- Keep WinUI source in C# / XAML. Do not substitute a WebView wrapper while describing it as native.
- Follow `desktop/README.md` and `.github/workflows/desktop-winui.yml` for build and distribution instructions.



## Current WinUI workflow and security boundaries

- Use official WinUI 3 `MicaBackdrop`, `TitleBar`, `NavigationView`, `InfoBar`, and platform theme resources; do not impersonate Fluent using web styles or copy proprietary art.
- Native Windows diagnostics must remain **allowlisted, read-only** and explicitly initiated by the user. Avoid `cmd /c`, PowerShell injection, arbitrary user-defined subprocess arguments, and silent elevation.
- WPR/WPA/Procmon are optional official Microsoft utilities, linked via documentation; do not download executables or install packages without explicit user action.
- New CSV formats must include unambiguous units, real test vectors and regression tests; never silently infer unknown log signal units.
- For any native XAML updates, verify event-handler names and named controls, run C# core checks, build on `windows-latest`, and manually smoke-test a published EXE.
- Generated diagnostic outputs or engineering handoffs may contain confidential device and employee information. All data stays local; never send it to GitHub Actions, external APIs or public issues.
