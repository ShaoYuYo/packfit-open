using System.Globalization;
using System.Text;

namespace PackFit.Engineering.Core;

/// <summary>Raw event counts and normalized screening rates. This is NOT a pass/fail verdict.</summary>
public sealed record FindingDifference(
    string Code, int BaselineCount, int CandidateCount,
    double BaselinePerThousandSamples, double CandidatePerThousandSamples)
{
    public int CountDelta => CandidateCount - BaselineCount;
    public double RateDelta => CandidatePerThousandSamples - BaselinePerThousandSamples;
}

public sealed record TelemetryComparisonResult(
    string BaselineSource, string CandidateSource,
    TelemetryAnalysis Baseline, TelemetryAnalysis Candidate,
    IReadOnlyList<FindingDifference> Findings,
    IReadOnlyList<string> Cautions)
{
    public TelemetryThresholds? AppliedThresholds { get; init; }

    private static readonly CultureInfo C = CultureInfo.InvariantCulture;
    private static string F(double? value, string format="0.##")
        => value?.ToString(format, C) ?? "N/A";
    private static string Safe(string source)
    {
        // Strip both Windows and Unix path separators even when run on another OS.
        var name = Path.GetFileName(source.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(name)) name = "Unnamed local file";
        string safe = name.Replace("|", "/").Replace("\r", " ").Replace("\n", " ")
            .Replace("<", "&lt;").Replace(">", "&gt;")
            .Replace("[", "&#91;").Replace("]", "&#93;").Trim();
        return safe.Length <= 160 ? safe : safe[..160] + "…";
    }

    public string ToMarkdown()
    {
        var b = new StringBuilder();
        b.AppendLine("## Baseline vs candidate — telemetry screening");
        b.AppendLine();
        b.AppendLine("> **REVIEW ONLY — not a release, root-cause, or safety decision.**");
        b.AppendLine("Candidate minus baseline. The same thresholds are required for both sources.");
        b.AppendLine("Finding rates are counts per 1,000 sample rows, **not** event frequencies per hour.");
        if (AppliedThresholds is { } limits)
        {
            b.AppendLine();
            b.AppendLine("### Recorded screening parameters (applied to both files)");
            b.AppendLine($"- Temperature: {F(limits.MinimumTemperatureC)} to {F(limits.MaximumTemperatureC)} °C");
            b.AppendLine($"- Pack voltage: min {F(limits.MinimumVoltageV)} V; max {F(limits.MaximumVoltageV)} V (N/A = disabled)");
            b.AppendLine($"- Absolute current: {F(limits.MaximumAbsCurrentA)} A (N/A = disabled)");
            b.AppendLine($"- Maximum SOC slope: {F(limits.MaxSocRatePctPerMinute)} %/min; sample gap: {F(limits.MaximumGapSeconds)} seconds");
            b.AppendLine("- Discharge sign: " + (limits.PositiveCurrentMeansDischarge ? "positive current" : "negative current"));
        }
        else b.AppendLine("**Threshold values were not supplied with this comparison; do not assume matching conditions.**");
        b.AppendLine();
        b.AppendLine($"- Baseline: {Safe(BaselineSource)} ({Baseline.Samples:N0} samples, {Baseline.Duration})");
        b.AppendLine($"- Candidate: {Safe(CandidateSource)} ({Candidate.Samples:N0} samples, {Candidate.Duration})");
        b.AppendLine();
        b.AppendLine("| Measurement | Baseline | Candidate | Candidate - baseline |");
        b.AppendLine("|---|---:|---:|---:|");
        Metric("Maximum temperature", Baseline.MaxTemperatureC, Candidate.MaxTemperatureC, "°C");
        Metric("Minimum pack voltage", Baseline.MinVoltageV, Candidate.MinVoltageV, "V");
        Metric("Maximum pack voltage", Baseline.MaxVoltageV, Candidate.MaxVoltageV, "V");
        Metric("Estimated DC discharge energy", Baseline.EstimatedDischargedWh, Candidate.EstimatedDischargedWh, "Wh");
        b.AppendLine();
        b.AppendLine("| Finding code | Baseline | Candidate | Δ count | Baseline / 1k rows | Candidate / 1k rows |");
        b.AppendLine("|---|---:|---:|---:|---:|---:|");
        if (Findings.Count == 0) b.AppendLine("| No screening flags with current limits | 0 | 0 | 0 | 0 | 0 |");
        foreach (var row in Findings)
            b.AppendLine($"| {row.Code} | {row.BaselineCount} | {row.CandidateCount} | {row.CountDelta:+#;-#;0} | {F(row.BaselinePerThousandSamples)} | {F(row.CandidatePerThousandSamples)} |");
        b.AppendLine();
        b.AppendLine("### Comparability checklist — review before interpreting deltas");
        foreach (string caution in Cautions)
            b.AppendLine("- [ ] " + caution);
        return b.ToString();

        void Metric(string name, double? before, double? after, string unit)
        {
            double? delta = before is not null && after is not null ? after - before : null;
            b.AppendLine($"| {name} ({unit}) | {F(before)} | {F(after)} | {F(delta, "+0.##;-0.##;0")} |");
        }
    }
}

/// <summary>
/// Compares the first two selected logs without making an improvement or safety claim.
/// Requires both analyses to have been generated with the SAME thresholds and current sign.
/// </summary>
public static class TelemetryComparison
{
    public static TelemetryComparisonResult Compare(
        TelemetryBatchItem baseline, TelemetryBatchItem candidate,
        TelemetryThresholds? appliedThresholds = null)
    {
        appliedThresholds?.Validate();
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(candidate);
        var before = baseline.Analysis;
        var after = candidate.Analysis;
        if (before is null || after is null || before.Samples <= 0 || after.Samples <= 0)
            throw new ArgumentException("Comparison requires two complete nonempty analyses.");
        var codes = before.FindingCodeCounts.Keys
            .Union(after.FindingCodeCounts.Keys, StringComparer.Ordinal)
            .OrderBy(code => code, StringComparer.Ordinal);
        var differences = codes.Select(code =>
        {
            int left = before.FindingCodeCounts.GetValueOrDefault(code);
            int right = after.FindingCodeCounts.GetValueOrDefault(code);
            return new FindingDifference(code, left, right,
                1000.0 * left / before.Samples, 1000.0 * right / after.Samples);
        }).ToArray();
        var cautions = new List<string>
        {
            "Verify the same device/pack setup, SOC window, workload, firmware, environment and probe placement.",
            "Confirm both CSV logs use the same timestamp meaning, voltage/current units and discharge-current sign.",
            "Screening flags indicate threshold crossings; fewer flags alone do not establish improvement or safety."
        };
        if (appliedThresholds is null)
            cautions.Add("The caller did not provide the thresholds used for both analyses; independently verify exact screening settings.");
        if (RatioBeyond(before.Samples, after.Samples, .25))
            cautions.Add("Sample counts differ by more than 25%; compare row-normalized rates with extra care.");
        var durationLeft = before.Duration.TotalSeconds;
        var durationRight = after.Duration.TotalSeconds;
        if (durationLeft <= 0 || durationRight <= 0)
            cautions.Add("At least one file has zero duration; time-based interpretations are unavailable.");
        else if (RatioBeyond(durationLeft, durationRight, .25))
            cautions.Add("Recording durations differ by more than 25%; the energy totals are not directly comparable.");
        if ((before.MaxTemperatureC is null) != (after.MaxTemperatureC is null))
            cautions.Add("Only one log contains a temperature column; temperature deltas are unavailable.");
        if ((before.MinVoltageV is null) != (after.MinVoltageV is null))
            cautions.Add("Only one log contains a voltage column; voltage deltas are unavailable.");
        if ((before.EstimatedDischargedWh is null) != (after.EstimatedDischargedWh is null))
            cautions.Add("Only one log supports discharge-energy integration; Wh comparison is unavailable.");
        if (before.FindingCodeCounts.ContainsKey("TIME_ORDER") ||
            after.FindingCodeCounts.ContainsKey("TIME_ORDER") ||
            before.FindingCodeCounts.ContainsKey("SAMPLE_GAP") ||
            after.FindingCodeCounts.ContainsKey("SAMPLE_GAP"))
            cautions.Add("Time-order or sampling-gap flags exist; review timing before comparing transient signals.");
        if (before.HiddenFindings > 0 || after.HiddenFindings > 0)
            cautions.Add("Some detailed findings were truncated in the UI, but category totals cover the entire imported logs.");
        return new TelemetryComparisonResult(
            baseline.FileName, candidate.FileName,
            before, after, Array.AsReadOnly(differences), cautions.AsReadOnly())
        {
            AppliedThresholds = appliedThresholds
        };
    }

    private static bool RatioBeyond(double x, double y, double tolerance)
        => Math.Abs(x - y) / Math.Max(x, y) > tolerance;
}
