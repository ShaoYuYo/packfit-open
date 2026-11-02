# PackFit Open maintainer workflow

PackFit is run as an independent, MIT-licensed open-source project. The maintainer is responsible for release decisions, user privacy, documentation, test interpretation and bug triage. AI tools may help draft changes, but **a model's output or a passing test is not a substitute for engineering review**.

## Intake → reproduce → patch → verify → release

1. **Triage:** Ask for the affected public version, Windows build/browser, steps to reproduce, expected result, actual result and consented *redacted* logs or fully synthetic fixtures. Mark what remains unknown. Do not assume a battery supplier is responsible.
2. **Reproduce:** Prefer small regression cases with explicitly stated units, timestamps, battery count and thresholds. Never copy proprietary cell specifications, system IDs or customer telemetry into the repository.
3. **Prepare a topic branch and PR:** Link the issue, explain the change, list risk and test evidence, and identify whether the change touches the battery math, file import, security, UI or release pipeline.
4. **Run verification:** Browser `npm run check` + `npm test`; Windows `dotnet run --project desktop/PackFit.Engineering.Checks`; WinUI native publish and both normal/compatible UI smoke checks on the Windows GitHub runner.
5. **Human review:** Inspect the source diff, reject misleading thresholds or unsupported safety claims, and ask a domain reviewer for unfamiliar formats or electrical assumptions. Independent peer review is welcome but **not currently demonstrated**.
6. **Merge and document:** Use GitHub PR records, update any changed assumptions, re-run main branch CI and note unresolved limitations.
7. **Release intentionally:** For a Windows prerelease, verify source SHA, ZIP/EXE expected filenames, SHA-256 hashes, unsigned status, WinUI XAML and compatibility smoke tests, and versioned notes. Avoid publishing an additional beta for every documentation-only change.
8. **Respond to feedback:** Record real regressions as issues, identify affected versions and include test-backed fixes. Correct evidence and documentation if previous claims were inaccurate.

## Privacy and safety boundaries

- The web UI runs locally in the browser; native Windows report, CSV and Markdown actions do not upload their data.
- The Microsoft and GitHub documentation links are optional navigation, not automatic software installation.
- Real battery reports can contain usernames, device serials, internal model names and timestamps. Submit only **synthetic or deliberately anonymized** examples.
- Do not run uploaded scripts, commands or PR content without review. No unexpected elevation or bypassing corporate security.
- Battery capacity retention is not a safety verdict. SOC-based standby loss is not a calibrated power-meter measurement. Validation PASS depends on explicitly chosen limits.

## Practical use of Codex / API credits if granted

Use a maintainer-owned, opt-in workflow only for **public repository issues and synthetic fixtures**:

- Summarize and classify reproducible issues by module.
- Draft candidate unit tests for Windows report variations, telemetry parsing and boundary values.
- Review proposed C#/JS diffs for unit conversion, parsing, memory limits, security and translation parity.
- Draft bilingual maintainer notes and release change summaries.
- Require explicit human acceptance before merging, editing public issues, calling APIs or publishing packages.

No API credentials are required to use PackFit. An API integration is **proposed, not currently shipped**. Never send private Windows battery reports, proprietary supplier data or user-local diagnostic logs to OpenAI or another external API without the data owner's authorization.

## What we can honestly claim today

- A core maintainer maintains an OSS repo with browser/Windows build pipelines, unit checks, CodeQL, beta releases, an MIT license, contribution guidelines and tracked issues.
- The public project is **very young**, with few stars, no forks and no independent PR review at the evidence snapshot.
- Maintainer-origin issues, automated commits or downloaded release assets are not proof of broad use.
- OpenAI's Codex for Open Source decision is discretionary; no pass percentage is calculable from these signals.

See [public evidence log](PROJECT_EVIDENCE.md) and [review checklist](RELEASE_CHECKLIST.md).
