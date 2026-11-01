# PackFit Open

**Battery intelligence, made accessible.** A bilingual, responsive, browser-only battery runtime estimator and battery pack engineering sandbox.

[Open the live app](https://shaoyuyo.github.io/packfit-open/) · [Issues](https://github.com/ShaoYuYo/packfit-open/issues) · [Methodology](docs/METHODOLOGY.md)

> **Project status:** v0.1.0 — initial community release. Mathematical feasibility estimates only; not certified engineering analysis.

## What you can do

| Workspace | Capabilities |
| --- | --- |
| **Overview** | Estimate runtime from battery Wh, average watts, reserve percentage and conversion efficiency; switch example device profiles. |
| **Pack Studio** | Model S/P topology, nominal voltage/energy, total cell + overhead mass, rectangular envelope volume, Wh/kg, Wh/L and ideal runtime. |
| **Compare** | Save up to four scenarios in your browser, compare charts and metrics, load previous inputs, and export UTF-8 BOM CSV for Excel. |
| **Methodology** | Inspect formulas, assumptions, scope and safety limitations. |

### Design principles

- **Zero runtime dependencies, zero API keys, zero telemetry:** plain HTML, CSS and JavaScript modules.
- **Privacy-first:** inputs and saved scenarios stay in local browser storage; no backend or accounts.
- **Accessible by default:** semantic form controls, keyboard navigation, reduced-motion support, mobile layout, dark/light themes.
- **Bilingual:** Traditional Chinese and English.
- **Modular and testable:** pure calculation functions are independent of the interface; node built-in test runner provides regressions.
- **Safety-aware:** advisory messages for certain idealized inputs; avoids presenting estimates as validation or regulatory certification.

## Run

For a quick test, serve the repository root with any HTTP static server. Python 3 users can run:

    python -m http.server 8000

Then open http://localhost:8000 . Opening the HTML via a file URL may block ES modules in some browsers.

To run tests with Node.js 20+ (no npm install required):

    npm test
    npm run check

## Deploy to GitHub Pages

Repository **Settings → Pages → Build and deployment → Deploy from a branch → main → / (root) → Save**. The site will appear at https://shaoyuyo.github.io/packfit-open/ once deployment completes.

## Accuracy & limits

This is a **first-pass parametric estimator**, not a physical battery pack design validator.

- Energy: S × P × nominal cell voltage × cell capacity (Ah)
- Mass: cell count × individual cell mass + explicitly provided pack overhead
- Envelope volume: rectangular X × Y × Z dimensions in mm, divided by 1,000,000 to get liters
- Runtime: available energy divided by **constant average** load; no cell discharge curve, aging, thermal loss or safety cutoff simulation
- No checks for actual cell fit, stack-up, routing, interconnects, BMS, current limits, electrical protection, charging or thermal safety
- Transport considerations depend on jurisdiction and carrier; any displayed advisory is only an awareness prompt

All examples are **illustrative**, not specifications for an actual product or a manufacturer's validated battery. See [full methodology](docs/METHODOLOGY.md).

## Contributing

Pull requests, verified formula corrections, translations and test cases are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) and open an issue describing the intended behavior. Never post confidential employer, supplier or customer information.

## License

MIT © 2026 ShaoYuYo. See [LICENSE](LICENSE).