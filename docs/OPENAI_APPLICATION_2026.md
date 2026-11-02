# OpenAI Codex for Open Source — PackFit Open application (2026)

**Official form:** https://openai.com/form/codex-for-oss/  
**Source:** https://developers.openai.com/community/codex-for-oss  
**Status:** Draft for the human project maintainer to review and submit. **Submitting this repository document is NOT submitting an application.**

**Identity and submission note (corrected 2026-11-02):** The repository's independent software maintainer and the battery engineer who provided needs-discovery feedback are different people. Never describe the maintainer as a battery development engineer unless that is independently true. This document is a **revised reference draft only**; changing it does not modify an already-submitted OpenAI form. If an earlier form made an inaccurate identity claim, the applicant should clarify it through an official follow-up rather than reusing the claim.

This application is about a genuine engineering problem, the practical usefulness of an MIT-licensed public project, and responsible use of AI. We do **not** claim unverified stars, download counts, review activity from outside contributors, external users, or an official OpenAI endorsement.

## Every form field (verified 2026-11-02)

| Field | What to enter |
| --- | --- |
| First name * | **YOUR LEGAL OR CONTACT FIRST NAME** — complete yourself |
| Last name * | **YOUR LEGAL OR CONTACT LAST NAME** — complete yourself |
| Email * | **THE EMAIL ASSOCIATED WITH YOUR CHATGPT ACCOUNT** — do not assume it equals the public GitHub email |
| GitHub username * | `ShaoYuYo` (profile must be public) |
| GitHub repository URL * | `https://github.com/ShaoYuYo/packfit-open` (public) |
| Describe your role * | **Primary maintainer** |
| Why does this repository qualify? * | Paste text A below (maximum 500 characters) |
| I'm interested in... | Consider **API credits for my project** if you genuinely plan to use them for OSS maintenance. **Codex Security** is optional; request it only if the project could benefit from that review. |
| OpenAI Organization ID * | Obtain your own `org-...` identifier at https://platform.openai.com/settings/organization/general ; **do not put a GitHub ID here** |
| How will you use API credits for your project? * | Paste text B below (maximum 500 characters) |
| Anything else we should know? | Paste text C below (maximum 500 characters) |
| Terms | Read and accept the official program terms when you personally submit |

## A. Why does this repository qualify? (490/500 characters)

> I maintain PackFit Open, an MIT project for laptop battery tools. It includes a bilingual browser app and WinUI 3 workbench for battery reports, CSV/TSV logs, validation and dimension-first cell layouts. Engineering workflows were shaped by discussions with a battery engineer; that is needs discovery, not product endorsement. I maintain synthetic regression tests, public CI and beta builds without confidential data. Adoption is early; independent testers and reviewers are still needed.

This emphasizes practical domain value and honest early adoption. If actual stars or users become independently verifiable later, add them **only after checking** the character limit and evidence.

## B. How will you use API credits for your project? (499/500 characters)

> I would use OpenAI API credits for opt-in maintainer automation: triaging public GitHub issues, drafting regression tests for new battery-report and log formats, reviewing PRs for unit mistakes, unsafe file handling and security risks, and improving bilingual documentation. Only synthetic or explicitly authorized public data would be used with APIs. Personal Windows diagnostics and confidential supplier or employer data would remain local. All generated changes would be reviewed before merging.

**Important:** describes intended *future* uses, not features that already send logs to OpenAI. The current project analyzes files locally. Never upload proprietary or identifiable Windows battery, process or diagnostic logs into public AI workflows without authorization.

## C. Anything else we should know? (489/500 characters)

> I'm an independent developer, not a battery design engineer or a company. PackFit grew from battery questions and discussions with a battery engineer, including how to compare cells when only an L/W/H envelope is provided. AI accelerated coding and documentation, but I check changes with synthetic fixtures, native Windows builds, Linux CLI tests and documented limitations. I seek independent testers and engineering reviewers; I do not claim widespread use or certified battery designs.

This is where the maintainer's personal motivation appears without overclaiming organization-wide impact or implying an employer endorses the project.

## Current project evidence (updated 2026-11-02)

