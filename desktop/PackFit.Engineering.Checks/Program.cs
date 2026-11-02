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


string capacityHistoryHtml="<h2>Installed batteries</h2><table>"+
    "<tr><td>DESIGN CAPACITY</td><td>56,000 mWh</td></tr>"+
    "<tr><td>FULL CHARGE CAPACITY</td><td>49,000 mWh</td></tr></table>"+
    "<h2>Battery capacity history</h2><table>"+
    "<tr><th>Period</th><th>Full charge capacity</th><th>Design capacity</th></tr>"+
    "<tr><td>2026-08</td><td>52,000 mWh</td><td>56,000 mWh</td></tr>"+
    "<tr><td>2026-09</td><td>51,000 mWh</td><td>56,000 mWh</td></tr>"+
    "<tr><td>2026-10</td><td>49,000 mWh</td><td>56,000 mWh</td></tr></table>";
var historyPoints=BatteryHistoryParser.Parse(capacityHistoryHtml);
Assert(historyPoints.Count==3 && Close(historyPoints[0].FullChargeWh,52) &&
    Close(historyPoints[2].FullChargeWh,49),
    "Windows full-charge history is parsed in report order without mixing current batteries");
Assert(BatteryHistoryParser.Parse("<h2>Installed batteries</h2><table></table>").Count==0,
    "Missing battery capacity history yields an empty series, not invented data");

var drainEstimate=StandbyDrainEstimator.Estimate(85,77,8,56);
Assert(Close(drainEstimate.LostSocPercent,8) &&
    Close(drainEstimate.LossPercentPerHour,1) &&
    Close(drainEstimate.LostEnergyWh??0,4.48) &&
    Close(drainEstimate.AveragePowerW??0,.56),
    "Standby SOC loss, hourly rate and user-supplied Wh estimate");
Assert(StandbyDrainEstimator.Estimate(85,77,8,null).AveragePowerW is null,
    "Missing capacity never fabricates average W");
try
{
    StandbyDrainEstimator.Estimate(70,85,8,56);
    throw new Exception("FAIL: rising SOC cannot be called standby drain");
}
catch(ArgumentException){Console.WriteLine("PASS: rising SOC is rejected");}

Assert(analysis.Trace.Count>=2 &&
    analysis.Trace.Count<=321 &&
    analysis.Trace[0].Timestamp < analysis.Trace[^1].Timestamp,
    "Native chart trace is bounded, chronological and includes the final sample");
var nextSteps=EngineeringTriage.NextSteps(analysis,battery,false);
Assert(nextSteps.Any(s=>s.Contains("Temperature flags")) &&
    nextSteps.Any(s=>s.Contains("validation matrix")),
    "Evidence-based triage proposes next checks without claiming an automatic root cause");


var longCsvRows=Enumerable.Range(0,1208).Select(index =>
    DateTimeOffset.Parse("2026-11-01T00:00:00Z")
        .AddSeconds(index).ToString("O",System.Globalization.CultureInfo.InvariantCulture) +
    $",{(index==1207?9.0:11.5):0.0},2.0,{(index<1207?65:34)},80");
var longLog="Timestamp,Voltage_V,Current_A,Temp_C,SOC_pct\n"
    +string.Join("\n",longCsvRows)+"\n";
var longAnalysis=TelemetryAnalyzer.Analyze(longLog,
    new TelemetryThresholds(MaximumTemperatureC:60,MinimumVoltageV:10));
Assert(longAnalysis.TotalFindings>1000 && longAnalysis.HiddenFindings>0,
    "large telemetry logs report truncated detailed findings explicitly");
Assert(!longAnalysis.Findings.Any(f=>f.Code=="VOLTAGE_LOW") &&
    longAnalysis.FindingCodeCounts.TryGetValue("VOLTAGE_LOW",out int lowCount) &&
    lowCount==1 &&
    longAnalysis.FindingCodeCounts.GetValueOrDefault("TEMP_HIGH")>=1200,
    "full-log finding category counts preserve late anomalies beyond the first 1000 flags");
