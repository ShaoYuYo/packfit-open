import test from "node:test";
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
import {dirname,resolve} from "node:path";
import {fileURLToPath} from "node:url";

const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const read=p=>readFileSync(resolve(root,p),"utf8");
const source=read("desktop/PackFit.Engineering.Desktop/MainWindow.xaml");
const main=[
 read("desktop/PackFit.Engineering.Desktop/MainWindow.xaml.cs"),
 read("desktop/PackFit.Engineering.Desktop/MainWindow.Tools.cs"),
 read("desktop/PackFit.Engineering.Desktop/MainWindow.Batch.cs"),
 read("desktop/PackFit.Engineering.Desktop/MainWindow.Insights.cs")
].join("\n");

test("native WinUI events have C# handlers and every named control is unique",()=>{
 const handlers=[...source.matchAll(/(?:Click|SelectionChanged|PaneToggleRequested|SizeChanged|TextChanged)="([A-Za-z_]\w+)"/g)].map(x=>x[1]);
 const missing=[...new Set(handlers.filter(x=>!new RegExp("\\b"+x+"\\s*\\(").test(main)))];
 assert.deepEqual(missing,[]);
 const names=[...source.matchAll(/x:Name="([^"]+)"/g)].map(x=>x[1]);
 assert.equal(new Set(names).size,names.length);
});

test("WinUI native folder distribution includes startup troubleshooting scripts",()=>{
 const project=read("desktop/PackFit.Engineering.Desktop/PackFit.Engineering.Desktop.csproj");
 assert.match(project,/<UseWinUI>true<\/UseWinUI>/);
 assert.match(project,/<WindowsPackageType>None<\/WindowsPackageType>/);
 assert.match(project,/<WindowsAppSDKSelfContained>true<\/WindowsAppSDKSelfContained>/);
 assert.match(project,/<PublishSingleFile>false<\/PublishSingleFile>/);
 assert.match(project,/Start-PackFit\.cmd/);
 assert.match(project,/Diagnose-PackFit\.ps1/);
 const release=read(".github/workflows/publish-beta3.yml");
 assert.match(release,/Start-PackFit\.cmd/);
 assert.match(release,/Diagnose-PackFit\.ps1/);
 assert.match(release,/Exploratory launch probe/);
});

test("fatal startup failures have a local log and recoverable mode",()=>{
 const app=read("desktop/PackFit.Engineering.Desktop/App.xaml.cs");
 const diagnostics=read("desktop/PackFit.Engineering.Desktop/StartupDiagnostics.cs");
 assert.match(app,/StartupDiagnostics\.Record/);
 assert.match(app,/new RecoveryWindow\(ex\)/);
 assert.match(diagnostics,/Environment\.SpecialFolder\.LocalApplicationData/);
 assert.match(diagnostics,/MaxBytes = 512_000/);
 assert.match(main,/--safe-ui/);
 assert.match(main,/AppTitleBar\.Visibility = Visibility\.Collapsed/);
});

test("Fluent typography and movement respect platform settings",()=>{
 const styles=read("desktop/PackFit.Engineering.Desktop/App.xaml");
 for(const token of ["PfEyebrowText","PfPageHeading","PfSectionHeading","PfLeadText","PfMetricValue"])
   assert.match(styles,new RegExp('x:Key="'+token+'"'));
 assert.match(styles,/Segoe UI Variable/);
 assert.match(source,/<MicaBackdrop/);
 assert.match(main,/new Windows\.UI\.ViewManagement\.UISettings\(\)\.AnimationsEnabled/);
 assert.match(main,/TimeSpan\.FromMilliseconds\(190\)/);
 assert.match(main,/WorkspaceRoot_SizeChanged/);
 assert.match(source,/x:Name="OverviewStatsGrid"/);
});

test("safe UI and its native compatibility flow bypass the XAML-based MainWindow",()=>{
 const app=read("desktop/PackFit.Engineering.Desktop/App.xaml.cs");
 const compat=read("desktop/PackFit.Engineering.Desktop/RecoveryWindow.cs");
 const appSafe=app.indexOf('a.Equals("--safe-ui"');
 const appNormal=app.indexOf('_mainWindow = new MainWindow();',appSafe);
 assert.ok(appSafe>=0 && appNormal>appSafe,"safe UI must branch before normal MainWindow load");
 assert.match(app,/new RecoveryWindow\(null\)/);
 assert.match(app,/--selftest-compat/);
 assert.match(compat,/BatteryReportParser\.ParseAll/);
 assert.match(compat,/TelemetryAnalyzer\.Analyze/);
 assert.match(compat,/ValidationMatrix\.Parse/);
 assert.match(compat,/PackCalculator\.Calculate/);
 assert.match(compat,/EngineeringReport\.Build/);
 assert.doesNotMatch(compat,/InitializeComponent\(\)/);
});

