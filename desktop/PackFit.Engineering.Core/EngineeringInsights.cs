using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace PackFit.Engineering.Core;

/// <summary>Capacity-history observations are historical report samples, not a safety test.</summary>
public sealed record CapacityHistoryPoint(string Period, double FullChargeWh, double? DesignWh);
public static class BatteryHistoryParser
{
    public static IReadOnlyList<CapacityHistoryPoint> Parse(string html)
    {
        if (string.IsNullOrWhiteSpace(html) || html.Length > 5_000_000)
            throw new FormatException("Invalid Windows battery history HTML.");
        var headings = Regex.Matches(html, @"(?is)<h[1-6]\b[^>]*>(.*?)</h[1-6]\s*>");
        string? section = null;
        for (int i = 0; i < headings.Count; i++)
        {
            string title = Plain(headings[i].Groups[1].Value);
            if (!title.Contains("Battery capacity history",StringComparison.OrdinalIgnoreCase) &&
                !title.Contains("電池容量歷程") && !title.Contains("電池容量歷史"))
                continue;
            int start = headings[i].Index + headings[i].Length;
            int end = i+1 < headings.Count ? headings[i+1].Index : html.Length;
            section = html[start..end];
            break;
        }
        if(section is null)return Array.Empty<CapacityHistoryPoint>();
        var results = new List<CapacityHistoryPoint>();
        foreach(Match table in Regex.Matches(section, @"(?is)<table\b[^>]*>(.*?)</table\s*>"))
        {
            foreach(Match row in Regex.Matches(table.Groups[1].Value,@"(?is)<tr\b[^>]*>(.*?)</tr\s*>"))
            {
                var cells = Regex.Matches(row.Groups[1].Value,@"(?is)<t[dh]\b[^>]*>(.*?)</t[dh]\s*>")
                    .Select(m=>Plain(m.Groups[1].Value)).ToArray();
                if(cells.Length != 3)continue; // Ambiguous multi-battery columns are not silently merged.
                if(!LooksLikeDate(cells[0]))continue;
                double? full = ParseEnergy(cells[1]);
                if(full is null)continue;
                double? design = ParseEnergy(cells[2]);
                if(results.Count>=260)break;
                results.Add(new(cells[0],full.Value,design));
            }
        }
        return results.AsReadOnly();
    }
    private static bool LooksLikeDate(string value)
        => Regex.IsMatch(value,@"\b20\d{2}[-/]\d{1,2}\b") &&
           value.Length <= 100;
    private static double? ParseEnergy(string value)
    {
        var m=Regex.Match(value,@"([\d][\d\s,.]*)\s*(mWh|Wh)\b",RegexOptions.IgnoreCase);
        if(!m.Success)return null;
        if(!double.TryParse(Regex.Replace(m.Groups[1].Value,@"[\s,]",""),
            NumberStyles.Float,CultureInfo.InvariantCulture,out double n) ||
            !double.IsFinite(n) || n<=0)return null;
        return m.Groups[2].Value.Equals("mWh",StringComparison.OrdinalIgnoreCase) ? n/1000 : n;
    }
    private static string Plain(string html)
        => WebUtility.HtmlDecode(Regex.Replace(html,@"(?s)<[^>]+>"," ")).Trim();
}

/// <summary>Uses explicitly supplied before/after SOC and elapsed time. No hidden OEM limits.</summary>
public sealed record StandbyDrainResult(double StartSoc, double EndSoc, double DurationHours,
    double LostSocPercent, double LossPercentPerHour, double? LostEnergyWh, double? AveragePowerW);
public static class StandbyDrainEstimator
{
    public static StandbyDrainResult Estimate(double before, double after, double hours, double? capacityWh)
    {
        if(!double.IsFinite(before)||!double.IsFinite(after)||before<0||before>100||after<0||after>100)
            throw new ArgumentOutOfRangeException(nameof(before),"SOC values must be 0–100%.");
        if(!double.IsFinite(hours)||hours<=0||hours>720)
            throw new ArgumentOutOfRangeException(nameof(hours),"Standby interval must be 0–720 hours.");
        if(capacityWh is double wh && (!double.IsFinite(wh)||wh<=0||wh>1000))
            throw new ArgumentOutOfRangeException(nameof(capacityWh),"Capacity Wh must be >0 and <=1000.");
        double loss=before-after;
        if(loss<0)throw new ArgumentException("SOC rose during the interval; cannot estimate a discharge loss.");
        double? energy=capacityWh*loss/100;
        return new(before,after,hours,loss,loss/hours,energy,energy/hours);
    }
}

/// <summary>Explain what evidence to collect next, never automatically assert a root cause.</summary>
public static class EngineeringTriage
{
    public static IReadOnlyList<string> NextSteps(TelemetryAnalysis? telemetry,
        BatteryHealth? battery, bool hasValidation)
    {
        var steps=new List<string>();
        if(battery is not null)
            steps.Add($"Capacity retention {battery.RetentionPercent:0.0}%: compare measurement date, cycle count and workload; retention alone does not diagnose safety.");
        if(telemetry is not null)
        {
            // The UI preview keeps only 1,000 findings. Recommendations must
            // use counts gathered across all rows, including later anomalies.
            var flags=telemetry.FindingCodeCounts.Count>0
                ? telemetry.FindingCodeCounts.Keys.ToHashSet(StringComparer.Ordinal)
                : telemetry.Findings.Select(f=>f.Code).ToHashSet(StringComparer.Ordinal);
            if(flags.Contains("TEMP_HIGH")||flags.Contains("TEMP_LOW"))
                steps.Add("Temperature flags: verify probe placement, ambient conditions, sample timestamps and project-specific thermal specification.");
            if(flags.Contains("VOLTAGE_LOW")||flags.Contains("VOLTAGE_HIGH"))
                steps.Add("Voltage flags: check pack vs cell signal definitions and approved voltage thresholds before escalation.");
            if(flags.Contains("SOC_JUMP"))
                steps.Add("SOC jumps: compare timestamp continuity and gauge/firmware estimation against raw current measurements.");
            if(flags.Contains("SAMPLE_GAP")||flags.Contains("TIME_ORDER"))
                steps.Add("Time integrity: correct log gaps or ordering before comparing transients or integrating energy.");
            if(telemetry.HiddenFindings>0)
                steps.Add("More findings exist beyond the first 1,000 shown; examine full source data.");
            if(telemetry.EstimatedDischargedWh is null)
                steps.Add("Integrated Wh unavailable: confirm voltage/current columns, current sign and logging continuity.");
        }
        if(!hasValidation)steps.Add("No validation matrix imported: map measured evidence to test ID, limits, owner and pending test status.");
        steps.Add("Assign Battery / Power / System / BIOS ownership only after reviewing aligned evidence.");
        return steps.AsReadOnly();
    }
}
