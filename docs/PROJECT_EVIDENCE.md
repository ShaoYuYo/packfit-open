# Real-world evidence and adoption log

**Purpose:** preserve honest, verifiable evidence of community impact. This is not a marketing scoreboard.

## Public repository snapshot (2026-11-02)

- **4 GitHub Stars**, **0 forks**, and **no independent PRs** at the time of verification. A Star is not proof of usage; no conversion from stars to users is claimed.
- **2 published prereleases**: [beta.1](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.1) and [beta.2](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.2). Both have downloadable Windows EXE, ZIP and SHA-256 files, subject to their build provenance; they are not proof of physical-device compatibility.
- **3 open maintainer-raised issues** addressing browser QA, battery report variants and native Windows QA; these are **not** external bug reports.
- Windows and browser CI have passed on recent commits. CI does **not** replace first-hand device testing.
- The maintainer has started a [15-minute trial](TRY_PACKFIT.md) and an issue template to collect verified external feedback. **There are not yet published independent trial reports.**

## Implementation milestones (source-code claims only)

- 2026-11-01, v0.1: public runtime and pack feasibility calculators, bilingual interface, methodology and unit tests.
- 2026-11-01, v0.2: local Windows batteryreport import, everyday charging estimates and responsive interface.
- 2026-11-01, v0.3: daily usage planner, scoped offline caching, portable validated JSON scenarios, open example data and onboarding improvements.

- 2026-11-02, first public beta: [v0.7.0-beta.1](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.1) is a **real published GitHub Prerelease** with Windows ZIP, single-file EXE and SHA256 file. The Windows release workflow passed browser tests, pure C# engineering checks and WinUI publication. Real hardware launch testing and external adoption remain unverified.

These entries describe code changes; **they do not assert external adoption, downloads, users, or community contributions**.

## Evidence still needed

| Signal | Verification path | What can currently be claimed |
| --- | --- | --- |
| Stars and forks | GitHub repository | Must verify live counts |
| Unique clones and traffic | GitHub Insights / Traffic | Not yet documented |
| Independent contributors | PRs and contributors list | Not yet documented |
| Independent bug reports | Public issues with reproducible evidence | Not yet documented |
| Downstream real use | Cited sites, learning materials or consented feedback | Not yet documented |
| Release maintenance | Tags, Actions results, release notes | First public beta is published; subsequent releases and long-term maintenance remain to be demonstrated |
| Independent formula review | External engineer or reproducible public test cases | Not yet documented |

## Ethical project growth

- Do not buy or automate GitHub Stars, forks or traffic.
- Do not invent testimonials, community activity, code reviews or adoption.
- Do not count AI-generated commits as independent community contributions.
- Never expose employer, vendor or customer confidential battery data in fixtures or issues.
- Protect contributors and user's private Windows battery reports.

## Practical next steps

1. Test on multiple real laptops and browsers; record anonymous, consented feedback.
2. Add public synthetic fixtures for unsupported battery report formats.
3. Track reproducible issues and actually resolve them.
4. Request independent reviews of equations, boundary handling and accessibility.
5. Publish versioned releases after CI and hands-on browser checks.
6. Update this file only when new evidence can be verified.

Official Codex for Open Source application: https://openai.com/form/codex-for-oss/ . A newly created repository is not automatically eligible.

## Battery report reliability progress (2026-11-02)

A real parser risk was found: Windows capacity-history tables could be confused with current battery data. The native Windows parser now scopes to the installed-battery section and separates supported multiple batteries. Synthetic regression tests cover history, localization, malformed units and two-battery reports. See [issue #2](https://github.com/ShaoYuYo/packfit-open/issues/2) for remaining real-device and format validation. This is **maintenance work**, not evidence of independent adoption.


The beta.2 release was built only after browser tests, C# engineering tests, native WinUI packaging and checksum generation passed. [Release workflow](https://github.com/ShaoYuYo/packfit-open/actions/workflows/publish-beta2.yml). As of this update there are no independent user testimonials or external PRs to claim.
