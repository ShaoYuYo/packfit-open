using System.Globalization;
using System.Text;

namespace PackFit.Engineering.Core;

/// <summary>Non-overlapping orthogonal usable XY zone with a local maximum Z height.</summary>
public sealed record LayoutZone(double XMm, double YMm, double WidthMm, double HeightMm, double MaxThicknessMm);
/// <summary>Blocked XY rectangle. KeepoutMarginMm is added outside this rectangle.</summary>
public sealed record LayoutKeepout(double XMm, double YMm, double WidthMm, double HeightMm);
/// <summary>Rigid, rectangular cell footprint. Tab, pouch seal, swelling and cooling geometry are NOT modeled.</summary>
public sealed record LayoutCellSpec(
    string Id, double WidthMm, double HeightMm, double ThicknessMm,
    double NominalVoltageV, double CapacityAh, double MassG, bool Allow90DegreeRotation = true);
public sealed record LayoutTopology(int Series, int Parallel);
public sealed record LayoutProblem(
    IReadOnlyList<LayoutZone> Zones,
    IReadOnlyList<LayoutKeepout> Keepouts,
    IReadOnlyList<LayoutCellSpec> Cells,
    IReadOnlyList<LayoutTopology> Topologies,
    double CellGapMm = 2,
    double BoundaryMarginMm = 1,
    double KeepoutMarginMm = 1,
    double NonCellMassG = 40,
    double TargetEnergyWh = 0,
    int MaxSearchNodes = 6000);

public sealed record LayoutPlacement(
    int Index, int ZoneIndex, double XMm, double YMm,
    double WidthMm, double HeightMm, bool Rotated90);
public sealed record LayoutOption(
    string CellId, LayoutTopology Topology, IReadOnlyList<LayoutPlacement> Placements,
    double NominalVoltageV, double NominalCapacityAh, double NominalEnergyWh,
    double EstimatedMassG, double EstimatedPackWhKg,
    double GrossZoneEnergyLoadingWhL, double GrossFootprintFillPct,
    bool MeetsEnergyTarget, int ExploredNodes);

