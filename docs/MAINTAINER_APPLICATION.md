# Codex for Open Source — application readiness

This is an internal planning note for an open-source maintainer, not proof of acceptance.

Official program: https://openai.com/form/codex-for-oss/

OpenAI says it assesses meaningful usage, software ecosystem relevance and ongoing maintainer responsibilities, including issue triage, code review and releases. It does **not** publish a fixed GitHub star threshold.

## Publicly self-reported recipients (2026)

- [SQLit](https://github.com/Maxteabag/sqlit): the creator reported receiving six months of ChatGPT Pro for an SQL TUI with thousands of repository stars. [Self-report](https://www.reddit.com/r/codex/comments/1udnd8s/).
- [Typia](https://github.com/samchon/typia): the creator reported OSS grant support for a widely used TypeScript validation library. [Self-report](https://www.reddit.com/r/codex/comments/1wn1bfp/).
- [Super Productivity](https://github.com/super-productivity/super-productivity): the maintainer reported acceptance in August 2026, together with trouble redeeming the benefit while subscribed. [Community thread](https://community.openai.com/t/codex-for-oss-activation-link-rejects-accepted-maintainers-who-already-subscribe/1393080).
- Discourse team members publicly reported acceptance. [Community thread](https://community.openai.com/t/codex-for-open-source-2026/1376418).

These are independent self-reports, **not an official OpenAI list of approved repositories**.

## PackFit Open checklist

- [x] Public GitHub project and MIT license.
- [x] Bilingual browser application with useful beginner and engineering flows.
- [x] Public formulas, limitations and synthetic sample input.
- [x] Regression tests and CI configuration.
- [x] Contributions, privacy and versioned backup design.
- [x] Verify the first public beta release and CI gate: [`v0.7.0-beta.1`](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.1) is published with Windows artifacts; browser and engineering core checks passed.
- [ ] Re-verify CI and deployment on **future updates** and validate actual WinUI functionality on Windows hardware.
- [ ] Verify end-to-end user experience in real browsers.
- [ ] Collect permissioned, verifiable user feedback or adoption.
- [ ] Demonstrate ongoing issue triage, PR reviews and meaningful releases.

## Application wording to revise **once evidence exists**

> I maintain PackFit Open, a free privacy-first tool for battery runtime planning, Windows battery report interpretation, and engineering feasibility estimates. It offers bilingual interfaces, transparent formulas, offline support and reusable design scenarios. I maintain calculation tests, issue triage, security and release workflows. [Insert only independently verifiable adoption, actual community contributions and cited ecosystem use.]

Do not replace the bracketed section with fabricated Star counts, users or downloads.

**Redemption caveat:** public reports indicate that some accepted maintainers with existing paid ChatGPT subscriptions saw activation issues. Check official terms and actual acceptance email before making any plan or subscription changes.