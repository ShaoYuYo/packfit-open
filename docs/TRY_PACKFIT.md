# Try PackFit in 15 minutes

PackFit is maintained by a battery development engineer. Its browser tools are for everyday questions; its Windows desktop beta is for first-pass diagnosis and validation work.

This is a **real user trial plan**, not a request for stars. Please report what worked, what was confusing, and what failed. Negative feedback is welcome. You do not need a GitHub account to use the web tool, but filing an issue requires a GitHub account.

## Before you start

- **Privacy first:** use the included synthetic files. Battery reports, ETL, event logs and supplier measurements may contain machine names, paths, serial numbers or confidential IP. Do not attach your unredacted personal or company reports to a public issue.
- If using the Windows beta, download from [the verified release page](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.8.0-beta.1). It is unsigned and experimental; obey organization IT policy.
- All scores are *screening*, not diagnostic certification. Never use a PackFit result as the sole basis for charging, thermal, safety, or release decisions.

## Trial A: browser (5 minutes)

1. Open https://shaoyuyo.github.io/packfit-open/.
2. Estimate runtime for a battery of **56 Wh**, average load **14 W**, reserve **0%**, efficiency **100%**. The simple constant-power reference is **4 hours**. Change either setting and confirm that results update.
3. Use the everyday tools to estimate charging cost or battery retention. Check whether units and assumptions are understandable without an EE background.
4. Switch Chinese/English and system/light/dark appearance. Refresh the page and confirm your chosen theme behaves as expected.
5. Navigate the page on a phone if available; check labels, inputs and keyboard usability.

## Trial B: native Windows beta (10 minutes)

1. Download the **win-x64 ZIP**, extract *all* files and launch the native EXE. If it cannot launch, record only Windows edition, CPU architecture and the exact error wording.
2. Open Battery Health. Import [the synthetic HTML report](../desktop/samples/battery-report-sample.html). Expected: design **56 Wh**, full **49 Wh**, **87.5%** capacity retention. The result is an example, not a real battery measurement.
3. Open Telemetry. Import [the synthetic measurement CSV](../desktop/samples/telemetry-sample.csv). Review the temperature warning and any time-gap messages. Do not interpret illustrative limits as OEM requirements.
4. Import [the synthetic validation matrix](../desktop/samples/validation-sample.csv). Expect the display to distinguish PASS, FAIL and NOT RUN. Verify the output is easy to interpret.
5. Open Guided Investigation. Select Sleep Drain; review the checklist and create a Markdown handoff with *fictional* notes.
6. Open Reporting. Confirm imported results are represented without claiming safety/compliance approval.

## Trial C: cross-platform engineering CLI (3–5 minutes, .NET 10 SDK)

This developer trial uses **only public synthetic data** and works on Windows, Linux and macOS. It is available from the `main` source after [PR #8](https://github.com/ShaoYuYo/packfit-open/pull/8) merged; it is not part of the older v0.8 Windows release binary.

1. From the repository root, run `dotnet run --project desktop/PackFit.Engineering.Cli -- compare desktop/samples/telemetry-sample.csv desktop/samples/telemetry-after-synthetic.csv --min-voltage 11 --format json`.
2. Expect a JSON object with `kind: screening-comparison`, where baseline `TEMP_HIGH` count is **1**, candidate `TEMP_HIGH` is **0**, and baseline `VOLTAGE_LOW` is **1** with candidate **0**. Both may contain additional time-gap or SOC flags.
3. Repeat with `--format markdown` and check whether an engineering colleague could understand the cautions without reading the source code.
4. Try `--output review.md` twice: the second run must **refuse to overwrite** the existing report.
5. Report unclear units, malformed/ambiguous errors, incompatibilities or misleading interpretations. Testers are not required to disclose real device data.

See the [full A/B comparison method](CLI_AND_COMPARISON.md) for limitations and runnable examples.

## Tell us what happened

For a useful public issue, please share:

- Product: browser / native Windows beta
- OS and browser versions (general only; do not include device ID)
- What you attempted and what you expected
- What actually happened; exact error if available
- Screenshot with sensitive information removed, or a synthetic minimal reproduction
- How you would describe the tool's purpose to a colleague
- Whether you would use it again, and what would need to change

Use the [feedback template](https://github.com/ShaoYuYo/packfit-open/issues/new?template=trial_feedback.md) or [bug report](https://github.com/ShaoYuYo/packfit-open/issues/new?template=bug_report.md).

**Maintainer follow-through:** legitimate issues should be triaged, reproduced on public fixtures where possible, fixed with tests, and linked to subsequent release notes. Feedback is *not* treated as evidence of independent adoption until it can be verified or the reporter consents to a public attribution.