public sealed record LayoutResult(
    IReadOnlyList<LayoutOption> Options,
    IReadOnlyList<string> Rejected,
    bool AnySearchBudgetExceeded,
    double TotalZoneVolumeL,
    double TotalZoneAreaMm2)
{
    private static readonly CultureInfo C = CultureInfo.InvariantCulture;
    private static string F(double v) => v.ToString("0.##", C);
    public string ToMarkdown(LayoutProblem p)
    {
        var b = new StringBuilder();
        b.AppendLine("# Irregular battery layout — geometry pre-screen");
        b.AppendLine();
        b.AppendLine("> **NOT AN OPTIMALITY CERTIFICATE OR MANUFACTURABLE BATTERY DESIGN.**");
        b.AppendLine("Deterministic bounded XY packing of **rigid rectangular cell footprints** into non-overlapping orthogonal zones with per-zone maximum Z thickness. No tab/seal, expansion, cooling, routing, bend-radius, fixture, insulation, safety or electrical-connection feasibility is verified.");
        b.AppendLine();
        b.AppendLine($"- Usable zone definitions: {p.Zones.Count} · keepouts: {p.Keepouts.Count} · cell candidates: {p.Cells.Count} · topologies: {p.Topologies.Count}");
        b.AppendLine($"- Gross allowed-zone area: {F(TotalZoneAreaMm2)} mm²; gross volume: {F(TotalZoneVolumeL)} L (includes XY keepouts; NOT pack envelope volume)");
        b.AppendLine($"- Cell gap: {F(p.CellGapMm)} mm; boundary margin: {F(p.BoundaryMarginMm)} mm; keepout margin: {F(p.KeepoutMarginMm)} mm");
        b.AppendLine($"- Assumed NON-cell mass: {F(p.NonCellMassG)} g; target nominal energy: {F(p.TargetEnergyWh)} Wh");
        b.AppendLine($"- Maximum nodes per attempted topology/cell: {p.MaxSearchNodes}; any capped runs: {AnySearchBudgetExceeded}");
        b.AppendLine();
        b.AppendLine("## Candidate screening table");
        b.AppendLine();
        b.AppendLine("| Rank | Cell | S/P | Nominal Wh | Assumed pack g | Estimated Wh/kg | Wh/L of GROSS allowed zones | Gross XY cell fill | Meets Wh target |");
        b.AppendLine("|---:|---|---|---:|---:|---:|---:|---:|---|");
        if (Options.Count == 0) b.AppendLine("| — | No arrangement found within the search budget | — | — | — | — | — | — | — |");
        for (int i = 0; i < Options.Count; i++)
        {
            var o=Options[i];
            b.AppendLine($"| {i+1} | {o.CellId} | {o.Topology.Series}S{o.Topology.Parallel}P | {F(o.NominalEnergyWh)} | {F(o.EstimatedMassG)} | {F(o.EstimatedPackWhKg)} | {F(o.GrossZoneEnergyLoadingWhL)} | {F(o.GrossFootprintFillPct)}% | {(o.MeetsEnergyTarget ? "Yes" : "No")} |");
        }
        foreach (var option in Options.Take(5))
        {
            b.AppendLine();
            b.AppendLine($"### {option.CellId} · {option.Topology.Series}S{option.Topology.Parallel}P placement (XY mm)");
            b.AppendLine("| Cell | Zone | X | Y | W | H | 90° rotated |");
            b.AppendLine("|---:|---:|---:|---:|---:|---:|---|");
            foreach (var item in option.Placements)
                b.AppendLine($"| {item.Index+1} | {item.ZoneIndex+1} | {F(item.XMm)} | {F(item.YMm)} | {F(item.WidthMm)} | {F(item.HeightMm)} | {(item.Rotated90 ? "Yes" : "No")} |");
        }
        if (Rejected.Count != 0)
        {
            b.AppendLine();
            b.AppendLine("## Not found in bounded search");
            foreach (var line in Rejected.Take(20))
                b.AppendLine("- " + line);
        }
        b.AppendLine();
        b.AppendLine("## Mandatory engineering review before any real pack decision");
        b.AppendLine("- [ ] Validate exact available CAD volume and varying local Z clearance, as well as structural ribs and assembly tolerances.");
        b.AppendLine("- [ ] Model tab/terminal exit locations, pouch seals, BMS/PCB, connector, harness routing and service clearances as full keepouts.");
        b.AppendLine("- [ ] Check current, voltage windows, series/parallel matching, protection, thermal, swelling, abuse, aging and applicable OEM/safety requirements.");
        b.AppendLine("- [ ] Replace the assumed non-cell mass with a reviewed BOM; **Wh/L here is energy / gross zone volume, NOT product pack volumetric energy density**.");
        b.AppendLine("- [ ] Verify actual CAD collisions, manufacturability and assembly access. Rectilinear decomposition forbids straddling zone boundaries.");
        b.AppendLine("- [ ] This is a finite anchor-position search with a hard node cap, not a global optimum proof. Try multiple cells, topologies and orientations.");
        return b.ToString();
    }
}

