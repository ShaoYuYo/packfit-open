using System.Globalization;
using System.Text;

namespace PackFit.Engineering.Core;

public sealed record TelemetrySample(
    int SourceRow, DateTimeOffset Timestamp, double? VoltageV,
    double? CurrentA, double? TemperatureC, double? SocPercent);

public sealed record TelemetryThresholds(
    double MinimumTemperatureC = -20, double MaximumTemperatureC = 60,
    double? MinimumVoltageV = null, double? MaximumVoltageV = null,
    double? MaximumAbsCurrentA = null, double MaxSocRatePctPerMinute = 15,
    double MaximumGapSeconds = 120, bool PositiveCurrentMeansDischarge = true)
{
    public void Validate()
    {
        if (!double.IsFinite(MinimumTemperatureC) || !double.IsFinite(MaximumTemperatureC) ||
            MinimumTemperatureC >= MaximumTemperatureC) throw new ArgumentException("Invalid temperature thresholds.");
        if (MinimumVoltageV is not null && (!double.IsFinite(MinimumVoltageV.Value) || MinimumVoltageV < 0))
            throw new ArgumentException("Invalid minimum voltage.");
        if (MaximumVoltageV is not null && (!double.IsFinite(MaximumVoltageV.Value) || MaximumVoltageV <= 0))
            throw new ArgumentException("Invalid maximum voltage.");
        if (MinimumVoltageV is not null && MaximumVoltageV is not null &&
            MinimumVoltageV >= MaximumVoltageV) throw new ArgumentException("Minimum voltage must be lower than maximum.");
        if (MaximumAbsCurrentA is not null && (!double.IsFinite(MaximumAbsCurrentA.Value) || MaximumAbsCurrentA <= 0))
            throw new ArgumentException("Invalid current threshold.");
        if (!double.IsFinite(MaxSocRatePctPerMinute) || MaxSocRatePctPerMinute <= 0 ||
            !double.IsFinite(MaximumGapSeconds) || MaximumGapSeconds <= 0)
            throw new ArgumentException("Invalid rate or time-gap threshold.");
    }
}

public sealed record TelemetryFinding(int Row, string Code, string Severity, string Message);

public sealed record TelemetryAnalysis(
    int Samples, DateTimeOffset First, DateTimeOffset Last,
    double? MinVoltageV, double? MaxVoltageV,
    double? MinTemperatureC, double? MaxTemperatureC,
    double? MinSocPercent, double? MaxSocPercent,
    double? EstimatedDischargedWh, int TotalFindings,
    IReadOnlyList<TelemetryFinding> Findings)
{
    public TimeSpan Duration => Last - First;
    public int HiddenFindings => Math.Max(0, TotalFindings - Findings.Count);
}

public static class TelemetryAnalyzer
{
    private const int MaxLines = 200_000;
    private const int MaxReportFindings = 1000;
    private const int MaxChars = 12_000_000;