- [v0.8.0-beta.1](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.8.0-beta.1) is a **real, unsigned Windows prerelease** with self-contained ZIP, original-filename optional EXE and checksum file. CI exercised main and compatibility UI native graphics; this is **not** a physical-device certification or a signed binary.
- Issue [#4](https://github.com/ShaoYuYo/packfit-open/issues/4) documents the early XAML launch failure and work toward a functional safe mode. A maintainer-reported successful launch is not proof of independent adoption.
- Issue [#5](https://github.com/ShaoYuYo/packfit-open/issues/5) and [PR #6](https://github.com/ShaoYuYo/packfit-open/pull/6) document a discovered telemetry aggregation flaw, bounded local import and removal of optional third-party executable JavaScript. This is **maintainer-origin maintenance**, not external review.
- Subsequent merged [PR #7](https://github.com/ShaoYuYo/packfit-open/pull/7) and [PR #8](https://github.com/ShaoYuYo/packfit-open/pull/8) add provenance-safe WinUI workflows plus reproducible local A/B telemetry comparisons and an offline Linux/macOS/Windows CLI. [Linux](https://github.com/ShaoYuYo/packfit-open/actions/runs/41450896154), [Windows](https://github.com/ShaoYuYo/packfit-open/actions/runs/41450896156) and [CodeQL](https://github.com/ShaoYuYo/packfit-open/actions/runs/41450896221) passed. Neither PR proves external usage.
- The verified [evidence log](PROJECT_EVIDENCE.md) lists 4 Stars and no forks/external PRs at its snapshot; these numbers should never be converted into invented user or installation counts.
- [Maintainer workflow](MAINTAINER_WORKFLOW.md) describes proposed opt-in Codex issue/PR analysis, human review and privacy safeguards. API-based automation is **not currently shipped**.

**Submitting a new commit or revising this document does not amend any form previously sent to OpenAI.** Use new facts only if the program requests additional information.

## Factual context and links

- Public repository: https://github.com/ShaoYuYo/packfit-open
- Everyday web app: https://shaoyuyo.github.io/packfit-open/
- Native Windows engineering desktop: https://github.com/ShaoYuYo/packfit-open/blob/main/desktop/README.md
- Beta releases: https://github.com/ShaoYuYo/packfit-open/releases
- Open methodology: https://github.com/ShaoYuYo/packfit-open/blob/main/docs/METHODOLOGY.md
- Public tests and Windows CI: https://github.com/ShaoYuYo/packfit-open/actions
- Real adoption claims and caveats: https://github.com/ShaoYuYo/packfit-open/blob/main/docs/PROJECT_EVIDENCE.md

## Eligibility considerations

OpenAI says that active maintainers may apply, and its review weighs actual usage, ecosystem importance and sustained maintenance, not a published minimum Stars threshold. The project is still at the initial beta stage; this is honest to say. Release management should be verified through a real published release; CI compilation should not be described as Windows field validation. Acceptance is not guaranteed.

If the applicant's employer has confidentiality or external software publication rules, comply with those policies and omit private internal design data and proprietary metrics. A publicly owned repo does not imply employer endorsement.

## After submitting: confirmation email and review timing

The official form says OpenAI **reviews applications on a rolling basis and emails selected applicants**. It does **not** promise an immediate receipt email or an email for every rejection. OpenAI Support wrote on 2026-04-29 that several weeks without a reply can occur. A later official support clarification on 2026-10-06 says that seeing a **success confirmation after submitting the form indicates the submission completed successfully**.

- Official form: https://openai.com/form/codex-for-oss/
- OpenAI Support on uncertain waiting periods: https://community.openai.com/t/when-should-i-expect-a-response-to-my-codex-for-oss-application/1379830/2
- OpenAI Support on successful submission screen: https://community.openai.com/t/codex-for-open-source-application-receipt-and-localized-forms/1402329/5

If you submitted on November 1, no email by November 2 is normal and **not proof of rejection or a failed submission**. Check the address provided on the form (including junk/spam). Avoid submitting duplicates just because no email arrived. Continue improving the existing public repository; reviewers can inspect the current source and release records.

## Submission checklist

- [ ] Verify personal contact name, ChatGPT account email and OpenAI Organization ID.
- [ ] Check your GitHub profile and repository are public.
- [ ] Check whether the first beta release has actually been published and contains a Windows binary, or leave it explicitly labeled pending.
- [ ] Review all three 500-character passages for accuracy.
- [ ] Review the program terms and submit at the official form.
- [ ] Watch the registered email for selection or follow-up verification; OpenAI reviews on a rolling basis.
