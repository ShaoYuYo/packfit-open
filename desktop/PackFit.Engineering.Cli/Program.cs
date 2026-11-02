using System.Globalization;
using System.Text;
using System.Text.Json;
using PackFit.Engineering.Core;

return await Cli.RunAsync(args);

internal static class Cli
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    private static readonly HashSet<string> AllowedOptions = new(StringComparer.Ordinal)
    {
        "--min-temp", "--max-temp", "--min-voltage", "--max-voltage",
        "--max-current", "--max-soc-rate", "--max-gap",
        "--negative-discharge", "--format", "--output", "--help"
    };

    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
            {
                Help();
                return args.Length == 0 ? 2 : 0;
            }
            string command = args[0].ToLowerInvariant();
            if (command is not ("telemetry" or "compare"))
                throw new ArgumentException("Unknown command: " + command);
            var files = new List<string>();
            var options = new Dictionary<string,string>(StringComparer.Ordinal);
            for (int i = 1; i < args.Length; i++)
            {
                string arg = args[i];
                if (!arg.StartsWith("--",StringComparison.Ordinal))
                {
                    files.Add(arg);
                    continue;
                }
                if (!AllowedOptions.Contains(arg))
                    throw new ArgumentException("Unknown option: " + arg);
                if (arg == "--help")
                {
                    Help();
                    return 0;
                }
                if (arg == "--negative-discharge")
                {
                    if (!options.TryAdd(arg,"true"))
                        throw new ArgumentException("Duplicate option: " + arg);
                    continue;
                }
                if (i+1 >= args.Length || args[i+1].StartsWith("--",StringComparison.Ordinal))
                    throw new ArgumentException("Option requires a value: " + arg);
                if (!options.TryAdd(arg,args[++i]))
                    throw new ArgumentException("Duplicate option: " + arg);
            }
            int expected = command == "compare" ? 2 : 1;
            if (files.Count != expected)
                throw new ArgumentException(command + " expects exactly " + expected + " file path(s).");
            string format = options.GetValueOrDefault("--format") ?? "markdown";
            if (format is not ("markdown" or "json"))
                throw new ArgumentException("Format must be markdown or json.");

            double Number(string key, double fallback)
                => options.TryGetValue(key, out string? value) ? ParseNumber(key,value) : fallback;
            double? Optional(string key)
                => options.TryGetValue(key,out string? value) ? ParseNumber(key,value) : null;
            var thresholds = new TelemetryThresholds(
                MinimumTemperatureC: Number("--min-temp",-20),
                MaximumTemperatureC: Number("--max-temp",60),
                MinimumVoltageV: Optional("--min-voltage"),
                MaximumVoltageV: Optional("--max-voltage"),
                MaximumAbsCurrentA: Optional("--max-current"),
                MaxSocRatePctPerMinute: Number("--max-soc-rate",15),
                MaximumGapSeconds: Number("--max-gap",120),
                PositiveCurrentMeansDischarge: !options.ContainsKey("--negative-discharge"));
            thresholds.Validate();

            var analyzed = new List<TelemetryBatchItem>(files.Count);
            foreach (string path in files)
            {
                // Same bounded local reader on Linux, macOS and Windows; no report upload.
                string csv = await LocalDataFiles.ReadTextAsync(path,12_000_000);
                var result = TelemetryAnalyzer.Analyze(csv,thresholds);
                analyzed.Add(new TelemetryBatchItem(SafeName(path),result));
            }

            string output;
            if (command == "compare")
            {
                var compare = TelemetryComparison.Compare(analyzed[0],analyzed[1],thresholds);
                output = format == "json"
                    ? JsonSerializer.Serialize(new
                    {
                        kind = "screening-comparison",
                        appliedThresholds = thresholds,
                        baseline = Snapshot(compare.BaselineSource,compare.Baseline),
                        candidate = Snapshot(compare.CandidateSource,compare.Candidate),
                        findings = compare.Findings.Select(d => new
                        {
                            code = d.Code,
                            baselineCount = d.BaselineCount,
                            candidateCount = d.CandidateCount,
                            deltaCount = d.CountDelta,
                            baselinePerThousandSamples = d.BaselinePerThousandSamples,
                            candidatePerThousandSamples = d.CandidatePerThousandSamples,
                            deltaPerThousandSamples = d.RateDelta
                        }),
                        cautions = compare.Cautions,
                        verdict = "Review only; not a safety, reliability, or release verdict"
                    }, new JsonSerializerOptions { WriteIndented = true })
                    : compare.ToMarkdown();
            }
            else
            {
                var result = analyzed[0];
                output = format == "json"
                    ? JsonSerializer.Serialize(new
                    {
                        kind = "telemetry-screening",
                        source = Snapshot(result.FileName,result.Analysis),
                        thresholds,
                        disclaimer = "Threshold flags are not a root-cause or safety verdict"
                    }, new JsonSerializerOptions { WriteIndented = true })
                    : "# PackFit CLI — local telemetry screening\n\n" +
                      "- File: " + SafeName(result.FileName) + "\n" +
                      "- Private local processing; no network upload\n\n" +
                      EngineeringReport.Build("Local CSV screening","",null,
                          result.Analysis,null,null,thresholds);
            }
            if (options.TryGetValue("--output",out string? destination))
            {
                // CreateNew intentionally refuses to overwrite a CSV or a prior report.
                await using var stream = new FileStream(destination,FileMode.CreateNew,FileAccess.Write,FileShare.None);
                await using var writer = new StreamWriter(stream,new UTF8Encoding(false));
                await writer.WriteAsync(output);
                await writer.FlushAsync();
                Console.Error.WriteLine("Saved report to a new local file.");
            }
            else
                Console.Out.WriteLine(output);
            return 0;
        }
        catch (Exception e) when (e is ArgumentException or FormatException or
            IOException or UnauthorizedAccessException or JsonException)
        {
            Console.Error.WriteLine("PackFit: " + e.Message);
            return 2;
        }
    }

    private static double ParseNumber(string option, string value)
    {
        if (!double.TryParse(value,NumberStyles.Float,Culture,out double number) ||
            !double.IsFinite(number))
            throw new ArgumentException("Invalid finite number for " + option + ".");
        return number;
    }

    private static string SafeName(string path)
    {
        var file = Path.GetFileName(path.Replace('\\','/'));
        return file.Replace("\r"," ").Replace("\n"," ").Replace("|","/")
            .Replace("<","&lt;").Replace(">","&gt;");
    }

    private static object Snapshot(string path, TelemetryAnalysis analysis) => new
    {
        name = SafeName(path),
        samples = analysis.Samples,
        first = analysis.First,
        last = analysis.Last,
        durationSeconds = analysis.Duration.TotalSeconds,
        minimumVoltageV = analysis.MinVoltageV,
        maximumVoltageV = analysis.MaxVoltageV,
        minimumTemperatureC = analysis.MinTemperatureC,
        maximumTemperatureC = analysis.MaxTemperatureC,
        estimatedDischargedWh = analysis.EstimatedDischargedWh,
        totalFindings = analysis.TotalFindings,
        truncatedDetails = analysis.HiddenFindings,
        findingCounts = analysis.FindingCodeCounts
    };

    private static void Help() => Console.Out.WriteLine("""
        PackFit Engineering CLI — offline CSV screening for Linux, macOS and Windows (.NET 10)

        Usage:
          dotnet run --project desktop/PackFit.Engineering.Cli -- telemetry <file.csv> [options]
          dotnet run --project desktop/PackFit.Engineering.Cli -- compare <before.csv> <after.csv> [options]

        Options:
          --min-temp C          Default -20 °C (example only)
          --max-temp C          Default 60 °C (example only)
          --min-voltage V       Optional PACK voltage threshold
          --max-voltage V       Optional PACK voltage threshold
          --max-current A       Optional absolute current threshold
          --max-soc-rate %/min  Default 15
          --max-gap seconds     Default 120
          --negative-discharge  Interpret negative current as discharge
          --format markdown|json
          --output path         Create a NEW local output file; refuses overwrite
          --help                Show this help

        Use synthetic or explicitly authorized files only. Compare requires comparable
        workload, SOC window, sampling and hardware setup; flags are not safety verdicts.
        No account, server, API key or telemetry upload is required.
        """);
}