test("portable WinUI release retains its original .exe filename to preserve resource URI mapping",()=>{
 const release=read(".github/workflows/publish-beta4.yml");
 assert.match(release,/dist\/portable\/PackFit\.Engineering\.exe/);
 assert.doesNotMatch(release,/PackFit-Engineering-portable-win-x64\.exe/);
 assert.doesNotMatch(release,/Copy-Item .*PackFit\.Engineering\.exe/);
 assert.match(release,/Compatibility self-test: COMPAT_WINDOW_OK/);
 const winci=read(".github/workflows/desktop-winui.yml");
 assert.match(winci,/MAIN_WINDOW_OK/);
 assert.match(winci,/COMPAT_WINDOW_OK/);
});

test("v0.8 uses native diagram workspaces with fully wired UI events",()=>{
 const requiredPanels=["VisualsPanel","SleepPanel","BatteryPanel","TelemetryPanel",
   "ValidationPanel","PackPanel","GuidePanel","ResourcesPanel"];
 for(const p of requiredPanels)assert.match(source,new RegExp('x:Name="'+p+'"'));
 for(const control of ["BatteryVisualCanvas","TelemetryChartCanvas","PackDiagramCanvas",
   "SleepGraphCanvas","VisualsBattery","VisualsTelemetry","VisualsPack","GuideFlowCanvas",
   "WorkflowDiagram","ExperienceMode"])assert.match(source,new RegExp('x:Name="'+control+'"'));
 const insights=read("desktop/PackFit.Engineering.Desktop/MainWindow.Insights.cs");
 assert.match(insights,/LoadDemoClick/);
 assert.match(insights,/StandbyDrainEstimator\.Estimate/);
 assert.match(insights,/ExperienceModeChanged/);
 assert.match(insights,/EngineeringVisuals\.Telemetry/);
 assert.match(insights,/EngineeringVisuals\.Pack/);
 assert.match(insights,/RunVisualSmokeTest/);
});

test("chart renderer is a local-only native implementation and uses each signal's units",()=>{
 const renderer=read("desktop/PackFit.Engineering.Desktop/EngineeringVisuals.cs");
 assert.doesNotMatch(renderer,/new WebView|new HttpClient|https?:\/\//);
 for(const unit of ["Pack voltage","Pack current","Temperature","State of charge","Pack topology"])
   assert.ok(renderer.includes(unit)||unit==="Pack topology"&&renderer.includes("拓樸"));
 assert.match(renderer,/Microsoft\.UI\.Xaml\.Shapes/);
 assert.match(renderer,/Viewbox/);
 assert.match(renderer,/trace\.Select\(p=>p\.TemperatureC\)/);
});

test("independent compatibility UI receives the same visual and demo capabilities",()=>{
 const app=read("desktop/PackFit.Engineering.Desktop/App.xaml.cs");
 const compat=read("desktop/PackFit.Engineering.Desktop/RecoveryWindow.cs");
 for(const method of ["LoadDemoAsync","CalculateStandbyAsync","UpdateVisuals","RunVisualSmokeTest"])
   assert.ok(compat.includes(method));
 assert.match(app,/--selftest-visuals/);
 assert.match(app,/--selftest-compat-visuals/);
 const ci=read(".github/workflows/desktop-winui.yml");
 assert.match(ci,/VISUALS_OK/);
 assert.match(ci,/COMPAT_VISUALS_OK/);
});

test("premium workbench couples visible status with real source provenance",()=>{
  const premium=read("desktop/PackFit.Engineering.Desktop/MainWindow.Premium.cs");
  const insights=read("desktop/PackFit.Engineering.Desktop/MainWindow.Insights.cs");
  for(const name of ["DataModeText","DataModeDetails","OverviewBatterySource",
    "OverviewTelemetrySource","OverviewValidationSource","SessionActions",
    "FindingFilterBox","FindingCodeSummary","FindingPreviewCaption","WorkspaceScroll"])
    assert.match(source,new RegExp('x:Name="'+name+'"'));
  for(const signal of ["HasSyntheticData","ConfirmReplaceSessionAsync",
    "ClearWorkspaceClick","DataProvenanceMarkdown","RenderTelemetryFindings",
    "FindingFilter_TextChanged","FindingCodeCounts","HiddenFindings"])
    assert.match(premium,new RegExp(signal));
  assert.match(main,/DataProvenanceMarkdown\(\)/);
  assert.match(insights,/if\s*\(!await ConfirmReplaceSessionAsync\(\)\) return/);
  assert.match(insights,/Synthetic data provenance contract failed/);
  assert.match(insights,/Workspace reset smoke test failed/);
  assert.match(source,/Key="S" Modifiers="Control"/);
  assert.match(source,/AutomationProperties.Name="搜尋異常紀錄"/);
  assert.match(main,/UpdateSessionStatus\(\)/);
  assert.match(main,/WorkspaceScroll.ChangeView\(null, 0, null, true\)/);
});
