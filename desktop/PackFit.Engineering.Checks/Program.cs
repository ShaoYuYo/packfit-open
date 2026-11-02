using PackFit.Engineering.Core;

static void Assert(bool condition, string description)
{
    if (!condition) throw new Exception("FAIL: " + description);
    Console.WriteLine("PASS: " + description);
}

static bool Close(double value, double expected, double tolerance = .000001)
    => Math.Abs(value-expected)<tolerance;

var pack=PackCalculator.Calculate(new PackDesign(3,1,3.85,5,57,65,240,70,6.8,16));
Assert(Close(pack.EnergyWh,57.75),"3S1P nominal energy");
Assert(Close(pack.TotalMassG,236),"whole pack mass including overhead");
Assert(Close(pack.VolumetricWhL,57.75/(240*70*6.8/1_000_000.0)),"rectangular envelope Wh/L");
Assert(Close(pack.IdealRuntimeHours,57.75/16),"nominal constant-load runtime");

string html="<table><tr><td>DESIGN CAPACITY</td><td>56,000 mWh</td></tr>" +
    "<tr><td>FULL CHARGE CAPACITY</td><td>49,000 mWh</td></tr>" +
    "<tr><td>CYCLE COUNT</td><td>234</td></tr></table>";
var battery=BatteryReportParser.Parse(html);
Assert(Close(battery.RetentionPercent,87.5),"capacity retention");
Assert(battery.Cycles==234,"cycle count");
try
{
    BatteryReportParser.Parse(html.Replace("</table>",
        "<tr><td>FULL CHARGE CAPACITY</td><td>48,000 mWh</td></tr></table>"));
    throw new Exception("FAIL: multi battery rows must be rejected");
}
catch(FormatException){Console.WriteLine("PASS: multiple battery data rejected");}

// A realistic Windows report has capacity history after Installed batteries.
// Old parsers could mix these unrelated tables and create false extra records.
string installed="<h2><span>Installed batteries</span></h2><table>" +
    "<tr><td>NAME</td><td>Battery A</td></tr>" +
    "<tr><td>DESIGN CAPACITY</td><td>56,000 mWh</td></tr>" +
    "<tr><td>FULL CHARGE CAPACITY</td><td>49,000 mWh</td></tr>" +
    "<tr><td>CYCLE COUNT</td><td>234</td></tr></table>";
string history="<h2>Battery capacity history</h2><table>" +
    "<tr><td>FULL CHARGE CAPACITY</td><td>40,000 mWh</td></tr></table>";
var scoped=BatteryReportParser.ParseAll("<html><body>"+installed+history+"</body></html>");
Assert(scoped.Count==1 && Close(scoped[0].RetentionPercent,87.5),
    "capacity history ignored; Installed batteries section is authoritative");
string second="<table><tr><td>NAME</td><td>Battery B</td></tr>" +
    "<tr><td>DESIGN CAPACITY</td><td>30,000 mWh</td></tr>" +
    "<tr><td>FULL CHARGE CAPACITY</td><td>27,000 mWh</td></tr></table>";
var multi=BatteryReportParser.ParseAll("<h2>Installed batteries</h2>"+
    installed.Substring(installed.IndexOf("<table>",StringComparison.Ordinal))+second+history);
Assert(multi.Count==2 && Close(multi[0].RetentionPercent,87.5) &&
    Close(multi[1].RetentionPercent,90),
    "two physical batteries remain distinct without combining Wh");
try
{
    BatteryReportParser.Parse("<h2>Installed batteries</h2>"+
        installed.Substring(installed.IndexOf("<table>",StringComparison.Ordinal))+second+history);
    throw new Exception("FAIL: single-battery compatibility API must reject multi-battery reports");
}
catch(FormatException){Console.WriteLine("PASS: ambiguous multi-battery single-record read rejected");}
string localized="<h2>已安裝電池</h2><table>" +
    "<tr><td>設計容量</td><td>62,000 mWh</td></tr>" +
    "<tr><td>完整充電容量</td><td>55,000 mWh</td></tr></table>";
Assert(Close(BatteryReportParser.Parse(localized).DesignWh,62),
    "localized Chinese installed battery labels supported");
