using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace PackFit.Engineering.Core;

/// <summary>
/// A single physical battery capacity snapshot. Capacity retention is descriptive,
/// not a safety or remaining-useful-life verdict.
/// </summary>
public sealed record BatteryHealth(double DesignWh, double FullChargeWh, int? Cycles)
{
    public double RetentionPercent => FullChargeWh / DesignWh * 100.0;
}

public static partial class BatteryReportParser
{
    private const int MaxHtmlLength = 5_000_000;
    private const int MaxBatteries = 12;

    /// <summary>
    /// For compatibility with code expecting one battery. Explicitly refuses
    /// to merge or silently choose among multiple physical batteries.
    /// </summary>
    public static BatteryHealth Parse(string report)
    {
        var batteries = ParseAll(report);
        return batteries.Count switch
        {
            1 => batteries[0],
            _ => throw new FormatException(
                $"Report contains {batteries.Count} battery records. Use ParseAll to review them separately.")
        };
    }

    /// <summary>
    /// Reads installed-battery capacity snapshots from a common Windows powercfg
    /// HTML report. Only rows inside the Installed batteries section are considered
    /// when that heading exists: history tables must not become phantom batteries.
    /// Repeated NAME groups and separate battery tables are supported. Ambiguous
    /// multi-column layouts are rejected rather than combined.
    /// </summary>
    public static IReadOnlyList<BatteryHealth> ParseAll(string report)
    {
        if (report is null || report.Length == 0 || report.Length > MaxHtmlLength)
            throw new FormatException("Battery report is empty or exceeds 5 MB.");

        string source = InstalledSection(report);
        var batteries = new List<BatteryHealth>();
        foreach (Match table in TableRegex().Matches(source))
        {
            double? design = null, full = null;
            int? cycles = null;
            bool hasAny = false;

            void Complete()
            {
                if (!hasAny) return;
                if (design is null || full is null || design <= 0 || full <= 0)
                    throw new FormatException("Battery capacity record is incomplete.");
                if (batteries.Count >= MaxBatteries)
                    throw new FormatException("Too many battery records.");
                batteries.Add(new(design.Value, full.Value, cycles));
                design = null;
                full = null;
                cycles = null;
                hasAny = false;
            }

            foreach (Match row in RowRegex().Matches(table.Groups[1].Value))
            {
                var cells = CellRegex().Matches(row.Groups[1].Value);
                if (cells.Count < 2) continue;

                string key = Text(cells[0].Groups[1].Value).Trim().TrimEnd(':', '：').ToUpperInvariant();
                string value = Text(cells[1].Groups[1].Value);
                if (Is(key, "NAME", "名稱", "名称"))
                {
                    if (hasAny) Complete();
                    continue;
                }
                if (Is(key, "DESIGN CAPACITY", "設計容量", "设计容量"))
                {
                    if (design is not null) Complete();
                    design = Energy(value) ?? throw new FormatException("Unrecognized design-capacity units.");
                    hasAny = true;
                }
                else if (Is(key, "FULL CHARGE CAPACITY", "完整充電容量",
                    "完全充電容量", "完全充电容量"))
                {
                    if (full is not null) Complete();
                    full = Energy(value) ?? throw new FormatException("Unrecognized full-charge capacity units.");
                    hasAny = true;
                }
                else if (Is(key, "CYCLE COUNT", "循環次數", "循环次数"))
                {
                    var number = Regex.Match(value, @"\d[\d,]*");
                    if (cycles is null && number.Success &&
                        int.TryParse(number.Value.Replace(",", ""),
                            NumberStyles.Integer,CultureInfo.InvariantCulture,out var count))
                        cycles = count;
                }
            }
            Complete();
        }

        if (batteries.Count == 0)
            throw new FormatException(
                "Could not find an installed battery with valid design and full-charge capacities.");
        return batteries.AsReadOnly();
    }

    private static string InstalledSection(string html)
    {
        var heading = Regex.Match(html,
            @"(?is)<h[1-6]\b[^>]*>\s*(?:<[^>]+>\s*)*(?:Installed\s+batteries|已安裝的電池|已安裝電池|安装的电池|已安装的电池)\s*</h[1-6]\s*>");
        if (!heading.Success) return html; // Supports minimal standalone test fixtures.
        int start = heading.Index + heading.Length;
        var next = Regex.Match(html[start..], @"(?is)<h[1-6]\b");
        int length = next.Success ? next.Index : html.Length - start;
        return html.Substring(start,length);
    }

    private static bool Is(string value, params string[] expected)
        => expected.Any(x => value.Equals(x, StringComparison.OrdinalIgnoreCase));

    private static string Text(string html)
        => WebUtility.HtmlDecode(Regex.Replace(
            Regex.Replace(html, @"(?is)<script\b.*?</script>", " "),
            @"<[^>]+>", " ")).Trim();

    private static double? Energy(string value)
    {
        var match = Regex.Match(value, @"([\d][\d\s,.]*)\s*(mWh|Wh)\b", RegexOptions.IgnoreCase);
        if (!match.Success) return null;
        string raw = Regex.Replace(match.Groups[1].Value, @"[\s,]", "");
        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ||
            !double.IsFinite(n) || n <= 0)
            return null;
        return match.Groups[2].Value.Equals("mWh", StringComparison.OrdinalIgnoreCase) ? n / 1000 : n;
    }

    [GeneratedRegex(@"(?is)<table\b[^>]*>(.*?)</table\s*>", RegexOptions.CultureInvariant)]
    private static partial Regex TableRegex();
    [GeneratedRegex(@"(?is)<tr\b[^>]*>(.*?)</tr\s*>", RegexOptions.CultureInvariant)]
    private static partial Regex RowRegex();
    [GeneratedRegex(@"(?is)<t[dh]\b[^>]*>(.*?)</t[dh]\s*>", RegexOptions.CultureInvariant)]
    private static partial Regex CellRegex();
}
