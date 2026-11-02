# PackFit CLI & baseline/candidate telemetry review

**Status:** Source-level feature on the development branch until its PR is reviewed, Windows/Linux CI succeeds and a main-branch merge is completed. It is **not** included in the older v0.8.0-beta.1 download.

PackFit's shared C# engine is now available from a dependency-light `.NET 10` CLI on Windows, Linux and macOS (the native GUI still requires Windows). The CLI does not contact any server, model or API; all inputs are local, user-selected, and limited to 12,000,000 bytes each.

## Reproduce the engineering results

Install the public .NET 10 SDK, clone the repository and run these commands from the repository root:

```bash
# Single-log screening with explicit project-specific PACK voltage threshold.
dotnet run --project desktop/PackFit.Engineering.Cli -- telemetry \
  desktop/samples/telemetry-sample.csv --min-voltage 11

# Before vs after: applies the SAME limits to both synthetic input files.
dotnet run --project desktop/PackFit.Engineering.Cli -- compare \
  desktop/samples/telemetry-sample.csv \
  desktop/samples/telemetry-after-synthetic.csv \
  --min-voltage 11

# JSON for a script or a CI job.
dotnet run --project desktop/PackFit.Engineering.Cli -- compare \
  desktop/samples/telemetry-sample.csv \
  desktop/samples/telemetry-after-synthetic.csv \
  --min-voltage 11 --format json

# Optional user-controlled Markdown output (refuses to overwrite an existing file).
dotnet run --project desktop/PackFit.Engineering.Cli -- telemetry \
  desktop/samples/telemetry-sample.csv --output engineering-review.md
```

Expected fixture observations at `--min-voltage 11` and `--max-temp 60`: the baseline sample triggers **one TEMP_HIGH and one VOLTAGE_LOW**, while the synthetic candidate triggers neither. Both include a sample gap. The total flag counts may still include additional warnings, e.g. SOC slope. Values are only a test fixture, **not battery performance data**.

Use `--help` to see available thresholds, file format, optional current-sign switch and safe output handling.

## Native WinUI A/B workflow

1. Open **量測 CSV 分析** and set the project's screening thresholds (all imported files use one set).
2. Select **批次匯入 CSV / TSV** and import two to twelve locally authorized files.
3. Choose **基準資料 (Before)** and **候選資料 (After)** in the new comparison panel.
4. Press **比較兩份量測**. Review temperature and voltage extrema, full-log finding counts, and rate bars per 1,000 sample rows.
5. In **工程報告**, save the Markdown handoff with the exact before/after sources, changes and checklists.
6. The comparison clears when another batch is imported or the session is reset; changing the selected files invalidates the old result.

The CLI and GUI share the same `TelemetryAnalyzer` and `TelemetryComparison` logic; GUI binary artifacts for this change become available only when Windows CI completes successfully.

## Mathematical interpretation and limitations

`CountDelta = CandidateFlagCount - BaselineFlagCount`.

`NormalizedRate = 1000 × FlagCount / NumberOfSampleRows`.

These are counts of detected flags rather than unique failures, because a single row may trigger multiple flags. Normalization does **not** make different sampling rates, logging intervals, loads, firmware, environment, SOC windows, or battery models equivalent. A before/after difference is not proof of product improvement.

- The compared absolute maximum temperature or minimum voltage is meaningful **only if those signals exist in both inputs**. Missing columns yield N/A, not zero.
- Logs with different row counts (over 25%) or durations (over 25%) receive explicit caution text.
- Time-order and sample-gap anomalies add cautions.
- The analyzer retains detailed previews for up to 1,000 findings, but computes **full-log per-code totals** over all accepted rows. A late abnormal voltage event must still influence the summary.
- Discharge Wh integration requires voltage/current data, monotonic time and an explicitly verified current-sign convention. **Do not equate total Wh across different durations or loads.**
- All thresholds are user-defined screening aids, **not** OEM, IEC, UL or transportation safety criteria. The output cannot certify safety, SVTP compliance, product readiness or root cause.

## Privacy and contribution

Source CSVs and Windows battery reports can reveal host names, internal project identifiers and sensitive measurements. Use the synthetic examples when reporting public bugs. **Never upload unredacted employer, customer or supplier data** to an issue or an AI service. The CLI prints local file basenames, not full paths, in normal report data; you remain responsible for reviewing any report you choose to publish.

Check with:

```bash
dotnet run --project desktop/PackFit.Engineering.Checks --configuration Release
npm run check
npm test
```

The Linux Actions job also runs end-to-end CLI JSON checks, refuses a repeated `--output` destination, and rejects an oversized file. Windows Actions compile the native shell, execute the MainWindow and compatibility-window smoke tests, and verify the A/B view/report integration. These are automated results, not independent usability validation or physical battery measurements.

**Independent reviewers wanted:** API and unit semantics, CSV parsing edge cases, keyboard/screen-reader navigation, and confusing engineering explanations. Report only findings that you actually reproduced with public or authorized redacted samples.
