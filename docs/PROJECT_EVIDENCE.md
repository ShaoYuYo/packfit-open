# PackFit Open — verified project evidence

This log separates **maintainer activity**, **automated verification**, **on-device observations** and **independent adoption**. It is not a claim of OpenAI acceptance, downstream deployments, professional certification or audited business impact.

## Public snapshot — 2026-11-02

Source: [GitHub repository](https://github.com/ShaoYuYo/packfit-open) and GitHub public Releases API at the time of inspection.

| Signal | Observed evidence | What it does *not* establish |
| --- | --- | --- |
| Repository | Public, MIT-licensed, created 2026-11-01 | Long-term maturity or ecosystem adoption |
| GitHub Stars | **4** | Four active users, downloads or independent endorsements |
| Forks | **0** | Independent integrations |
| External pull requests | **None observed** at this snapshot | A community of maintainers |
| Issues | **4** publicly tracked, opened by the maintainer | Reports from unrelated users |
| Native prereleases | **5** tagged prereleases through v0.8.0-beta.1 | Five stable or independently tested versions |
| Release asset counts | Available from public GitHub Releases; low counts include unknown and potentially repeat downloaders | Unique users or installed systems |

Releases: [beta.1](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.1), [beta.2](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.2), [beta.3](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.3), [beta.4](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.4), and [v0.8.0-beta.1](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.8.0-beta.1).

These releases occurred during the project's first two days. The frequency reflects early iterative development **not** mature, long-term OSS maintenance.

## Engineering value demonstrated in source

PackFit combines a public bilingual browser calculator with a local-first native C# / WinUI 3 engineering workspace.

- Windows battery report import with installed-battery scoping, separate physical batteries and an optional historical capacity chart.
- Telemetry CSV/TSV screening with explicit units, user-defined limits, current-sign handling, time integrity checks and reportable findings.
- Battery pack nominal voltage, capacity, Wh, mass and bounding-envelope estimates.
- Validation matrix PASS / FAIL / NOT RUN review and Markdown report handoffs.
- Guided Windows sleep and energy diagnostics, plus synthetic demo data.
- Two independent native UIs: a full XAML interface and a XAML-independent compatibility workspace.

**Important limits:** formulas and examples do not represent OEM acceptance criteria, safety regulations, battery qualification, or measured standby power. No employer/customer/supplier confidential data has been included as a sample.

## Verified automated evidence

- [Web CI](https://github.com/ShaoYuYo/packfit-open/actions/workflows/ci.yml) — JavaScript syntax, unit/regression tests and static assets.
- [Windows CI](https://github.com/ShaoYuYo/packfit-open/actions/workflows/desktop-winui.yml) — .NET engineering test vectors, WinUI self-contained build, main XAML creation, compatibility UI creation, and native demo/chart smoke tests.
- [CodeQL](https://github.com/ShaoYuYo/packfit-open/actions/workflows/codeql.yml) — automated source scanning of JS and C#. **A green workflow is not proof that no security vulnerabilities exist.**
- [v0.8.0-beta.1 release](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.8.0-beta.1) — ZIP, original-name optional EXE and SHA-256 checksum file, published from a passing Windows release workflow.

These are **CI/build facts**, not test certificates from a laboratory or independent domain reviewers.

## New self-maintained source evidence — November 2, 2026

Three maintainer-authored PRs have been merged after the first v0.8 Windows prerelease:

- [#6](https://github.com/ShaoYuYo/packfit-open/pull/6): full-log anomaly truthfulness, bounded local imports and network-independent web runtime.
- [#7](https://github.com/ShaoYuYo/packfit-open/pull/7): native WinUI session provenance, safe sample replacement, searchable finding previews and report warnings.
- [#8](https://github.com/ShaoYuYo/packfit-open/pull/8): same-threshold A/B screening, recorded parameter assumptions, before/after charts, Markdown/JSON outputs and a cross-platform local CLI. [CI](https://github.com/ShaoYuYo/packfit-open/actions/runs/41450896154), [native Windows](https://github.com/ShaoYuYo/packfit-open/actions/runs/41450896156) and [CodeQL](https://github.com/ShaoYuYo/packfit-open/actions/runs/41450896221) passed on the PR head before merge.

The [synthetic two-run trial](CLI_AND_COMPARISON.md) makes it possible for an unrelated engineer to reproduce A/B numbers without private logs. **No such independent trial report is yet claimed.** These source changes have not been retroactively inserted into the published v0.8.0-beta.1 binary.

## Real laptop feedback and outstanding investigation

A Windows user reported a native XAML parsing failure during startup; logs showed App initialization succeeded before MainWindow failed. This prompted [issue #4](https://github.com/ShaoYuYo/packfit-open/issues/4), diagnostics, an independent safe mode and a portable EXE packaging correction. The maintainer subsequently reported that the app could open, which is useful first-hand **maintainer feedback** but is not an independent multi-device acceptance study.

Still unverified: cross-device Windows 10/11 compatibility, accessible keyboard/screen-reader operation, real-world battery-report format diversity, no-false-positive validation thresholds, and independent engineer review of formulas.

## Why the project matters; what remains to prove

Laptop battery diagnostics cross hardware, firmware, OS power management, data logs and practical user questions. PackFit aims to reduce repetitive setup and create reviewable handoffs **without sending private Windows data to an AI model**. This is a plausible niche ecosystem contribution, not yet proven broad adoption.

Before claiming substantial external impact, obtain:

1. At least one voluntary, consented first-hand trial report from someone other than the maintainer, with Windows/browser version and reproducible steps; never publish an unredacted battery report.
2. An independent technical review of battery capacity handling, CSV unit/sign rules or WinUI accessibility.
3. A real PR / issue triage cycle with tests and documented resolution.
4. Follow-up maintenance across weeks/months, not only an intensive initial release day.
5. Measurable downstream reuse or credible anonymized results if contributors willingly report them.

Do not buy stars, fabricate testimonials, count AI-generated commits as independent contributions, infer unique installations from asset download counts, or imply employer sponsorship.

See [maintenance workflow](MAINTAINER_WORKFLOW.md), [engineering acceptance checklist](RELEASE_CHECKLIST.md) and [15-minute trial](TRY_PACKFIT.md).

Official [Codex for Open Source eligibility](https://openai.com/form/codex-for-oss/) prioritizes real usage, ecosystem relevance and active maintenance. This evidence log does **not** assert that the program has approved PackFit.