public static class LayoutOptimizer
{
    private const double Eps = 1e-7;
    private static bool IsIn(double v,double low,double high)
        => double.IsFinite(v) && v>=low && v<=high;
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new ArgumentException(message);
    }
    private static bool Overlap(double x1,double y1,double w1,double h1,
        double x2,double y2,double w2,double h2)
        => x1+Eps<x2+w2 && x2+Eps<x1+w1 &&
           y1+Eps<y2+h2 && y2+Eps<y1+h1;
    private static bool Separated(double ax,double ay,double aw,double ah,
        double bx,double by,double bw,double bh,double gap)
        => ax+aw+gap<=bx+Eps || bx+bw+gap<=ax+Eps ||
           ay+ah+gap<=by+Eps || by+bh+gap<=ay+Eps;

    public static void Validate(LayoutProblem p)
    {
        ArgumentNullException.ThrowIfNull(p);
        Require(p.Zones is {Count: >=1 and <=12},"Provide 1 to 12 usable XY zones.");
        Require(p.Keepouts is {Count: <=30},"Provide at most 30 keepouts.");
        Require(p.Cells is {Count: >=1 and <=12},"Provide 1 to 12 cell candidates.");
        Require(p.Topologies is {Count: >=1 and <=16},"Provide 1 to 16 S/P topologies.");
        Require(IsIn(p.CellGapMm,0,100) && IsIn(p.BoundaryMarginMm,0,100) &&
            IsIn(p.KeepoutMarginMm,0,100),"Clearances must be finite and 0–100 mm.");
        Require(IsIn(p.NonCellMassG,0,20000) && IsIn(p.TargetEnergyWh,0,2000),
            "Assumed non-cell mass or target Wh is outside limits.");
        Require(p.MaxSearchNodes is >=100 and <=30000,"MaxSearchNodes must be 100–30000.");
        foreach (var zone in p.Zones)
        {
            Require(zone is not null && IsIn(zone.XMm,-10000,10000) &&
                IsIn(zone.YMm,-10000,10000) && IsIn(zone.WidthMm,1,2000) &&
                IsIn(zone.HeightMm,1,2000) && IsIn(zone.MaxThicknessMm,.1,100),
                "Invalid zone coordinate, XY footprint or maximum Z height.");
        }
        for (int i=0;i<p.Zones.Count;i++)
        for (int j=i+1;j<p.Zones.Count;j++)
        {
            var a=p.Zones[i];var b=p.Zones[j];
            Require(!Overlap(a.XMm,a.YMm,a.WidthMm,a.HeightMm,b.XMm,b.YMm,b.WidthMm,b.HeightMm),
                "Usable zones must not overlap. Decompose an irregular shape into disjoint rectangles.");
        }
        foreach(var k in p.Keepouts)
            Require(k is not null && IsIn(k.XMm,-10000,10000) &&
                IsIn(k.YMm,-10000,10000) && IsIn(k.WidthMm,.01,2000) &&
                IsIn(k.HeightMm,.01,2000),"Invalid keepout.");
        var names=new HashSet<string>(StringComparer.Ordinal);
        foreach(var cell in p.Cells)
        {
            Require(cell is not null && !string.IsNullOrEmpty(cell.Id) &&
                cell.Id.Length<=48 && cell.Id.All(ch=> char.IsAsciiLetterOrDigit(ch) ||
                ch is '-' or '_'),"Cell IDs must be 1–48 ASCII letters, digits, - or _.");
            Require(names.Add(cell.Id),"Cell IDs must be unique.");
            Require(IsIn(cell.WidthMm,.1,2000) && IsIn(cell.HeightMm,.1,2000) &&
                IsIn(cell.ThicknessMm,.1,100) && IsIn(cell.NominalVoltageV,1,5) &&
                IsIn(cell.CapacityAh,.01,50) && IsIn(cell.MassG,.01,2000),
                "Cell footprint, height, voltage, capacity or mass is invalid.");
        }
        foreach(var topology in p.Topologies)
            Require(topology is not null && topology.Series is >=1 and <=12 &&
                topology.Parallel is >=1 and <=4 &&
                topology.Series*topology.Parallel<=12,
                "Each topology needs 1–12 series, 1–4 parallel, and at most 12 cells.");
    }

    public static LayoutResult Optimize(LayoutProblem p)
    {
        Validate(p);
        double zoneVolumeL=p.Zones.Sum(z=>z.WidthMm*z.HeightMm*z.MaxThicknessMm)/1_000_000;
        double zoneArea=p.Zones.Sum(z=>z.WidthMm*z.HeightMm);
        var options=new List<LayoutOption>();
        var failures=new List<string>();
        bool truncated=false;
        foreach(var cell in p.Cells)
        foreach(var topology in p.Topologies.Distinct())
        {
            int count=topology.Series*topology.Parallel;
            var engine=new Search(p,cell);
            if(engine.TryFit(count,out var layout))
            {
                double wh=count*cell.NominalVoltageV*cell.CapacityAh;
                double grams=count*cell.MassG+p.NonCellMassG;
                double area=count*cell.WidthMm*cell.HeightMm;
                options.Add(new LayoutOption(cell.Id,topology,layout!,
                    topology.Series*cell.NominalVoltageV,topology.Parallel*cell.CapacityAh,
                    wh,grams,wh/(grams/1000),wh/zoneVolumeL,
                    area/zoneArea*100,wh+Eps>=p.TargetEnergyWh,engine.Visited));
            }
            else
                failures.Add(cell.Id+" "+topology.Series+"S"+topology.Parallel+"P"+
                    (engine.BudgetReached?" — no fit found before search budget":" — no fit found by bounded anchor search"));
            truncated |= engine.BudgetReached;
        }
        var ranked=options
            .OrderByDescending(x=>x.MeetsEnergyTarget)
            .ThenByDescending(x=>x.NominalEnergyWh)
            .ThenByDescending(x=>x.EstimatedPackWhKg)
            .ThenBy(x=>x.CellId,StringComparer.Ordinal)
            .ThenBy(x=>x.Topology.Series).ThenBy(x=>x.Topology.Parallel)
            .ToArray();
        return new LayoutResult(ranked,failures.AsReadOnly(),truncated,zoneVolumeL,zoneArea);
    }

    private sealed class Search
    {
        private readonly LayoutProblem p;
        private readonly LayoutCellSpec c;
        private readonly List<LayoutPlacement> placed=[];
        public int Visited {get;private set;}
        public bool BudgetReached {get;private set;}

        public Search(LayoutProblem p,LayoutCellSpec c){this.p=p;this.c=c;}

        public bool TryFit(int n,out IReadOnlyList<LayoutPlacement>? result)
        {
            bool ok=Place(0,n);
            result=ok ? placed.ToArray():null;
            return ok;
        }

        private bool Place(int index,int total)
        {
            if(index==total)return true;
            if(Visited >= p.MaxSearchNodes){BudgetReached=true;return false;}
            Visited++;
            // Conservative area bound: ignores keepouts but never rejects feasible layouts.
            double remaining=(total-index)*c.WidthMm*c.HeightMm;
            double zoneArea=p.Zones.Sum(z=>z.WidthMm*z.HeightMm);
            if(remaining>zoneArea+Eps)return false;

            for(int zi=0;zi<p.Zones.Count;zi++)
            {
                var zone=p.Zones[zi];
                if(c.ThicknessMm>zone.MaxThicknessMm+Eps)continue;
                for(int rotation=0;rotation<(c.Allow90DegreeRotation && Math.Abs(c.WidthMm-c.HeightMm)>Eps?2:1);rotation++)
                {
                    bool turned=rotation==1;
                    double w=turned?c.HeightMm:c.WidthMm;
                    double h=turned?c.WidthMm:c.HeightMm;
                    if(w>zone.WidthMm-2*p.BoundaryMarginMm+Eps ||
                       h>zone.HeightMm-2*p.BoundaryMarginMm+Eps)continue;
                    var xs=Anchors(zone, w, true);
                    var ys=Anchors(zone, h, false);
                    // Stable, deterministic bottom-left placement search; no randomness.
                    foreach(double y in ys)
                    foreach(double x in xs)
                    {
                        if(!Valid(x,y,w,h,zone))continue;
                        placed.Add(new LayoutPlacement(index,zi,x,y,w,h,turned));
                        if(Place(index+1,total))return true;
                        placed.RemoveAt(placed.Count-1);
                        if(BudgetReached)return false;
                    }
                }
            }
            return false;
        }

        private IReadOnlyList<double> Anchors(LayoutZone z,double size,bool horizontal)
        {
            double lo=(horizontal?z.XMm:z.YMm)+p.BoundaryMarginMm;
            double hi=(horizontal?z.XMm+z.WidthMm:z.YMm+z.HeightMm)-p.BoundaryMarginMm-size;
            var set=new SortedSet<double>{lo,hi};
            foreach(var o in p.Keepouts)
            {
                double start=horizontal?o.XMm:o.YMm;
                double end=start+(horizontal?o.WidthMm:o.HeightMm);
                set.Add(start-size-p.KeepoutMarginMm);
                set.Add(end+p.KeepoutMarginMm);
            }
            foreach(var other in placed)
            {
                double start=horizontal?other.XMm:other.YMm;
                double end=start+(horizontal?other.WidthMm:other.HeightMm);
                set.Add(start-size-p.CellGapMm);
                set.Add(end+p.CellGapMm);
            }
            return set.Where(x=>x+Eps>=lo && x<=hi+Eps).ToArray();
        }

        private bool Valid(double x,double y,double w,double h,LayoutZone zone)
        {
            double edge=p.BoundaryMarginMm;
            if(x<zone.XMm+edge-Eps || y<zone.YMm+edge-Eps ||
               x+w>zone.XMm+zone.WidthMm-edge+Eps ||
               y+h>zone.YMm+zone.HeightMm-edge+Eps)return false;
            foreach(var no in p.Keepouts)
            {
                double margin=p.KeepoutMarginMm;
                if(Overlap(x,y,w,h,no.XMm-margin,no.YMm-margin,
                    no.WidthMm+2*margin,no.HeightMm+2*margin))
                    return false;
            }
            foreach(var prev in placed)
                if(!Separated(x,y,w,h,prev.XMm,prev.YMm,
                    prev.WidthMm,prev.HeightMm,p.CellGapMm))
                    return false;
            return true;
        }
    }
}