var longNext=EngineeringTriage.NextSteps(longAnalysis,null,true);
Assert(longNext.Any(step=>step.Contains("Voltage flags")),
    "late voltage events still inform next-step triage");
var longReport=EngineeringReport.Build("synthetic","",null,longAnalysis,null,null,
    new TelemetryThresholds(MaximumTemperatureC:60,MinimumVoltageV:10));
Assert(longReport.Contains("VOLTAGE_LOW: 1") &&
    longReport.Contains("Full-log finding counts"),
    "reviewable report includes anomalies suppressed from the detailed table");


string boundedFixture=Path.Combine(Path.GetTempPath(),
    "packfit-local-fixture-"+Guid.NewGuid().ToString("N")+".csv");
try
{
    await File.WriteAllTextAsync(boundedFixture,
        "Timestamp,Voltage_V\n2026-11-01T00:00:00Z,11.5\n",
        new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier:true));
    string imported=await LocalDataFiles.ReadTextAsync(boundedFixture,500);
    Assert(imported.Contains("Voltage_V") && imported.Contains("11.5"),
        "local importer correctly reads BOM-prefixed synthetic CSV within the byte limit");

    await File.WriteAllBytesAsync(boundedFixture,new byte[501]);
    try
    {
        await LocalDataFiles.ReadTextAsync(boundedFixture,500);
        throw new Exception("FAIL: importer accepted oversized local input");
    }
    catch(FormatException){Console.WriteLine("PASS: 501-byte input rejected against 500-byte limit");}
}
finally
{
    if(File.Exists(boundedFixture)) File.Delete(boundedFixture);
}

// Reproducible two-log investigation: same timestamp/window, explicit pack-V limit.
string compareBefore = "Timestamp,Voltage_V,Current_A,Temp_C,SOC_pct\n" +
    "2026-11-01T00:00:00Z,11.5,1,34,80\n" +
    "2026-11-01T00:00:10Z,10.9,1,64,79\n";
string compareAfter = "Timestamp,Voltage_V,Current_A,Temp_C,SOC_pct\n" +
    "2026-11-01T00:00:00Z,11.5,1,34,80\n" +
    "2026-11-01T00:00:10Z,11.1,1,54,79\n";
var compareLimits=new TelemetryThresholds(MinimumVoltageV:11,MaximumTemperatureC:60);
var oldLog=TelemetryAnalyzer.Analyze(compareBefore,compareLimits);
var newLog=TelemetryAnalyzer.Analyze(compareAfter,compareLimits);
var comparison=TelemetryComparison.Compare(
    new TelemetryBatchItem("before.csv",oldLog),
    new TelemetryBatchItem("after.csv",newLog),compareLimits);
var tempDiff=comparison.Findings.Single(x=>x.Code=="TEMP_HIGH");
var voltageDiff=comparison.Findings.Single(x=>x.Code=="VOLTAGE_LOW");
Assert(tempDiff.BaselineCount==1 && tempDiff.CandidateCount==0 &&
    Close(tempDiff.RateDelta,-500),
    "same-limit A/B comparison reports lower temperature flags per thousand rows");
Assert(voltageDiff.CountDelta==-1 && comparison.ToMarkdown().Contains("REVIEW ONLY"),
    "comparison retains exact voltage delta without an automatic Pass/Fail claim");
Assert(comparison.ToMarkdown().Contains("| TEMP_HIGH | 1 | 0 | -1 |") &&
    comparison.ToMarkdown().Contains("Recorded screening parameters") &&
    comparison.ToMarkdown().Contains("min 11 V"),
    "Markdown handoff includes exact threshold assumptions and candidate-minus-baseline counts");

var divergentTimeLog=TelemetryAnalyzer.Analyze("Timestamp,Voltage_V\n" +
    "2026-11-01T00:00:00Z,11.5\n" +
    "2026-11-01T00:10:00Z,11.4\n",compareLimits);
