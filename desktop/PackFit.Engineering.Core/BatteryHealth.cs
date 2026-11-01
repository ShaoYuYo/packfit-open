using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace PackFit.Engineering.Core;

public sealed record BatteryHealth(double DesignWh, double FullChargeWh, int? Cycles)
{
    public double RetentionPercent => FullChargeWh / DesignWh * 100.0;
}

public static partial class BatteryReportParser
{
    /// <summary>Reads a common Windows powercfg HTML report as untrusted text. Multiple batteries are not merged.</summary>
    public static BatteryHealth Parse(string report)
    {
        if (report is null || report.Length > 5_000_000)
            throw new FormatException("Report is empty or exceeds 5 MB.");
        var matches = RowRegex().Matches(report);
        double? design = null, full = null;
        int? cycles = null;
        int fullRows = 0;
        foreach (Match row in matches)
        {
            var cells = CellRegex().Matches(row.Groups[1].Value);
            if (cells.Count < 2) continue;
            string key = Text(cells[0].Groups[1].Value).Trim().TrimEnd(':', '：').ToUpperInvariant();
            string val = Text(cells[1].Groups[1].Value);
            if (Is(key, "DESIGN CAPACITY", "設計容量", "设计容量"))
                design ??= Energy(val);
            if (Is(key, "FULL CHARGE CAPACITY", "完整充電容量", "完全充電容量", "完全充电容量"))
            {
                fullRows++;
                full ??= Energy(val);
            }
            if (Is(key, "CYCLE COUNT", "循環次數", "循环次数"))
            {
                var number = Regex.Match(val, @"\d[\d,]*");
                if (cycles is null && number.Success && int.TryParse(
                    number.Value.Replace(",", ""), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var n)) cycles = n;
            }
        }
        if (design is null || full is null || design <= 0 || full <= 0)
            throw new FormatException("The report must contain valid design and full-charge capacities.");
        if (fullRows > 1)
            throw new FormatException("Multiple batteries detected; this version does not merge battery reports.");
        return new(design.Value, full.Value, cycles);
    }

    private static bool Is(string value, params string[] expected)
        => expected.Any(x => value.Equals(x, StringComparison.OrdinalIgnoreCase));
    private static string Text(string html)
        => WebUtility.HtmlDecode(Regex.Replace(Regex.Replace(html,
             @"(?is)<script\b.*?</script>", " "), @"<[^>]+>", " ")).Trim();
    private static double? Energy(string value)
    {
        var match = Regex.Match(value, @"([\d][\d\s,.]*)\s*(mWh|Wh)\b", RegexOptions.IgnoreCase);
        if (!match.Success) return null;
        string raw = Regex.Replace(match.Groups[1].Value, @"[\s,]", "");
        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var n) || n <= 0)
            return null;
        return match.Groups[2].Value.Equals("mWh", StringComparison.OrdinalIgnoreCase) ? n / 1000 : n;
    }

    [GeneratedRegex(@"(?is)<tr\b[^>]*>(.*?)</tr\s*>", RegexOptions.CultureInvariant)]
    private static partial Regex RowRegex();
    [GeneratedRegex(@"(?is)<t[dh]\b[^>]*>(.*?)</t[dh]\s*>", RegexOptions.CultureInvariant)]
    private static partial Regex CellRegex();
}
