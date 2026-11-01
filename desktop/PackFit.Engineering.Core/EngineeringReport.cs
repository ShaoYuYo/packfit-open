using System.Globalization;
using System.Text;

namespace PackFit.Engineering.Core;

public static class EngineeringReport
{
    private static readonly CultureInfo C=CultureInfo.InvariantCulture;
    private static string F(double? v, string fmt="0.###")
        => v?.ToString(fmt,C) ?? "N/A";
    private static string Safe(string text)
        => text.Replace("\r"," ").Replace("\n"," ").Replace("|","/").Replace("<","&lt;").Replace(">","&gt;").Trim();

    /// <summary>Produces a human-readable, reviewable engineering summary, not a certification.</summary>
    public static string Build(
        string project, string author, BatteryHealth? battery,
        TelemetryAnalysis? telemetry, IReadOnlyList<ValidationItem>? matrix,
        PackResult? pack, TelemetryThresholds? limits)
    {
        var b=new StringBuilder();
        b.AppendLine("# PackFit Engineering — analysis summary");
        b.AppendLine();
        b.AppendLine($"- Project: {Safe(project)}");
        b.AppendLine($"- Prepared by: {Safe(author)}");
        b.AppendLine($"- Generated (UTC): {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}Z");
        b.AppendLine("- Status: Draft — review all inputs, thresholds and interpretations");
        b.AppendLine("- Data handling: Local-only; no report upload");
        b.AppendLine();
        if (battery is not null)
        {
            b.AppendLine("## Windows battery capacity snapshot");
            b.AppendLine($"- Design: {F(battery.DesignWh)} Wh");
            b.AppendLine($"- Full charge: {F(battery.FullChargeWh)} Wh");
            b.AppendLine($"- Capacity retention: {F(battery.RetentionPercent,"0.0")}%");
            b.AppendLine($"- Cycle count: {(battery.Cycles?.ToString(C) ?? "Not reported")}");
            b.AppendLine("- Limitation: capacity retention is not a safety, life or health certification.");
            b.AppendLine();
        }
        if (pack is not null)
        {
            b.AppendLine("## Pack estimate");
            b.AppendLine($"- Topology: {pack.CellCount} total cells");
            b.AppendLine($"- Nominal voltage/capacity: {F(pack.NominalVoltageV)} V / {F(pack.CapacityAh)} Ah");
            b.AppendLine($"- Nominal energy: {F(pack.EnergyWh)} Wh");
            b.AppendLine($"- Pack mass: {F(pack.TotalMassG)} g; envelope: {F(pack.VolumeL)} L");
            b.AppendLine($"- Energy density: {F(pack.GravimetricWhKg)} Wh/kg; {F(pack.VolumetricWhL)} Wh/L");
            b.AppendLine($"- Ideal runtime: {F(pack.IdealRuntimeHours)} h (constant load, no reserve).");
            b.AppendLine("- Limitation: nominal calculations do not validate mechanical fit or cell/BMS safety.");
            b.AppendLine();
        }
        if (telemetry is not null)
        {
            b.AppendLine("## Telemetry screening");
            b.AppendLine($"- Samples: {telemetry.Samples}; duration: {telemetry.Duration}");
            b.AppendLine($"- Pack voltage: {F(telemetry.MinVoltageV)} to {F(telemetry.MaxVoltageV)} V");
            b.AppendLine($"- Temperature: {F(telemetry.MinTemperatureC)} to {F(telemetry.MaxTemperatureC)} °C");
            b.AppendLine($"- SOC: {F(telemetry.MinSocPercent)}% to {F(telemetry.MaxSocPercent)}%");
            b.AppendLine($"- Integrated discharge energy (where available): {F(telemetry.EstimatedDischargedWh)} Wh");
            b.AppendLine($"- Flagged findings: {telemetry.TotalFindings} (details shown: {telemetry.Findings.Count})");
            if (limits is not null)
            {
                b.AppendLine($"- Thresholds: temperature {F(limits.MinimumTemperatureC)}–{F(limits.MaximumTemperatureC)} °C;");
                b.AppendLine($"  voltage {F(limits.MinimumVoltageV)}–{F(limits.MaximumVoltageV)} V (N/A means disabled);");
                b.AppendLine($"  |current| max {F(limits.MaximumAbsCurrentA)} A (N/A means disabled), SOC slope {F(limits.MaxSocRatePctPerMinute)} %/min.");
                b.AppendLine("- Thresholds are user-selected screening limits, not universal battery safety limits.");
            }
            b.AppendLine();
            b.AppendLine("| Row | Severity | Code | Finding |");
            b.AppendLine("|---:|---|---|---|");
            foreach (var item in telemetry.Findings)
                b.AppendLine($"| {item.Row} | {Safe(item.Severity)} | {Safe(item.Code)} | {Safe(item.Message)} |");
            if (telemetry.HiddenFindings>0)
                b.AppendLine($"\n*{telemetry.HiddenFindings} additional flags omitted from this Markdown view.*");
            b.AppendLine();
        }
        if (matrix is not null)
        {
            var totals=ValidationMatrix.Summarize(matrix);
            b.AppendLine("## Validation matrix");
            b.AppendLine($"- Total {totals.Total}; PASS {totals.Passed}; FAIL {totals.Failed}; NOT RUN {totals.NotRun}");
            b.AppendLine($"- Pass rate among completed: {F(totals.PassRatePercent,"0.0")}%");
            b.AppendLine();
            b.AppendLine("| Test ID | Category | Metric | Value | Min | Max | Unit | Verdict | Owner | Notes |");
            b.AppendLine("|---|---|---|---:|---:|---:|---|---|---|---|");
            foreach(var t in matrix)
                b.AppendLine($"| {Safe(t.TestId)} | {Safe(t.Category)} | {Safe(t.Metric)} | {F(t.Measured)} | {F(t.Minimum)} | {F(t.Maximum)} | {Safe(t.Unit)} | **{t.Verdict}** | {Safe(t.Owner)} | {Safe(t.Notes)} |");
            b.AppendLine();
        }
        b.AppendLine("## Reviewer notes");
        b.AppendLine("- [ ] Confirm input units, sign conventions, clock offsets and data provenance.");
        b.AppendLine("- [ ] Confirm limits against the actual cell, pack, system and test plan.");
        b.AppendLine("- [ ] Review outliers with raw logs and reproduce any observed anomaly.");
        b.AppendLine("- [ ] Obtain independent sign-off before design, safety or release decisions.");
        b.AppendLine();
        b.AppendLine("*PackFit Open is an educational engineering assistant; this is not a compliance certificate.*");
        return b.ToString();
    }
}