var unequal=TelemetryComparison.Compare(
    new TelemetryBatchItem("before.csv",oldLog),
    new TelemetryBatchItem("after.csv",divergentTimeLog));
Assert(unequal.Cautions.Any(x=>x.Contains("durations differ")) &&
    unequal.Cautions.Any(x=>x.Contains("temperature column")),
    "non-comparable duration and absent temperature are disclosed, never fabricated");

var denseComparison=TelemetryComparison.Compare(
    new TelemetryBatchItem("long-before.csv",longAnalysis),
    new TelemetryBatchItem("long-after.csv",oldLog));
Assert(denseComparison.Findings.Any(x=>x.Code=="VOLTAGE_LOW"&&
    x.BaselineCount==1) && denseComparison.Cautions.Any(x=>x.Contains("truncated")),
    "late anomalies omitted from preview remain visible in full-log A/B comparisons");
Assert(!comparison.ToMarkdown().Contains("C:\\private\\path"),
    "comparison never invents machine paths into the report");


// Irregular-pack use case, with publicly invented cell geometry only.
// Disjoint XY rectangles form an L shape; the raised arm has a different Z limit.
var layoutCase=new LayoutProblem(
    Zones:[
        new LayoutZone(0,0,138,64,6.8),
        new LayoutZone(0,64,75,64,6.2)
    ],
    Keepouts:[new LayoutKeepout(70,75,5,12)],
    Cells:[
        new LayoutCellSpec("SYNTH-LONG",65,55,6,3.85,5,62,false),
        new LayoutCellSpec("SYNTH-COMPACT",42,55,5.5,3.85,3,39,true)
    ],
    Topologies:[
        new LayoutTopology(3,1),
        new LayoutTopology(4,1),
        new LayoutTopology(2,2)
    ],
    CellGapMm:2,BoundaryMarginMm:1,KeepoutMarginMm:1,
    NonCellMassG:48,TargetEnergyWh:48,MaxSearchNodes:6000);
var layouts=LayoutOptimizer.Optimize(layoutCase);
var threeCell=layouts.Options.Single(x=>x.CellId=="SYNTH-LONG"&&x.Topology==new LayoutTopology(3,1));
Assert(threeCell.Placements.Count==3&&Close(threeCell.NominalEnergyWh,57.75)&&
    Close(threeCell.EstimatedMassG,234)&&threeCell.MeetsEnergyTarget,
    "Irregular L-shaped zones fit exactly three nominal cells and account for assumed overhead");
Assert(layouts.Options.Any(x=>x.CellId=="SYNTH-COMPACT"&&
    x.Topology==new LayoutTopology(4,1) && !x.MeetsEnergyTarget),
    "More compact cells support a 4S layout but need not meet the energy target");
Assert(!layouts.Options.Any(x=>x.CellId=="SYNTH-LONG" &&
    x.Topology==new LayoutTopology(4,1)),
    "Oversized long-cell 4S layout is not spuriously reported as feasible");
Assert(layouts.Options[0].ParetoOnEnergyMass &&
    layouts.Options.Any(x=>x.CellId=="SYNTH-COMPACT" && x.ParetoOnEnergyMass),
    "Energy-vs-mass Pareto front retains lightweight alternatives without a single fake optimum");
Assert(layouts.Options[0].CellId=="SYNTH-LONG"&&
    layouts.ToMarkdown(layoutCase).Contains("NOT AN OPTIMALITY CERTIFICATE")&&
    layouts.ToMarkdown(layoutCase).Contains("NOT pack envelope volume"),
    "Sorted candidates disclose energy-density denominator and bounded-search limitations");
