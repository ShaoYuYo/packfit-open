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
 read("desktop/PackFit.Engineering.Desktop/MainWindow.Batch.cs")
].join("\n");

test("native WinUI events have C# handlers and every named control is unique",()=>{
 const handlers=[...source.matchAll(/(?:Click|SelectionChanged|PaneToggleRequested|SizeChanged)="([A-Za-z_]\w+)"/g)].map(x=>x[1]);
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
