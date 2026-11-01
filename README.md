# PackFit Open

![CI](https://github.com/ShaoYuYo/packfit-open/actions/workflows/ci.yml/badge.svg)

**A battery calculator for everyday questions and early engineering estimates.** Bilingual, privacy-conscious and offline-capable.

[Open the live app](https://shaoyuyo.github.io/packfit-open/) · [Issues](https://github.com/ShaoYuYo/packfit-open/issues) · [Methodology](docs/METHODOLOGY.md)

> **Project status:** v0.5.0 — initial community release. Mathematical feasibility estimates only; not certified engineering analysis.

## What you can do

| Workspace | Capabilities |
| --- | --- |
| **Overview** | Estimate runtime and plan a day of office/video/gaming usage with editable average wattages, current charge and reserve. |
| **My Battery** | Opt-in browser battery status (where supported), local Windows battery report analysis and manual capacity retention. |
| **Everyday Tools** | Charging cost, power-bank rated and deliverable Wh, and rough recharge time. |
| **Pack Studio** | Model S/P topology, nominal voltage/energy, total cell + overhead mass, rectangular envelope volume, Wh/kg, Wh/L and ideal runtime. |
| **Compare** | Save four scenarios, compare metrics, export CSV, and back up or restore validated versioned JSON on another device. |
| **Methodology** | Inspect formulas, assumptions, scope and safety limitations. |

### Design principles

- **Zero runtime dependencies, zero API keys, zero telemetry:** plain HTML, CSS and JavaScript modules.
- **Privacy-first:** inputs and saved scenarios stay in local browser storage; no backend or accounts.
- **Accessible by default:** semantic form controls, keyboard navigation, reduced-motion support, mobile layout, dark/light themes.
- **Bilingual:** Traditional Chinese and English.
- **Modular and testable:** pure calculation functions are independent of the interface; node built-in test runner provides regressions.
- **Safety-aware:** advisory messages for certain idealized inputs; avoids presenting estimates as validation or regulatory certification.

## First-time user guides

- [繁體中文使用指南](docs/USER_GUIDE.zh-TW.md)
- [English Quick Start](docs/USER_GUIDE.en.md)
- [Synthetic, downloadable scenario examples](examples/sample-scenarios.json)
- [Developer module reference](docs/DEVELOPER_API.md)
- [Design, typography and motion decisions](docs/DESIGN_SYSTEM.md)
- [Release quality checklist](docs/RELEASE_CHECKLIST.md)
- [Open-source adoption evidence (only verified claims)](docs/PROJECT_EVIDENCE.md)

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
## Personal Battery Lab (v0.2)

Live monitoring requests the optional browser Battery Status API only when the visitor clicks **Check my battery**. Many browsers do not support it; even where available it typically offers current charge level, charging state and approximate time, **not battery health, design capacity or cycle count**. Some browsers may expose default or rounded values.

On Windows, run `powercfg /batteryreport /output "%USERPROFILE%\Desktop\battery-report.html"`, then import the HTML file in **My Battery**. The static application reads the file locally in your browser, extracts design capacity, full-charge capacity and optionally cycle count, and displays the capacity retention ratio. Files are never uploaded or stored. Reports may contain private device identifiers; do not publish original files in GitHub issues. Manual entry works when reporting is unavailable.

New Everyday Tools apply bounded, transparent formulas to charging electricity costs, portable power bank Wh estimates and approximate charge time. All sample values are illustrative. Charging time uses a stated 15% modeling buffer; power bank mAh-to-Wh conversion uses the **cell-side nominal voltage**, not 5V output, and output efficiency is assumed. These numbers are **not physical measurements or safety certifications**.

The premium design layer is `assets/premium.css` with animations, responsive layouts, theme support and `prefers-reduced-motion` handling. Inspired by general contemporary product design principles; no Apple artwork or UI assets are copied.

## Offline use and portable scenarios (v0.3)

A scoped Service Worker precaches same-origin static modules after the first successful HTTPS visit. Browsers may provide install-to-home-screen options; offline availability and cache persistence depend on the platform. Google Fonts are external, with system font fallbacks. The application contains no analytics or upload endpoint.

Design comparison JSON exports are explicitly versioned (packfit.scenarios v1). Import validates every numeric field and canonicalizes the result; it replaces locally stored scenarios **only after user confirmation**. Files never leave the browser. CSV is read-only spreadsheet export; JSON is the editable interchange format.

Daily energy planning uses simple duration × average power sums; it does not simulate time-varying hardware load, BMS cutoffs, charging and discharge curves or thermal behavior.

For potential Codex for Open Source participation, see [maintainer readiness](docs/MAINTAINER_APPLICATION.md). We have not claimed outside adoption or an approval.

## Visual update (v0.4)

The UI has a more restrained visual hierarchy, rewritten user-facing copy, Inter + Noto Sans TC with system font fallbacks, and short interaction-led motion instead of continuous decorative animation. The interface respects system reduced-motion settings. All functionality from v0.3 is retained. See [design guidelines](docs/DESIGN_SYSTEM.md).

## Warm Graphite design and motion (v0.5)

The v0.5 appearance uses a warm ivory/graphite foundation with a muted plum action color and restrained champagne-brass accents. Both light and dark themes share the same semantic variables; comparison bars are now plum, brass and neutral gray instead of green/blue. The palette overrides are isolated in `assets/atelier.css`.

For optional enhanced transitions, `src/motion-enhance.js` imports the [MIT-licensed Motion](https://github.com/motiondivision/motion) JavaScript library from a **version-pinned jsDelivr URL (13.2.0)**. This is an asynchronous, nonessential CDN request and does not send battery inputs or reports. When offline, blocked or unavailable, the app uses the native Web Animations API. Essential controls, calculations and local storage work without Motion. Operating-system reduced-motion preferences disable these transitions.

See [design decisions](docs/DESIGN_SYSTEM.md) and [dependency/privacy policy](SECURITY.md).