foreach(var option in layouts.Options)
{
    var cell=layoutCase.Cells.Single(x=>x.Id==option.CellId);
    foreach(var item in option.Placements)
    {
        var zone=layoutCase.Zones[item.ZoneIndex];
        Assert(item.XMm>=zone.XMm+layoutCase.BoundaryMarginMm-1e-7 &&
            item.YMm>=zone.YMm+layoutCase.BoundaryMarginMm-1e-7 &&
            item.XMm+item.WidthMm<=zone.XMm+zone.WidthMm-layoutCase.BoundaryMarginMm+1e-7 &&
            item.YMm+item.HeightMm<=zone.YMm+zone.HeightMm-layoutCase.BoundaryMarginMm+1e-7 &&
            cell.ThicknessMm<=zone.MaxThicknessMm,
            "Each placed cell stays entirely inside a local XY/Z clearance");
        foreach(var block in layoutCase.Keepouts)
            Assert(item.XMm+item.WidthMm<=block.XMm-layoutCase.KeepoutMarginMm+1e-7 ||
                item.XMm>=block.XMm+block.WidthMm+layoutCase.KeepoutMarginMm-1e-7 ||
                item.YMm+item.HeightMm<=block.YMm-layoutCase.KeepoutMarginMm+1e-7 ||
                item.YMm>=block.YMm+block.HeightMm+layoutCase.KeepoutMarginMm-1e-7,
                "No cell footprint touches a keepout plus its margin");
    }
    for(int i=0;i<option.Placements.Count;i++)
    for(int j=i+1;j<option.Placements.Count;j++)
    {
        var a=option.Placements[i];var b=option.Placements[j];
        Assert(a.XMm+a.WidthMm+layoutCase.CellGapMm<=b.XMm+1e-7 ||
            b.XMm+b.WidthMm+layoutCase.CellGapMm<=a.XMm+1e-7 ||
            a.YMm+a.HeightMm+layoutCase.CellGapMm<=b.YMm+1e-7 ||
            b.YMm+b.HeightMm+layoutCase.CellGapMm<=a.YMm+1e-7,
            "Placed cells maintain the required XY gap");
    }
}
var rotated = LayoutOptimizer.Optimize(new LayoutProblem(
    [new LayoutZone(0,0,40,68,7)],[],
    [new LayoutCellSpec("ROTATION",60,30,6,3.8,2,40,true)],
    [new LayoutTopology(1,1)],BoundaryMarginMm:1));
Assert(rotated.Options.Count==1&&rotated.Options[0].Placements[0].Rotated90,
    "90-degree rotation can rescue an otherwise impossible footprint");
var unrotated=LayoutOptimizer.Optimize(new LayoutProblem(
    [new LayoutZone(0,0,40,68,7)],[],
    [new LayoutCellSpec("NO_ROT",60,30,6,3.8,2,40,false)],
    [new LayoutTopology(1,1)],BoundaryMarginMm:1));
Assert(unrotated.Options.Count==0,
    "Rotation restrictions are treated as hard constraints");
var tooThick=LayoutOptimizer.Optimize(new LayoutProblem(
    [new LayoutZone(0,0,100,100,5.9)],[],
    [new LayoutCellSpec("TOO_THICK",40,40,6,3.8,2,40)],
    [new LayoutTopology(1,1)]));
Assert(tooThick.Options.Count==0,
    "Local maximum Z height rejects geometrically overlapping thick cells");
try
{
    LayoutOptimizer.Optimize(new LayoutProblem(
        [new LayoutZone(0,0,50,50,7),new LayoutZone(40,40,50,50,7)],
        [],[new LayoutCellSpec("C",20,20,6,3.8,2,40)],
        [new LayoutTopology(1,1)]));
    throw new Exception("FAIL: overlapping zones must not inflate available energy-density volume");
}
catch(ArgumentException){Console.WriteLine("PASS: overlapping zones are rejected");}
try
{
    LayoutOptimizer.Optimize(new LayoutProblem(
        [new LayoutZone(double.NaN,0,50,50,7)],[],
        [new LayoutCellSpec("C",20,20,6,3.8,2,40)],
        [new LayoutTopology(1,1)]));
    throw new Exception("FAIL: invalid geometry must be rejected");
}
catch(ArgumentException){Console.WriteLine("PASS: NaN geometry rejected");}


