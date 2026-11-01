# PackFit Open

**Battery questions, answered with practical tools.**  
*A free, privacy-conscious project for everyday laptop users and battery development engineers.*

[Try the web app](https://shaoyuyo.github.io/packfit-open/) · [Windows desktop beta / Releases](https://github.com/ShaoYuYo/packfit-open/releases) · [Desktop guide](desktop/README.md) · [Report an issue](https://github.com/ShaoYuYo/packfit-open/issues) · [Contribute](CONTRIBUTING.md)

![Web CI](https://github.com/ShaoYuYo/packfit-open/actions/workflows/ci.yml/badge.svg) ![Windows desktop CI](https://github.com/ShaoYuYo/packfit-open/actions/workflows/desktop-winui.yml/badge.svg) ![MIT license](https://img.shields.io/badge/license-MIT-neutral)

## Why I started PackFit

I'm a **battery development engineer**. In laptop development, some of the most time-consuming work isn't designing the battery itself: it's repeatedly collecting diagnostic reports, cleaning voltage/current/temperature/SOC logs, comparing test results, checking limits, and writing a handoff that another engineer can actually reproduce.

Outside engineering work, people face simpler but equally real questions: *How long will my laptop last today? Has its battery aged? How much does charging cost? Why does it drain overnight?* Battery information should not require an electrical engineering background to understand.

**PackFit Open connects those two worlds:** a simple browser app for everyday battery decisions, and a native Windows engineering workbench for repetitive diagnostic and validation tasks.

I also believe AI is a powerful **development assistant**, particularly for turning domain experience into working prototypes, tests, accessible explanations, and maintainable open-source code. AI helps me move faster; it does **not** replace checking equations, verifying measurements, reviewing code, or making engineering safety decisions. I want to show how an engineer can use AI responsibly to build something useful for other people.

This project is independent, MIT-licensed, and built with **public or synthetic examples**. It does not represent my employer, a battery supplier, or an official certification body. No confidential design specifications or private product data belong in this repository.

## Two ways to use PackFit

| | Everyday battery tools | Engineering desktop beta |
| --- | --- | --- |
| **For** | Laptop owners, learners, repair enthusiasts | Battery, power, systems and validation engineers |
| **Platform** | Any modern desktop/mobile browser | Windows 10/11 x64 · native C# / WinUI 3 |
| **Start** | [Open the website](https://shaoyuyo.github.io/packfit-open/) · no sign-up | [Download a beta build from Releases](https://github.com/ShaoYuYo/packfit-open/releases) when the release build succeeds |
| **Typical use** | Runtime and charging cost, battery capacity reports, daily energy budget | Batch log screening, test matrices, sleep/wake troubleshooting, native diagnostics, engineering reports |
| **Privacy** | Calculations and file analysis stay local | Local-only processing, explicit file selection, no telemetry or cloud backend |
| **Scope** | Informational estimates | Engineering **screening**, not certified validation |

### Engineering Desktop — what it can do

- **Read Windows battery capacity:** use the built-in \`powercfg\` report or import an HTML report, with a clear distinction between capacity retention and overall battery safety.
- **Process multiple test logs:** import up to 12 supported CSV/TSV/semicolon logs, normalize explicitly labeled mV/mA units, check user-configured voltage, current, temperature, SOC and timing conditions, and compare findings.
- **Organize validation work:** turn measured values and Min/Max thresholds into PASS / FAIL / NOT RUN matrices, rather than maintaining ad hoc spreadsheet formulas.
- **Investigate laptop problems:** follow guided checklists for sleep drain, slow charging, unexpected shutdown, thermal throttling and capacity loss; run selected read-only Windows diagnostics without memorizing commands.
- **Explain optional specialist tools:** find official Microsoft documentation for WPR/WPA and Procmon; never auto-install them.
- **Prepare engineering handoffs:** save reproducible Markdown summaries, the thresholds used, unresolved flags and reviewer checklists.

**Windows EXE status:** This is an initial **beta / developer preview**. The build workflow publishes a self-contained WinUI ZIP, and also attempts a single-file executable. A successful CI build confirms compilation; it **does not prove** that every workflow has been smoke-tested on physical Windows devices. See the [desktop download and build instructions](desktop/README.md), [Fluent design notes](desktop/docs/FLUENT.md), and [troubleshooting guide](desktop/docs/TROUBLESHOOTING.md).

## Project status and maintenance

- **Browser app:** v0.6.1 — browser-based, Chinese/English, available on GitHub Pages.
- **First repository beta:** \`v0.7.0-beta.1\` — intended to document the two-product project and package the Windows workbench, pending an actual successful release build.
- **Maintainer:** [ShaoYuYo](https://github.com/ShaoYuYo), primary maintainer.
- **Engineering integrity:** formulas and input assumptions are documented; source tests, CI configuration and synthetic examples are included.
- **Community adoption:** still early. We do **not** claim users, downloads, independent contributions, certification or operational readiness without evidence.
- **Looking for:** genuine browser test feedback, privacy-safe Windows battery-report fixtures, test vectors, accessibility feedback, code review and bug reports.

[Public calculation methodology](docs/METHODOLOGY.md) · [Maintenance and adoption evidence](docs/PROJECT_EVIDENCE.md) · [Security policy](SECURITY.md) · [Release checklist](docs/RELEASE_CHECKLIST.md)

---

## What you can do

| Workspace | Capabilities |
| --- | --- |
| **Overview** | Estimate runtime and plan a day of office/video/gaming usage with editable average wattages, current charge and reserve. |
| **My Battery** | Opt-in browser battery status (where supported), local Windows battery report analysis and manual capacity retention. |
| **Everyday Tools** | Charging cost, power-bank rated and deliverable Wh, and rough recharge time. |
| **Device Insights** | Automatic browser hardware hints, opt-in WebGL graphics identification and local browser computation probe; not a Windows hardware inventory or a gaming benchmark. |
| **Pack Studio** | Model S/P topology, nominal voltage/energy, total cell + overhead mass, rectangular envelope volume, Wh/kg, Wh/L and ideal runtime. |
| **Compare** | Save four scenarios, compare metrics, export CSV, and back up or restore validated versioned JSON on another device. |
| **Methodology** | Inspect formulas, assumptions, scope and safety limitations. |

### Design principles

- **Dependency-free calculations, zero API keys, zero telemetry:** core modules are plain HTML/CSS/JavaScript; the optional Motion library is loaded separately for visual transitions.
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

## Device Insights and automatic color themes (v0.6)

**System appearance is the default.** PackFit follows the browser/OS light/dark preference using `matchMedia('(prefers-color-scheme: dark)')` and reacts when that preference changes. The Appearance selector provides System, Light and Dark overrides. Explicit new choices are stored locally. Legacy pre-v0.6 theme settings automatically migrate to System, because earlier settings could not distinguish user overrides from defaults.

**Basic browser hardware hints** are displayed when the site loads: `navigator.hardwareConcurrency` (possibly reduced logical processors), `navigator.deviceMemory` (coarse memory where supported), platform and WebGPU API presence. Missing hints stay unknown. The resource profile is a **low-confidence descriptive bracket**, not a CPU/GPU or gaming performance score.

**Detailed GPU identification** only runs if you click Check GPU details; it uses WebGL and may return an anonymized renderer or no data. **Computation probe** runs only after you choose Run local test, in a dedicated Worker and reports elapsed milliseconds for a fixed JavaScript task. Thermal conditions, background work and browsers affect the result. Neither test uploads, stores or fingerprints device information; there is no analytics endpoint.

The seven workspaces have a unified responsive layout in `assets/workspace.css`, with larger readable controls, consistent spacing and reflowing columns. See [User Guide](docs/USER_GUIDE.zh-TW.md) and [Device limitations](docs/DEVICE_INSIGHTS.md).

### Optional full Windows hardware names

The browser cannot read your full hardware inventory. To see a more precise CPU model, installed RAM label, OS and one or more GPU names, open Windows **DxDiag** (Win + R → `dxdiag`) and choose **Save All Information**. Select the resulting `.txt` report in Device Insights. Only these fields are extracted into memory; machine name and other diagnostic details are discarded. The raw report is **not uploaded, tracked or saved**. Do not publish DxDiag reports unredacted, because the original report may contain sensitive device identifiers.
