# Contributing to PackFit Open

Thanks for contributing. Battery calculations must be **transparent, verifiable, and narrowly scoped**.

## Ground rules

1. Open an issue explaining the user need or bug; small PRs are preferred.
2. Keep electrical/energy calculations in pure modules under src/ without DOM or storage dependencies.
3. Document numeric units, assumptions, rounding behavior and limitations for any new formula.
4. Add or update Node built-in tests for every calculation change and run npm test and npm run check.
5. Preserve keyboard access, mobile layouts, reduced-motion behavior, and zh-TW/en translation parity.
6. Never upload employer/customer/supplier confidential data or personal information.
7. Do not imply estimates certify hardware, charging procedures, transport regulations or safety.

## Architecture

- src/calc.js: validated mathematical core
- src/presets.js: **illustrative** example values
- src/format.js: locale-aware formatting
- src/i18n.js: dictionary and translations
- src/storage.js: versioned, validated localStorage state
- src/csv.js: safe CSV data conversion/export
- src/app.js: DOM interaction and presentation
- styles.css: responsive themes and design system
- tests/: pure JavaScript regression tests

No runtime framework or dependencies are needed.

## Development

Requires Node.js 20 or newer for tests. Serve project root over HTTP for ES modules. Follow the README.

Please use a clear PR description: what changed, why, how tested, and which assumptions were affected.

## v0.3 contribution entry points

- For new numerical calculators: add a pure function under src/calc.js, src/consumer.js, or src/planner.js; include units, assumptions and reproducible tests.
- For editable scenario data: change src/scenario-transfer.js and add versioned migration tests. Never silently accept unknown or malformed input.
- For UI work: check zh-TW/en strings, keyboard focus, mobile sizing and reduced-motion behavior.
- For offline changes: update sw.js asset list and cache version, then actually verify offline reloading.
- Before substantial changes, review [AGENTS.md](AGENTS.md), [Release Checklist](docs/RELEASE_CHECKLIST.md) and [Community Guidelines](CODE_OF_CONDUCT.md).

Useful issues for contributors include [browser validation](https://github.com/ShaoYuYo/packfit-open/issues/1) and [Windows report parser fixtures](https://github.com/ShaoYuYo/packfit-open/issues/2). Please avoid attaching real battery report files containing identifiers.

## Contributing to the native Windows engineering workbench

The pure engineering library is at `desktop/PackFit.Engineering.Core`; regression vectors are a standalone .NET console project at `desktop/PackFit.Engineering.Checks`. The native WinUI 3 client is in `desktop/PackFit.Engineering.Desktop`.

If you change a formula, parser, local file reader, telemetry flag or generated report:

- Document the input units, intended interpretation, invalid cases and limits.
- Add a synthetic reproducible C# check. For Windows-only behavior, ensure the native GitHub Actions job builds the app and exercises **both** the full XAML and compatible workspace.
- Keep unit conversions, time-zone handling, input size limits and battery count interpretation explicit. A truncated list of flags must never lead to a misleading aggregate diagnosis.
- Prefer a focused branch and Pull Request to a direct main-branch push. Leave honest comments on what was tested and what still needs physical-device verification.

If you change a desktop UI, verify high DPI, narrower widths, keyboard operation, reduced-motion settings and the separate `--safe-ui` view. When uploading screenshots, remove user identifiers and proprietary platform data.

## Real maintenance and community feedback

The [maintainer workflow](docs/MAINTAINER_WORKFLOW.md) explains issue intake, reproduction, PR checks and releases. The [15-minute trial](docs/TRY_PACKFIT.md) can be completed with synthetic data. Please report **actual** errors, supported formats and observations, not only a Star or a generic testimonial.

Project evidence is tracked in [docs/PROJECT_EVIDENCE.md](docs/PROJECT_EVIDENCE.md); no external contributors, users or test results are inferred from CI or GitHub activity.