    /// <summary>
    /// Canonical headers: Timestamp, Voltage_V or Voltage_mV, Current_A or Current_mA,
    /// Temp_C, SOC_pct. Timestamp must be an ISO-8601 value with timezone.
    /// Positive current is considered discharge unless the caller opts out.
    /// </summary>
    public static TelemetryAnalysis Analyze(string csv, TelemetryThresholds limits)
    {
        limits.Validate();
        if (csv is null || csv.Length > MaxChars)
            throw new FormatException("CSV is missing or larger than 12 MB.");
        using var reader = new StringReader(csv.TrimStart('\uFEFF'));
        var firstLine = reader.ReadLine();
        if (firstLine is null) throw new FormatException("CSV is empty.");
        char delimiter = DetectDelimiter(firstLine);
        var headers = CsvRow(firstLine,delimiter).Select(Normalize).ToList();
        int ti = Select(headers, "timestamp", "datetime", "time", "timestamputc");
        if (ti < 0) throw new FormatException("CSV must contain Timestamp (ISO-8601 with timezone).");
        var v = FindUnit(headers, ("voltagev", 1), ("packvoltagev", 1),
            ("voltagemv", .001), ("packvoltagemv", .001),
            ("batteryvoltagev",1),("battvoltagev",1),
            ("batteryvoltagemv",.001),("battvoltagemv",.001));
        var i = FindUnit(headers, ("currenta", 1), ("packcurrenta", 1),
            ("currentma", .001), ("packcurrentma", .001),
            ("batterycurrenta",1),("battcurrenta",1),
            ("batterycurrentma",.001),("battcurrentma",.001));
        int temp = Select(headers, "tempc", "temperaturec", "packtempc", "batterytempc", "battcelltempc", "celltempc");
        int soc = Select(headers, "soc", "socpct", "socpercent", "stateofcharge", "batterysoc", "batterysocpct", "batterysocpercent");
        if (v.Index < 0 && i.Index < 0 && temp < 0 && soc < 0)
            throw new FormatException("Provide at least one measured signal column.");

        int count=0, totalFindings=0;
        var findings=new List<TelemetryFinding>();
        DateTimeOffset? start=null;
        TelemetrySample? previous=null;
        double? minV=null,maxV=null,minT=null,maxT=null,minSoc=null,maxSoc=null;
        double dischargedWh=0;
        var hasPowerIntegration=false;
        string? line;
        while ((line=reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            count++;
            if (count > MaxLines) throw new FormatException("CSV exceeds 200,000 measurement rows.");
            var cells=CsvRow(line,delimiter);
            if (cells.Count != headers.Count) throw new FormatException($"Row {count+1}: wrong column count (multiline quoted values are not supported).");
            if (!DateTimeOffset.TryParse(cells[ti], CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var when) ||
                !HasTimeZone(cells[ti]))
                throw new FormatException($"Row {count+1}: timestamp needs ISO-8601 with timezone.");
            var current=new TelemetrySample(count+1, when,
                Value(cells,v.Index,v.Factor),Value(cells,i.Index,i.Factor),
                Value(cells,temp,1),Value(cells,soc,1));
            start ??= when;
            void Find(string code,string severity,string message)
            {
                totalFindings++;
                if (findings.Count < MaxReportFindings)
                    findings.Add(new(current.SourceRow,code,severity,message));
            }
            if (current.TemperatureC is double t)
            {
                minT=Math.Min(minT??t,t);maxT=Math.Max(maxT??t,t);
                if (t>limits.MaximumTemperatureC) Find("TEMP_HIGH","Warning",$"{t:F2} °C exceeds {limits.MaximumTemperatureC:F2} °C.");
                if (t<limits.MinimumTemperatureC) Find("TEMP_LOW","Warning",$"{t:F2} °C below {limits.MinimumTemperatureC:F2} °C.");
            }
            if (current.VoltageV is double volts)
            {
                minV=Math.Min(minV??volts,volts);maxV=Math.Max(maxV??volts,volts);
                if (limits.MinimumVoltageV is double low && volts<low)
                    Find("VOLTAGE_LOW","Warning",$"{volts:F3} V below {low:F3} V.");
                if (limits.MaximumVoltageV is double high && volts>high)
                    Find("VOLTAGE_HIGH","Warning",$"{volts:F3} V exceeds {high:F3} V.");
            }
            if (current.CurrentA is double amps && limits.MaximumAbsCurrentA is double maxCurrent
                && Math.Abs(amps)>maxCurrent)
                Find("CURRENT_HIGH","Warning",$"|{amps:F3} A| exceeds {maxCurrent:F3} A.");
            if (current.SocPercent is double pct)
            {
                minSoc=Math.Min(minSoc??pct,pct);maxSoc=Math.Max(maxSoc??pct,pct);
                if (pct<0||pct>100)Find("SOC_RANGE","Error",$"SOC {pct:F2}% is outside 0–100%.");
            }
            if (previous is not null)
            {
                double seconds=(when-previous.Timestamp).TotalSeconds;
                if (seconds<=0)
                    Find("TIME_ORDER","Error","Timestamp is not strictly increasing.");
                else
                {
                    if (seconds>limits.MaximumGapSeconds)
                        Find("SAMPLE_GAP","Info",$"Missing or delayed samples: {seconds:F1} seconds.");
                    if (current.SocPercent is double curSoc && previous.SocPercent is double prevSoc)
                    {
                        double rate=Math.Abs(curSoc-prevSoc)/(seconds/60);
                        if (rate>limits.MaxSocRatePctPerMinute)
                            Find("SOC_JUMP","Warning",$"SOC slope {rate:F2}%/min exceeds {limits.MaxSocRatePctPerMinute:F2}%/min.");
                    }
                    if (seconds <= limits.MaximumGapSeconds &&
                        current.VoltageV is double newV && previous.VoltageV is double oldV &&
                        current.CurrentA is double newA && previous.CurrentA is double oldA)
                    {
                        // Trapezoid integration of DC discharge energy.
                        double avgW=((newV*newA)+(oldV*oldA))/2.0 *
                            (limits.PositiveCurrentMeansDischarge?1:-1);
                        dischargedWh+=Math.Max(0,avgW)*seconds/3600;
                        hasPowerIntegration=true;
                    }
                }
            }
            previous=current;
        }
        if (count==0||previous is null||start is null)
            throw new FormatException("CSV contains no measurement samples.");
        return new(count,start.Value,previous.Timestamp,minV,maxV,minT,maxT,
            minSoc,maxSoc,hasPowerIntegration?dischargedWh:null,totalFindings,findings);
    }

    private static bool HasTimeZone(string text)
        => text.EndsWith("Z",StringComparison.OrdinalIgnoreCase) ||
           System.Text.RegularExpressions.Regex.IsMatch(text,@"[+-]\d{2}:?\d{2}$");

    private static string Normalize(string name) => new(name.ToLowerInvariant()
        .Where(char.IsLetterOrDigit).ToArray());
    private static int Select(List<string> headers,params string[] names)
        => headers.FindIndex(names.Contains);
    private static (int Index,double Factor) FindUnit(
        List<string> headers,params (string Name,double Factor)[] options)
    {
        foreach(var o in options)
        {
            int pos=headers.IndexOf(o.Name);
            if(pos>=0)return(pos,o.Factor);
        }
        return(-1,1);
    }

    private static double? Value(List<string> row,int col,double factor)
    {
        if(col<0||string.IsNullOrWhiteSpace(row[col]))return null;
        if(!double.TryParse(row[col],NumberStyles.Float,CultureInfo.InvariantCulture,out double x)
            ||!double.IsFinite(x))throw new FormatException($"Invalid numeric value at column {col+1}.");
        return x*factor;
    }

    /// <summary>One-line CSV parser supporting commas and escaped quotes; multiline values are rejected.</summary>
    private static char DetectDelimiter(string header)
    {
        int commas=0,tabs=0,semicolons=0;bool quoted=false;
        for(int i=0;i<header.Length;i++)
        {
            char ch=header[i];
            if(ch=='"')
            {
                if(quoted&&i+1<header.Length&&header[i+1]=='"'){i++;continue;}
                quoted=!quoted;
            }
            if(!quoted){if(ch==',')commas++;else if(ch=='\t')tabs++;else if(ch==';')semicolons++;}
        }
        if(tabs>commas&&tabs>=semicolons)return '\t';
        if(semicolons>commas&&semicolons>tabs)return ';';
        return ',';
    }

    private static List<string> CsvRow(string line,char delimiter)
    {
        var fields=new List<string>();
        var buffer=new StringBuilder();
        bool quoted=false;
        for(int n=0;n<line.Length;n++)
        {
            char c=line[n];
            if(c=='"')
            {
                if(quoted && n+1<line.Length && line[n+1]=='"'){buffer.Append('"');n++;}
                else quoted=!quoted;
            }
            else if(c==delimiter&&!quoted){fields.Add(buffer.ToString().Trim());buffer.Clear();}
            else buffer.Append(c);
        }
        if(quoted)throw new FormatException("Unclosed CSV quotation mark.");
        fields.Add(buffer.ToString().Trim());
        return fields;
    }
}