// Envelope-first workflow: engineers may only receive a maximum battery L/W/H.
// Never infer available interior CAD space, connector location or pouch-tab geometry.
var dimensionRequest = new DimensionFirstRequest(
    new PackEnvelopeMm(200,118,7),
    [
        new LayoutCellSpec("A-SYNTH",62,55,5.8,3.85,5,57,false),
        new LayoutCellSpec("B-SYNTH",42,52,5.2,3.85,3,38,false)
    ],
    RequiredSeries:3,MaximumParallel:2,EdgeAllowanceMm:2,
    TotalThicknessReserveMm:.8,MinimumCellGapMm:2,
    AssumedNonCellMassG:35,TargetNominalWh:50);
var dimensionalGeometry=DimensionFirstLayout.Build(dimensionRequest);
Assert(dimensionalGeometry.Zones.Count==1 &&
    Close(dimensionalGeometry.Zones[0].MaxThicknessMm,6.2) &&
    Close(dimensionalGeometry.Zones[0].WidthMm,200) &&
    Close(dimensionalGeometry.Zones[0].HeightMm,118),
    "External battery L/W/H becomes one provisional XY zone with explicit reserved Z");
Assert(Close(dimensionalGeometry.OuterEnvelopeVolumeL ?? 0,200*118*7/1_000_000.0),
    "Outer envelope volume uses original H, not thickness left after reserved Z");
Assert(dimensionalGeometry.Topologies.Count==2 &&
    dimensionalGeometry.Topologies.All(x=>x.Series==3) &&
    dimensionalGeometry.Topologies[0].Parallel==1 &&
    dimensionalGeometry.Topologies[1].Parallel==2,
    "Dimension-first mode keeps voltage topology fixed while comparing P alternatives");
Assert(dimensionalGeometry.Cells.All(x=>!x.Allow90DegreeRotation) &&
    dimensionalGeometry.Keepouts.Count==0,
    "No speculative tab rotation or invented internal CAD keepouts");
var dimensionalOptions=LayoutOptimizer.Optimize(dimensionalGeometry);
Assert(dimensionalOptions.Options.Any(x=>x.CellId=="A-SYNTH"&&
    x.Topology==new LayoutTopology(3,1) && Close(x.NominalEnergyWh,57.75)),
    "2D dimensions screen a three-cell pack with transparent nominal energy arithmetic");
Assert(dimensionalOptions.Options.Any(x=>x.CellId=="A-SYNTH"&&
    x.Topology==new LayoutTopology(3,1) &&
    Close(x.NominalOuterEnvelopeWhL ?? 0,57.75/(200*118*7/1_000_000.0))),
    "Nominal Wh per L of external envelope uses unreserved outer H");
Assert(dimensionalOptions.Options.Any(x=>x.CellId=="B-SYNTH"&&
    x.Topology==new LayoutTopology(3,1)),
    "Second qualified cell outline is actually compared using the same outer envelope");
Assert(dimensionalOptions.ToMarkdown(dimensionalGeometry)
    .Contains("OUTER PACK L/W/H ONLY") &&
    dimensionalOptions.ToMarkdown(dimensionalGeometry)
    .Contains("NOT AN OPTIMALITY CERTIFICATE"),
    "Engineering report exposes dimension-only source and non-CAD feasibility limits");
try
{
    DimensionFirstLayout.Build(dimensionRequest with { TotalThicknessReserveMm=7 });
    throw new Exception("FAIL: Z reserve must not consume all height.");
}
catch(ArgumentException){Console.WriteLine("PASS: zero available cell Z height rejected");}
try
{
    DimensionFirstLayout.Build(dimensionRequest with { EdgeAllowanceMm=100 });
    throw new Exception("FAIL: edge margin cannot consume the whole XY envelope.");
}
catch(ArgumentException){Console.WriteLine("PASS: unusable XY after margins rejected");}
try
{
    DimensionFirstLayout.Build(dimensionRequest with { RequiredSeries=4,MaximumParallel=4 });
    throw new Exception("FAIL: unsupported topology count must not be silently reduced.");
}
catch(ArgumentException){Console.WriteLine("PASS: impossible requested S/P topology count rejected");}
try
{
    DimensionFirstLayout.Build(dimensionRequest with { MinimumCellGapMm=double.NaN });
    throw new Exception("FAIL: NaN gap must not reach optimizer.");
}
catch(ArgumentException){Console.WriteLine("PASS: nonfinite dimensional margin rejected");}
var thickCell=dimensionRequest with { Cells=[
    new LayoutCellSpec("SYNTH-THICK",60,50,6.3,3.85,5,57,false)
] };
Assert(LayoutOptimizer.Optimize(DimensionFirstLayout.Build(thickCell)).Options.Count==0,
    "Cell thicker than reserved Z clearance is rejected even with enough XY area");


