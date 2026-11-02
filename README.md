# PackFit Open

**Understand a laptop battery. Investigate what went wrong. Spend less time cleaning logs.**

[**Use the web app**](https://shaoyuyo.github.io/packfit-open/) · [**Windows engineering beta**](https://github.com/ShaoYuYo/packfit-open/releases) · [15-minute guided trial](docs/TRY_PACKFIT.md) · [Report a problem](https://github.com/ShaoYuYo/packfit-open/issues/new/choose)

![Web tests](https://github.com/ShaoYuYo/packfit-open/actions/workflows/ci.yml/badge.svg) ![Native Windows build](https://github.com/ShaoYuYo/packfit-open/actions/workflows/desktop-winui.yml/badge.svg) ![License](https://img.shields.io/badge/license-MIT-6b6271)

PackFit is an independent, **MIT-licensed**, bilingual (English / 繁體中文) project maintained by a battery development engineer. There are two products in one repository: a free browser app for people who want practical answers about their laptop batteries, and a native WinUI 3 workbench for repetitive battery validation and Windows troubleshooting tasks.

## Why I built this

Working with laptop batteries, I've learned that a surprisingly large part of engineering is not about deriving a formula. It is checking where a measurement came from, converting mV into V, investigating a suspicious temperature or SOC change, and rewriting the same findings in a report so another engineer can follow them.

People outside engineering have related questions: *How long should my battery last? Is capacity dropping? Why is my computer draining overnight?* I wanted a tool that answers ordinary questions without jargon and gives engineers something they can actually use at work.

AI has helped me turn those ideas into code and documentation faster. I still treat its output as a draft to test and review. **The application doesn't use AI to invent a battery diagnosis**, and it never sends local reports to an AI service.

This project is personal and independent. It is **not endorsed by my employer or any battery supplier**, and none of the included example measurements are proprietary company data.

## Start here

| | **Web: Everyday Battery Tools** | **Windows: Engineering Desktop** |
| --- | --- | --- |
| **Who** | Laptop owners, students, repairers | Battery, power, EE, validation and system engineers |
| **Start** | [Open in browser](https://shaoyuyo.github.io/packfit-open/), no account needed | [Download latest beta](https://github.com/ShaoYuYo/packfit-open/releases) (Windows 10/11 x64) |
| **Tasks** | Estimate runtime and charging cost, read Windows battery capacity reports, compare pack ideas | Screen CSV/TSV logs, check test verdicts, run Windows diagnostics, prepare review reports |
| **Data handling** | Local calculations and file parsing | Files processed locally; no analytics/backend |
| **Scope** | Estimates and education | First-pass screening and handoff, **not** certified validation |

### Web app: an example you can reproduce

Try **56 Wh** of energy and **14 W** of constant load with zero reserve and 100% efficiency. The ideal runtime is **4 hours** (56 ÷ 14). Increase the reserve or reduce efficiency and see how the estimate changes.

The browser app also includes a daily energy budget, charging-cost calculator, Windows battery report import, battery pack series/parallel estimates, scenario comparison, Chinese/English UI, system light/dark mode and offline support for cached assets.

[Web user guide (English)](docs/USER_GUIDE.en.md) · [網站操作說明（繁中）](docs/USER_GUIDE.zh-TW.md) · [Formula definitions](docs/METHODOLOGY.md)

### Windows engineering workbench: an example you can reproduce

1. Download the [initial beta release](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.1). For a first trial, use the **win-x64 ZIP**, extract the whole folder and run \`PackFit.Engineering.exe\`. This is an unsigned developer preview; follow your IT policy.
2. Import the [synthetic Windows battery report](desktop/samples/battery-report-sample.html). It contains **56 Wh** design capacity and **49 Wh** full-charge capacity. The calculated retention is **87.5%**.
3. Import [synthetic telemetry](desktop/samples/telemetry-sample.csv), set project-specific screening limits and review time, voltage, SOC and temperature flags.
4. Import the [synthetic test matrix](desktop/samples/validation-sample.csv). Unmeasured cases should remain **NOT RUN**, not silently counted as passing.
5. Open **Guided Investigation** for a sleep-drain, charging or shutdown checklist, then export the Markdown engineering handoff.

The desktop also offers read-only Windows \`powercfg\` queries, Energy/SleepStudy report creation when supported, batch comparison of up to 12 log files, and official Microsoft tool links for WPR/WPA and Procmon. The application does not automatically install extra software or elevate permissions.

[Complete WinUI desktop guide](desktop/README.md) · [Windows troubleshooting](desktop/docs/TROUBLESHOOTING.md) · [Fluent design](desktop/docs/FLUENT.md)

## What is verified?

- **Open source:** all application and core calculation source is here under MIT.
- **Automated checks:** browser unit tests and an independent C# engineering test runner are run in GitHub Actions. See [web CI](https://github.com/ShaoYuYo/packfit-open/actions/workflows/ci.yml), [Windows build](https://github.com/ShaoYuYo/packfit-open/actions/workflows/desktop-winui.yml), and [first beta release build](https://github.com/ShaoYuYo/packfit-open/actions/workflows/publish-first-beta.yml).
- **Published binary:** [v0.7.0-beta.1](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.1) includes a self-contained WinUI ZIP, single-file EXE and SHA-256 hashes. CI compiled both distributions.
- **Still to verify:** Windows hardware smoke testing across devices, full accessibility review, additional real-world report formats and broad external adoption. Passing CI is **not** proof of hardware compatibility.
- **Not claimed:** production safety approval, calibrated capacity measurement, OEM-specific charging limits, regulatory certification or automated root-cause diagnosis.

[Full evidence log](docs/PROJECT_EVIDENCE.md) · [Known limitations and release checks](docs/RELEASE_CHECKLIST.md) · [Security](SECURITY.md)

## Architecture

\`\`\`text
index.html + src/ + assets/
  Browser experience: batteries, daily power, scenarios, import/export and PWA
desktop/
  PackFit.Engineering.Core/       Independent .NET calculations and parsers
  PackFit.Engineering.Checks/     Reproducible synthetic engineering tests
  PackFit.Engineering.Desktop/    Native Windows C# / WinUI 3
  samples/                        Public synthetic input files
.github/workflows/               Web CI, Windows CI, versioned beta publisher
docs/                             Methodology, guides, quality and evidence
\`\`\`

To run the website locally:

\`\`\`bash
git clone https://github.com/ShaoYuYo/packfit-open.git
cd packfit-open
npm run check
npm test
python -m http.server 8000
\`\`\`

Open http://localhost:8000/ . To build the native app, use a Windows environment and the commands in [desktop/README.md](desktop/README.md).

## Help make it genuinely useful

This is a young project, and real usage matters more than a Star count. The most helpful contribution is a **reproducible bug**, an independently checked formula, a privacy-safe sample file, a translation improvement or a better way to complete an engineering task.

- [Try the 15-minute trial](docs/TRY_PACKFIT.md) and tell us what is confusing.
- [Open a bug or trial feedback issue](https://github.com/ShaoYuYo/packfit-open/issues/new/choose).
- [See how to contribute](CONTRIBUTING.md) and the [development roadmap](docs/ROADMAP.md).
- [Review security and reporting rules](SECURITY.md) before sharing logs.

**Please do not attach unredacted Windows reports, device IDs, serial numbers, supplier data or employer/customer measurements to public issues.** All included sample inputs are synthetic.

Copyright contributors. Released under the [MIT License](LICENSE).
