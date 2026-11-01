using System.Globalization;
using System.Text;

namespace PackFit.Engineering.Core;

public sealed record TelemetryBatchItem(string FileName,TelemetryAnalysis Analysis);
public static class TelemetryBatch
{
    public static string ToMarkdown(IReadOnlyList<TelemetryBatchItem> items)
    {
        if(items.Count==0)return "";
        var b=new StringBuilder();
        b.AppendLine("## Batch telemetry comparison");
        b.AppendLine();
        b.AppendLine("The same user-defined screening thresholds were used for all files.");
        b.AppendLine("Flags are **not** root-cause or safety verdicts.");
        b.AppendLine();
        b.AppendLine("| Source file | Samples | Duration | Min V | Max V | Max °C | Findings | Est. discharge Wh |");
        b.AppendLine("|---|---:|---|---:|---:|---:|---:|---:|");
        foreach(var item in items)
        {
            var a=item.Analysis;
            string n=Path.GetFileName(item.FileName).Replace("|","/").Replace("<","&lt;").Replace(">","&gt;").Replace("\r"," ").Replace("\n"," ");
            string f(double? v)=>v?.ToString("0.###",CultureInfo.InvariantCulture)??"N/A";
            b.AppendLine($"| {n} | {a.Samples} | {a.Duration} | {f(a.MinVoltageV)} | {f(a.MaxVoltageV)} | {f(a.MaxTemperatureC)} | {a.TotalFindings} | {f(a.EstimatedDischargedWh)} |");
        }
        b.AppendLine();
        b.AppendLine("Review each source log separately and confirm timestamp ranges, current sign and units.");
        return b.ToString();
    }
}
