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
