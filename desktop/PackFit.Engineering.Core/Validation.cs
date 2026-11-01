using System.Globalization;
using System.Text;

namespace PackFit.Engineering.Core;

public sealed record ValidationItem(
    string TestId, string Category, string Metric, string Unit,
    double? Measured, double? Minimum, double? Maximum,
    string Owner, string Notes)
{
    public string Verdict => Measured is null ? "NOT RUN" :
        ((Minimum is not null && Measured < Minimum) ||
         (Maximum is not null && Measured > Maximum)) ? "FAIL" : "PASS";
}

public sealed record ValidationSummary(int Total, int Passed, int Failed, int NotRun)
{
    public double? CompletionPercent => Total == 0 ? null : 100.0 * (Passed + Failed) / Total;
    public double? PassRatePercent => Passed + Failed == 0 ? null :
        100.0 * Passed / (Passed + Failed);
}

public static class ValidationMatrix
{
    /// <summary>
    /// Input CSV: TestID,Category,Metric,Measured,Min,Max,Unit,Owner,Notes.
    /// Min or Max can be empty; rows without measured values are NOT RUN.
    /// </summary>
    public static IReadOnlyList<ValidationItem> Parse(string csv)
    {
        if (csv is null || csv.Length > 4_000_000)
            throw new FormatException("Validation matrix exceeds 4 MB.");
        var items = new List<ValidationItem>();
        using var reader = new StringReader(csv.TrimStart('\uFEFF'));
        string header = reader.ReadLine() ?? throw new FormatException("Validation CSV is empty.");
        string[] columns = header.Split(',').Select(x => x.Trim()).ToArray();
        string[] names = ["TestID", "Category", "Metric", "Measured", "Min", "Max", "Unit", "Owner", "Notes"];
        var indexes = names.Select(x => Array.FindIndex(columns, c => c.Equals(x,StringComparison.OrdinalIgnoreCase))).ToArray();
        if (indexes.Take(6).Any(x => x < 0))
            throw new FormatException("Required headers: TestID, Category, Metric, Measured, Min, Max.");
        string? line;
        while ((line=reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))continue;
            if (items.Count>=10_000)throw new FormatException("More than 10,000 cases.");
            var cells=ParseRow(line);
            string Col(int index) => index<0 ? "" :
                index>=cells.Count ? throw new FormatException("Missing expected CSV columns.") : cells[index].Trim();
            string id=Col(indexes[0]);
            if (string.IsNullOrWhiteSpace(id) || id.Length>100)
                throw new FormatException("Each case needs a TestID of up to 100 characters.");
            var measured=Number(Col(indexes[3]));
            var min=Number(Col(indexes[4]));
            var max=Number(Col(indexes[5]));
            if (min is null && max is null)
                throw new FormatException($"{id}: specify Min and/or Max.");
            if (min is not null && max is not null && min>max)
                throw new FormatException($"{id}: Min exceeds Max.");
            items.Add(new(id,Col(indexes[1]),Col(indexes[2]),Col(indexes[6]),
                measured,min,max,Col(indexes[7]),Col(indexes[8])));
        }
        return items;
    }

    public static ValidationSummary Summarize(IReadOnlyList<ValidationItem> cases)
        => new(cases.Count, cases.Count(x => x.Verdict=="PASS"),
            cases.Count(x => x.Verdict=="FAIL"),cases.Count(x => x.Verdict=="NOT RUN"));

    private static double? Number(string value)
    {
        if (string.IsNullOrEmpty(value))return null;
        if(!double.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var n)||
            !double.IsFinite(n))throw new FormatException($"Invalid number: {value}");
        return n;
    }

    private static List<string> ParseRow(string line)
    {
        var result=new List<string>();
        var buf=new StringBuilder(); bool quote=false;
        for(int i=0;i<line.Length;i++)
        {
            char c=line[i];
            if(c=='"')
            {
                if(quote && i+1<line.Length && line[i+1]=='"'){buf.Append('"');i++;}
                else quote=!quote;
            }
            else if(c==','&&!quote){result.Add(buf.ToString());buf.Clear();}
            else buf.Append(c);
        }
        if(quote)throw new FormatException("Malformed quoted field; multiline CSV fields unsupported.");
        result.Add(buf.ToString());
        return result;
    }
}