try
{
    BatteryReportParser.ParseAll("<h2>Installed batteries</h2><table>" +
        "<tr><td>DESIGN CAPACITY</td><td>Unknown</td></tr>" +
        "<tr><td>FULL CHARGE CAPACITY</td><td>48,000 mWh</td></tr></table>");
    throw new Exception("FAIL: unknown capacity units must not be silently accepted");
}
catch(FormatException){Console.WriteLine("PASS: unsupported capacity formats rejected");}

string telemetry="Timestamp,Voltage_V,Current_A,Temp_C,SOC_pct\n"+
    "2026-11-01T00:00:00Z,11.5,2,35,80\n"+
    "2026-11-01T00:00:10Z,11.4,2.1,66,77\n"+
    "2026-11-01T00:05:10Z,9.0,2.4,64,50\n";
var limits=new TelemetryThresholds(MinimumVoltageV:10,MaximumVoltageV:13,
    MaximumAbsCurrentA:3);
var analysis=TelemetryAnalyzer.Analyze(telemetry,limits);
Assert(analysis.Samples==3,"CSV rows parsed");
Assert(analysis.TotalFindings>=4,"temperature, voltage, SOC and sample gaps identified");
Assert(analysis.Findings.Any(x=>x.Code=="TEMP_HIGH"),"over-temperature flag");
Assert(analysis.Findings.Any(x=>x.Code=="VOLTAGE_LOW"),"under-voltage flag");
Assert(analysis.Findings.Any(x=>x.Code=="SAMPLE_GAP"),"time gap flag");
Assert(analysis.EstimatedDischargedWh>0,"trapezoid DC discharge Wh");

string matrix="TestID,Category,Metric,Measured,Min,Max,Unit,Owner,Notes\n"+
    "TC01,Thermal,Surface temp,49,,60,C,Test,example\n"+
    "TC02,Electrical,Voltage,14,,13.2,V,EE,example\n"+
    "TC03,System,Sleep drop,,0,3,%,System,pending\n";
var cases=ValidationMatrix.Parse(matrix);
var summary=ValidationMatrix.Summarize(cases);
Assert(summary.Total==3 && summary.Passed==1 && summary.Failed==1 &&
    summary.NotRun==1,"PASS FAIL NOT RUN counts");
Assert(Close(summary.PassRatePercent??0,50),"pass rate among finished tests");

string report=EngineeringReport.Build("Public synthetic sample","Test Engineer",
    battery,analysis,cases,pack,limits);
Assert(report.Contains("TEMP_HIGH")&&report.Contains("Validation matrix"),
    "auditable report includes findings and validation");
Assert(report.Contains("not a compliance certificate"),"explicit report safety disclaimer");

var sleepPlan=InvestigationGuides.Find("sleep-drain");
Assert(sleepPlan.Steps.Count>=4,"guided sleep-drain investigation covers actionable evidence");
Assert(InvestigationGuides.ToMarkdown(sleepPlan,"No company data").Contains("- [ ]"),
    "investigation handoff provides reviewable checkboxes");

string batchMarkdown=TelemetryBatch.ToMarkdown([
    new TelemetryBatchItem("synthetic-a.csv",analysis),
    new TelemetryBatchItem("synthetic-b.csv",analysis)
]);
Assert(batchMarkdown.Contains("synthetic-a.csv")&&batchMarkdown.Contains("synthetic-b.csv"),
    "batch comparison includes both source files");
Assert(batchMarkdown.Contains("same user-defined screening thresholds"),
    "batch report warns about common threshold assumptions");

string tabSeparated="Timestamp\tBattVoltage_mV\tBattCurrent_mA\tBatteryTemp_C\tBatterySOC_pct\n"+
    "2026-11-01T00:00:00Z\t11500\t2000\t33\t80\n"+
    "2026-11-01T00:00:10Z\t11400\t2100\t35\t79\n";
var tab=TelemetryAnalyzer.Analyze(tabSeparated,new TelemetryThresholds());
Assert(tab.Samples==2 && Math.Abs((tab.MinVoltageV??0)-11.4)<.00001,
    "TSV and mV/mA vendor columns auto-converted without Excel");

string semi="Timestamp;Voltage_V;Current_A;Temp_C;SOC_pct\n"+
    "2026-11-01T00:00:00Z;12;1;32;80\n"+
    "2026-11-01T00:00:10Z;11.9;1;32;79\n";
Assert(TelemetryAnalyzer.Analyze(semi,new TelemetryThresholds()).Samples==2,
    "semicolon-separated logs imported with no manual delimiter conversion");

Console.WriteLine("All PackFit engineering core checks passed.");