// A user-marked 2D keepout must participate in geometry fit, not just appear on
// the diagram. No employer geometry or battery specifications are used.
var withKeepout=dimensionRequest with {
    Keepouts=[new LayoutKeepout(86,25,20,22)],
    KeepoutMarginMm=1
};
var excludedProblem=DimensionFirstLayout.Build(withKeepout);
var excludedResult=LayoutOptimizer.Optimize(excludedProblem);
Assert(excludedProblem.Keepouts.Count==1 &&
    Close(excludedProblem.KeepoutMarginMm,1) &&
    excludedProblem.InputBasis?.Contains("1 user-marked 2D FULL-HEIGHT keepout")==true,
    "Dimension-first keepouts and clearance enter the core and its source provenance");
Assert(excludedResult.Options.Count>0,
    "Synthetic obstruction preserves some geometrically feasible cell comparisons");
foreach(var option in excludedResult.Options)
foreach(var placed in option.Placements)
{
    var block=excludedProblem.Keepouts[0];
    double m=excludedProblem.KeepoutMarginMm;
    Assert(placed.XMm+placed.WidthMm<=block.XMm-m+1e-7 ||
        placed.XMm>=block.XMm+block.WidthMm+m-1e-7 ||
        placed.YMm+placed.HeightMm<=block.YMm-m+1e-7 ||
        placed.YMm>=block.YMm+block.HeightMm+m-1e-7,
        "Every actual placement remains outside the keepout plus user margin");
}
Assert(excludedResult.ToMarkdown(excludedProblem).Contains("keepouts: 1") &&
    excludedResult.ToMarkdown(excludedProblem).Contains("FULL-HEIGHT keepout"),
    "Engineering report states block count and simplified 2D extrusion assumption");

var fullyBlocked=dimensionRequest with {
    Keepouts=[new LayoutKeepout(0,0,200,118)],
    KeepoutMarginMm=0
};
Assert(LayoutOptimizer.Optimize(DimensionFirstLayout.Build(fullyBlocked)).Options.Count==0,
    "A no-cell rectangle covering the entire outer envelope leaves no false fit");
try
{
    DimensionFirstLayout.Build(dimensionRequest with {
        Keepouts=[new LayoutKeepout(193,10,15,15)]
    });
    throw new Exception("FAIL: a no-cell rectangle outside L/W must be rejected.");
}
catch(ArgumentException){Console.WriteLine("PASS: out-of-envelope keepout rejected");}
try
{
    DimensionFirstLayout.Build(dimensionRequest with {
        Keepouts=[new LayoutKeepout(double.NaN,5,8,8)]
    });
    throw new Exception("FAIL: a nonfinite keepout must be rejected.");
}
catch(ArgumentException){Console.WriteLine("PASS: nonfinite keepout rejected");}
try
{
    DimensionFirstLayout.Build(dimensionRequest with {
        Keepouts=Enumerable.Range(0,13).Select(i=>
            new LayoutKeepout(1+i,1,2,2)).ToArray()
    });
    throw new Exception("FAIL: excessive user keepouts must be bounded.");
}
catch(ArgumentException){Console.WriteLine("PASS: excessive keepouts rejected");}

Console.WriteLine("All PackFit engineering core checks passed.");
