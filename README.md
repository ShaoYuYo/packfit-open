# PackFit Open

**Understand a laptop battery. Investigate what went wrong. Spend less time cleaning logs.**

[**Use the web app**](https://shaoyuyo.github.io/packfit-open/) · [**Windows engineering beta**](https://github.com/ShaoYuYo/packfit-open/releases) · [15-minute guided trial](docs/TRY_PACKFIT.md) · [Report a problem](https://github.com/ShaoYuYo/packfit-open/issues/new/choose)

![Web tests](https://github.com/ShaoYuYo/packfit-open/actions/workflows/ci.yml/badge.svg) ![Native Windows build](https://github.com/ShaoYuYo/packfit-open/actions/workflows/desktop-winui.yml/badge.svg) ![License](https://img.shields.io/badge/license-MIT-6b6271)

PackFit is an independent, **MIT-licensed**, bilingual (English / 繁體中文) project maintained by an independent developer, with engineering workflow direction informed by conversations with a battery engineer. There are two products in one repository: a free browser app for people who want practical answers about their laptop batteries, and a native WinUI 3 workbench for repetitive battery validation and Windows troubleshooting tasks.

## Why I built this

I started PackFit to make everyday battery information easier to understand and to explore repetitive engineering tasks such as checking measurement provenance, converting units, screening abnormal values and preparing reviewable handoffs. Discussions with a battery engineer have informed specific design problems; these conversations are needs discovery, not evidence of product adoption or professional qualifications.

People outside engineering have related questions: *How long should my battery last? Is capacity dropping? Why is my computer draining overnight?* I wanted a tool that answers ordinary questions without jargon and gives engineers something they can actually use at work.

AI has helped me turn those ideas into code and documentation faster. I still treat its output as a draft to test and review. **The application doesn't use AI to invent a battery diagnosis**, and it never sends local reports to an AI service.

This project is personal and independent. It is **not endorsed by any employer, customer or battery supplier**, and none of the included example measurements are proprietary company data.

## Independent maintenance, privacy and verification

PackFit is an **early-stage** project. Its practical value comes from domain-specific input validation, reproducible examples and report handoffs — not claims of wide adoption. There are no verified downstream deployments or independent PRs to cite yet.

- **Engineering evidence:** [real metrics, regression work and the gaps still open](docs/PROJECT_EVIDENCE.md).
- **How maintenance works:** [issue → reproduction → PR → CI → release](docs/MAINTAINER_WORKFLOW.md).
- **Security and privacy:** [local file handling, unsigned Windows betas and vulnerability reporting](SECURITY.md).
- **Interested in reviewing?** Start with the [15-minute synthetic-data trial](docs/TRY_PACKFIT.md), then [report an observation](https://github.com/ShaoYuYo/packfit-open/issues/new/choose). An independent formula or accessibility review is especially useful.

The browser build uses **system fonts and native browser animations**, not third-party executable animation scripts. It does not transmit local reports to an AI service. The public website itself is delivered through GitHub Pages; following external help links contacts their destinations.

## Start here

| | **Web: Everyday Battery Tools** | **Windows: Engineering Desktop** |
| --- | --- | --- |
| **Who** | Laptop owners, students, repairers | Battery, power, EE, validation and system engineers |
| **Start** | [Open in browser](https://shaoyuyo.github.io/packfit-open/), no account needed | [Download latest beta](https://github.com/ShaoYuYo/packfit-open/releases) (Windows 10/11 x64) |
| **Tasks** | Estimate runtime and charging cost, read Windows battery capacity reports, compare pack ideas | Screen CSV/TSV logs, check test verdicts, run Windows diagnostics, prepare review reports |
| **Data handling** | Local calculations and file parsing | Files processed locally; no analytics/backend |
| **Scope** | Estimates and education | First-pass screening and handoff, **not** certified validation |

### Enter L × W × H directly — native engineering workflow (merged PR #13)

A battery engineer told us that system design often provides **only the battery's allowed length, width and thickness**, without a 3D file she can open. The Windows workbench now starts from these dimensions, an optional second candidate cell, **fixed series S**, parallel range, user-entered side/Z reserves and target Wh. It invokes the same offline placement engine and generates a geometry diagram and traceable engineering report. The advanced JSON/L-shaped editor is optional. You can also **draw excluded XY rectangles directly over a 2D preview**, or enter precise X/Y/width/height coordinates with the keyboard, including a configurable keepout margin. These user-marked areas constrain the actual search, not just the diagram.

This feature was merged in [PR #13](https://github.com/ShaoYuYo/packfit-open/pull/13), following green Linux, native Windows and CodeQL checks. It is available from source but **not** part of the older tagged v0.8.0-beta.1 Windows binary. The outer dimensions do **not** prove an internally unobstructed battery volume; this model does not certify CAD feasibility, tab routing or pack safety. All default showcase values are explicitly fictional.

Read the [dimension-first engineering walkthrough](docs/DIMENSION_FIRST_LAYOUT.md).

### Irregular laptop battery-space layout explorer (merged source)

A real pre-design question from a battery engineer: **how can different pouch-cell outlines and S/P combinations fit a small, irregular allocation, with local Z restrictions and keepout zones?** PackFit now has an experimental **offline geometry-first layout engine** and native Windows candidate diagrams. It screens XY fit, per-zone height, 90° rotation, required gaps and forbidden areas, then compares nominal Wh, assumed Wh/kg, gross-zone Wh/L and a limited Wh-vs-mass Pareto frontier.

[Merged PR #11](https://github.com/ShaoYuYo/packfit-open/pull/11) documents the first source implementation, with Linux, native Windows and CodeQL checks passing before merge. It uses **invented geometry**, does not import CAD yet, does not certify manufacturability/safety, and **does not prove a global optimum**. It is not part of the old v0.8.0-beta.1 binary.

```bash
dotnet run --project desktop/PackFit.Engineering.Cli -- layout \
  desktop/samples/irregular-layout-synthetic.json --format json
```

Read the [reproducible irregular-space geometry method](docs/IRREGULAR_LAYOUT_OPTIMIZER.md) before interpreting density results; `Wh/L` here is nominal energy divided by **gross allowed-zone volume**, not a real finished-pack ED number.

### Engineering A/B comparison and cross-platform CLI (merged source)

PackFit's shared C# engine can compare two battery telemetry logs under the **same user-defined screening limits**. Engineers can inspect voltage and temperature extrema, per-code anomaly deltas, and normalized flags per 1,000 samples — without pretending those results establish safety or root cause.

The native Windows engineering workspace includes interactive baseline/candidate selections and comparison bars; the new **.NET 10 CLI** exposes the same logic on Linux, macOS and Windows, returning Markdown or JSON for local scripts and CI. The feature was merged in [PR #8](https://github.com/ShaoYuYo/packfit-open/pull/8) after Linux CLI, native Windows and CodeQL tests. It is available to source builders, but **not included in the older tagged v0.8.0-beta.1 Windows binary**.

```bash
dotnet run --project desktop/PackFit.Engineering.Cli -- compare \
  desktop/samples/telemetry-sample.csv \
  desktop/samples/telemetry-after-synthetic.csv --min-voltage 11 --format json
```

Read the [reproducible A/B and CLI guide](docs/CLI_AND_COMPARISON.md), including expected synthetic findings and data-handling limits. [Linux CI](https://github.com/ShaoYuYo/packfit-open/actions/workflows/ci.yml) tests this workflow without accessing private logs.

### Web app: an example you can reproduce

Try **56 Wh** of energy and **14 W** of constant load with zero reserve and 100% efficiency. The ideal runtime is **4 hours** (56 ÷ 14). Increase the reserve or reduce efficiency and see how the estimate changes.

The browser app also includes a daily energy budget, charging-cost calculator, Windows battery report import, battery pack series/parallel estimates, scenario comparison, Chinese/English UI, system light/dark mode and offline support for cached assets.

[Web user guide (English)](docs/USER_GUIDE.en.md) · [網站操作說明（繁中）](docs/USER_GUIDE.zh-TW.md) · [Formula definitions](docs/METHODOLOGY.md)

### v0.8 native engineering visuals

The current Windows beta adds functioning **native WinUI charts**, not screenshots or a WebView dashboard. Once v0.8.0-beta.1 is published, use the built-in **載入合成示範資料** button to view:

- Capacity-retention bars and a time-ordered full-charge-capacity history.
- Four separate V / A / °C / SOC charts from a bounded preview of a real CSV/TSV import.
- A live S/P pack topology diagram connected to nominal Wh, voltage and cell count.
- Before/after standby SOC bars with %/hour and optional energy estimates.
- Symptom-first triage, Basic/Engineering navigation, and evidence-backed next steps.

The XAML-independent `--safe-ui` mode also supports charts and synthetic sample workflows. [Visual workflow methods, units and limits](desktop/docs/ENGINEERING_VISUALS.md). Graphs and status indicators do not substitute for test instruments, project-approved thresholds or a safety approval.

### Windows engineering workbench: an example you can reproduce

1. Download the [current beta release](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.8.0-beta.1). For a first trial, use the **win-x64 ZIP**, extract the whole folder and launch `Start-PackFit.cmd` (or use `--safe-ui` for the independent fallback). This is an unsigned developer preview; follow your IT policy.
2. Import the [synthetic Windows battery report](desktop/samples/battery-report-sample.html). It contains **56 Wh** design capacity and **49 Wh** full-charge capacity. The calculated retention is **87.5%**.
3. Import [synthetic telemetry](desktop/samples/telemetry-sample.csv), set project-specific screening limits and review time, voltage, SOC and temperature flags.
4. Import the [synthetic test matrix](desktop/samples/validation-sample.csv). Unmeasured cases should remain **NOT RUN**, not silently counted as passing.
5. Open **Guided Investigation** for a sleep-drain, charging or shutdown checklist, then export the Markdown engineering handoff.

If the app cannot start, try `--safe-ui` and use the local diagnostic script included with beta.3. The desktop also offers read-only Windows `powercfg` queries, Energy/SleepStudy report creation when supported, batch comparison of up to 12 log files, and official Microsoft tool links for WPR/WPA and Procmon. The application does not automatically install extra software or elevate permissions.

[Windows startup troubleshooting（繁體中文）](desktop/docs/STARTUP.zh-TW.md) · [Complete WinUI desktop guide](desktop/README.md) · [Windows troubleshooting](desktop/docs/TROUBLESHOOTING.md) · [Fluent design](desktop/docs/FLUENT.md)

## What is verified?

- **Open source:** all application and core calculation source is here under MIT.
- **Automated checks:** browser unit tests and an independent C# engineering test runner are run in GitHub Actions. See [web CI](https://github.com/ShaoYuYo/packfit-open/actions/workflows/ci.yml), [Windows build](https://github.com/ShaoYuYo/packfit-open/actions/workflows/desktop-winui.yml), and [first beta release build](https://github.com/ShaoYuYo/packfit-open/actions/workflows/publish-first-beta.yml).
- **Published binary:** [v0.8.0-beta.1](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.8.0-beta.1) includes a self-contained WinUI ZIP, single-file EXE and SHA-256 hashes. CI compiled both distributions. Beta.4 preserves the original portable EXE filename and adds an independent, functional XAML-free compatibility workbench. The original user's launch must still be verified on their PC.
- **Still to verify:** Windows hardware smoke testing across devices, full accessibility review, additional real-world report formats and broad external adoption. Passing CI is **not** proof of hardware compatibility.
- **Not claimed:** production safety approval, calibrated capacity measurement, OEM-specific charging limits, regulatory certification or automated root-cause diagnosis.

[Full evidence log](docs/PROJECT_EVIDENCE.md) · [Known limitations and release checks](docs/RELEASE_CHECKLIST.md) · [Security](SECURITY.md)

## Architecture

```text
index.html + src/ + assets/
  Browser experience: batteries, daily power, scenarios, import/export and PWA
desktop/
  PackFit.Engineering.Core/       Independent .NET calculations and parsers
  PackFit.Engineering.Checks/     Reproducible synthetic engineering tests
  PackFit.Engineering.Desktop/    Native Windows C# / WinUI 3
  samples/                        Public synthetic input files
.github/workflows/               Web CI, Windows CI, versioned beta publisher
docs/                             Methodology, guides, quality and evidence
```

To run the website locally:

```bash
git clone https://github.com/ShaoYuYo/packfit-open.git
cd packfit-open
npm run check
npm test
python -m http.server 8000
```

Open http://localhost:8000/ . To build the native app, use a Windows environment and the commands in [desktop/README.md](desktop/README.md).

## Help make it genuinely useful

This is a young project, and real usage matters more than a Star count. The most helpful contribution is a **reproducible bug**, an independently checked formula, a privacy-safe sample file, a translation improvement or a better way to complete an engineering task.

- [Try the 15-minute trial](docs/TRY_PACKFIT.md) and tell us what is confusing.
- [Open a bug or trial feedback issue](https://github.com/ShaoYuYo/packfit-open/issues/new/choose).
- [See how to contribute](CONTRIBUTING.md) and the [development roadmap](docs/ROADMAP.md).
- [Review security and reporting rules](SECURITY.md) before sharing logs.

**Please do not attach unredacted Windows reports, device IDs, serial numbers, supplier data or employer/customer measurements to public issues.** All included sample inputs are synthetic.

Copyright contributors. Released under the [MIT License](LICENSE).
