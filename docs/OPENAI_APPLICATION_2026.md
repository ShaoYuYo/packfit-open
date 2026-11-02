# OpenAI Codex for Open Source — PackFit Open application (2026)

**Official form:** https://openai.com/form/codex-for-oss/  
**Source:** https://developers.openai.com/community/codex-for-oss  
**Status:** Draft for the human project maintainer to review and submit. **Submitting this repository document is NOT submitting an application.**

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

## A. Why does this repository qualify? (497/500 characters)

> I maintain PackFit Open, an MIT-licensed project from my work in laptop battery development. It addresses two needs: understandable battery tools for everyday laptop owners and a WinUI 3 workbench for repetitive engineering tasks. It parses Windows battery reports, screens CSV/TSV logs with explicit limits, tracks validation results, and creates reviewable handoffs. I maintain synthetic regression tests, documented limitations, tracked issues and public beta releases. Adoption is still early.

This emphasizes practical domain value and honest early adoption. If actual stars or users become independently verifiable later, add them **only after checking** the character limit and evidence.

## B. How will you use API credits for your project? (499/500 characters)

> I would use OpenAI API credits for opt-in maintainer automation: triaging public GitHub issues, drafting regression tests for new battery-report and log formats, reviewing PRs for unit mistakes, unsafe file handling and security risks, and improving bilingual documentation. Only synthetic or explicitly authorized public data would be used with APIs. Personal Windows diagnostics and confidential supplier or employer data would remain local. All generated changes would be reviewed before merging.

**Important:** describes intended *future* uses, not features that already send logs to OpenAI. The current project analyzes files locally. Never upload proprietary or identifiable Windows battery, process or diagnostic logs into public AI workflows without authorization.

## C. Anything else we should know? (493/500 characters)

> I'm a battery development engineer, not a software company. I started PackFit because I repeatedly saw both engineers and ordinary laptop owners struggle with battery information and time-consuming diagnostics. AI helped me prototype, test and explain a tool based on real domain knowledge. I'm now concentrating on reliability, reproducible public examples, release discipline and genuine user feedback. I don't treat AI output as a substitute for measurement, code review or safety judgment.

This is where the maintainer's personal motivation appears without overclaiming organization-wide impact or implying an employer endorses the project.

## What has actually changed since the first beta

- The public first beta contains signed-off GitHub Actions build artifacts, **not code-signed binaries**; Windows smoke testing is still outstanding.
- New parser tests cover separate installed batteries, capacity-history scoping and localized headings. Issue [#2](https://github.com/ShaoYuYo/packfit-open/issues/2) tracks unsupported variants and the remaining verification.
- A public [15-minute trial](https://github.com/ShaoYuYo/packfit-open/blob/main/docs/TRY_PACKFIT.md) and feedback template help collect genuine reproducible reports. No independent adoption is claimed.
- The second beta [v0.7.0-beta.2](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.2) is now published with Windows artifacts, verified by its [gated release workflow](https://github.com/ShaoYuYo/packfit-open/actions/workflows/publish-beta2.yml). This remains **automated build evidence**, not actual field validation.

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
