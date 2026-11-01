# Architecture

PackFit Open is a static, dependency-free ES module application built for GitHub Pages. No runtime server, API keys or analytics are required.

## Modules and responsibility

| Path | Responsibility |
| --- | --- |
| index.html | Accessible semantic HTML, four workspace views, control labels |
| styles.css | Design tokens, dark/light themes, responsive layouts |
| src/app.js | Event binding, DOM rendering, navigation, dynamic comparison charts |
| src/calc.js | Validated, side-effect-free formula implementation |
| src/presets.js | Clearly illustrative sample configurations |
| src/format.js | Locale-aware numbers and times |
| src/i18n.js | Traditional Chinese and English dictionary |
| src/storage.js | Validated, versioned localStorage records |
| src/csv.js | Escaped Excel-compatible CSV export |
| tests/*.test.js | Domain, persistence, translation and DOM-integrity regression coverage |
| .github/workflows/ci.yml | Node.js syntax/test checks on pushes and PRs |

## Data flow

1. User inputs are read from HTML form controls.
2. Pure calc functions validate and return new immutable result records; calculations retain full internal precision.
3. src/app.js formats and presents results. User-provided scenario names are rendered with textContent rather than innerHTML.
4. Scenario inputs (not calculated outputs) can be stored in browser localStorage. Stored input is validated again and recalculated when loaded.
5. Exports regenerate output from validated input, then escape CSV cells and prepend an Excel-compatible UTF-8 BOM.

## Design choices

**No framework:** Immediate GitHub Pages compatibility, fewer supply-chain dependencies, rapid first contributions. If the app grows significantly, adopt build tooling only when the maintenance cost is justified.

**Explicit units:** Avoid implicit cell-vs-pack unit conversion. Values in mAh, V, g, mm, Wh, W, Ah, kg and L are identified in UI and calculation modules.

**Transparent restrictions:** Mathematical validation is not engineering certification. The app does not infer cell fit from envelope dimensions or propose charging control limits.

**Local-only persistence:** Scenarios do not sync between devices. Clear browser site data to delete them.

## Testing strategy

- Unit tests: exact mathematical identities and boundaries.
- Persistence tests: corrupt stored JSON, invalid values, limits and round-trip behavior.
- Integrity tests: HTML IDs referenced in UI code and translation parity.
- CI: syntax checks, Node native tests and static entry-point checks.

Follow-up priorities include browser-level visual tests, more detailed user guidance, more locale coverage, and engineering model refinements with referenced formulas.
## Consumer additions (v0.2)

- `src/consumer.js`: pure bounded everyday models and a nonexecuting Windows report HTML parser.
- `src/consumer-ui.js`: browser-only UI controller, opt-in live Battery API, local file reader, formula rendering.
- `src/consumer-i18n.js`: bilingual beginner labels and explanations.
- `assets/premium.css`: light-first design refinement with graceful reduced motion.


## Portable and offline modules (v0.3)

- src/scenario-transfer.js — format-versioned JSON validation, numeric field whitelisting and canonicalization.
- src/planner.js / src/planner-ui.js — pure energy budget model and bilingual everyday planner UI.
- src/pwa.js, sw.js, manifest.webmanifest — optional service worker registration and scoped static precache, with no remote APIs.
- examples/sample-scenarios.json — completely synthetic test and onboarding data, not customer hardware specifications.

New model and transfer modules are unit-tested by Node.js built-in test runner. Offline and manifest tests verify that each precached file is present; full browser offline behavior must still be verified on actual platforms.
