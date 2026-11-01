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
