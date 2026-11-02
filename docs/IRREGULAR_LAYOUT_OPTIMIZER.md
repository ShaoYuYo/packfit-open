# Irregular laptop battery-space layout explorer

**Status:** Experimental geometry pre-screen merged into `main` via [PR #11](https://github.com/ShaoYuYo/packfit-open/pull/11). [Linux CI](https://github.com/ShaoYuYo/packfit-open/actions/runs/41460502989), [native Windows](https://github.com/ShaoYuYo/packfit-open/actions/runs/41460502991), and [CodeQL](https://github.com/ShaoYuYo/packfit-open/actions/runs/41460502972) passed on the reviewed final PR head. This does not establish actual CAD clearance or independent user adoption. Not included in the tagged v0.8.0-beta.1 Windows release.

## Problem definition

Laptop batteries are often allocated **small, nonrectangular and locally height-constrained** spaces around motherboard, hinge, speakers, screws and cable routing. The first-pass question is not just "what is the Wh value?" It is:

> Given the system's usable volume, obstacles and mandatory clearance, which existing cell outline and S/P topology can fit, and what energy / mass / packaging trade-offs are worth further engineering?

This project implements the first **testable and reproducible** slice of that problem. The original user observation was shared by a battery-engineering professional during a conversation about workflow, **not** a product trial or endorsement. All public examples use fictional geometry, not employer or OEM data.

## The workflow

1. From the WinUI **Pack design** page, select **載入完全合成的 L 形空間範例**. This fills an editable JSON geometry description.
2. Model the allowed outline as **disjoint XY rectangles**. Their union can form an L shape, stair steps or separated bays. Supply `MaxThicknessMm` **per zone**.
3. Add forbidden XY rectangles (`Keepouts`) for major ribs, screws or other no-go areas. Set `CellGapMm`, `BoundaryMarginMm` and `KeepoutMarginMm` explicitly. Don't treat these defaults as OEM-approved minimums.
4. Enter multiple **rigid rectangular** cell candidates, including W/H/Z, nominal voltage, Ah, mass and whether a 90° turn is permitted.
5. Enter potential `Series` / `Parallel` topologies, target nominal energy, assumed non-cell weight and bounded search budget.
6. Run the layout search, inspect **each candidate XY diagram** and compare nominal energy, provisional pack Wh/kg, and gross zone Wh/L. Export the Markdown report using the existing Engineering Report action.
7. Independently verify detailed CAD clearances, PCB/BMS and wiring before considering a physical prototype.

### Reproduce without a Windows laptop

Clone the repository and install the public .NET 10 SDK:

```bash
dotnet run --project desktop/PackFit.Engineering.Cli -- layout \
  desktop/samples/irregular-layout-synthetic.json --format markdown

dotnet run --project desktop/PackFit.Engineering.Cli -- layout \
  desktop/samples/irregular-layout-synthetic.json --format json

# Refuses to overwrite an existing report.
dotnet run --project desktop/PackFit.Engineering.Cli -- layout \
  desktop/samples/irregular-layout-synthetic.json --output layout-review.md
```

The synthetic fixture has two disjoint zones forming an L shape, different zone-height limits, one no-go rectangle and two **invented** cell outlines. The example's longer 65 × 55 × 6 mm cell is expected to support a 3S1P geometry candidate with nominal **57.75 Wh**, assuming the other fictional inputs; the compact alternative supports more cells, but 4S1P reaches only **46.2 Wh**. These are test vectors, **not measured laptop performance**.

## Mathematical definitions

For one candidate cell:

- **Nominal cell energy (Wh):** nominal cell voltage (V) × capacity (Ah).
- **Nominal pack energy (Wh):** `S × P × V_cell × Ah_cell`.
- **Estimated pack weight (g):** `S × P × mass_cell + assumed_noncell_mass`. All PCB, housing, connector, harness, foam, shields, etc. must eventually be included accurately.
- **Provisional gravimetric energy density (Wh/kg):** nominal pack Wh divided by the above estimated mass in kg.
- **Gross-zone energy loading (Wh/L):** nominal Wh divided by the **sum of each input zone's XY area × that zone's maximum Z**. It includes keepouts in the denominator and is **not finished-pack volumetric energy density**.
- **Gross footprint fill:** total rigid cell XY area divided by total allowed-zone XY area. It does not account for excluded cutouts or seals as a usable metric.
- **Actual finished-pack Wh/L:** cannot be determined accurately without a verified 3D pack assembly boundary and complete inactive material volume.

The optimizer ranks by whether the target Wh was met, then highest nominal Wh, then highest estimated Wh/kg. It **also marks a limited two-objective Pareto frontier**, where no other listed candidate has both more/equal nominal Wh and less/equal assumed pack mass, with at least one strict improvement. This limited frontier compares only the *enumerated* candidates: it is **not** a global optimum proof, nor does it establish S/P electrical compatibility, manufacturability, cost or thermal performance. With different cost, weight, thickness or thermal targets, other candidates may be better.

## Exactly what the engine does and does not do

The core uses a **deterministic, bounded anchor-position placement search**. It considers feasible XY positions at region edges, obstacle edges and already-placed cell edges, with zero or 90° orientations when permitted. Each candidate must stay completely within **one** rectangular region and respect that region's Z ceiling. It checks cell-cell separation and keepout margins.

- Non-overlapping rectangles may touch to form an L shape. **Cells cannot straddle their decomposition boundary**, so a valid physical arrangement may be missed. This makes the first release deliberately conservative.
- Obstacles are 2D extrusions; it does **not** model staggered or sloping Z clearance over the area of a single cell.
- One found solution is a **feasible XY arrangement under entered constraints only**, not the best geometric packing in the mathematical sense.
- When the node budget is reached, a "not found" result **does not mean impossible**. A full-space exact method is not yet part of this project.
- The modeled cell outline is a rigid box: it does **not** represent pouch seals, tabs, bend/fold limitations, busbars, expansion, manufacturing tolerance stackups, insulation or thermal runaway mitigation.
- S/P labels represent a **nominal count and voltage/energy arithmetic** only. They do not establish routing, parallel-cell matching, current rating, BMS design, protection, manufacturability or electrical/thermal safety.
- No neural network, remote service, CAD file upload, proprietary measurement or automatic design approval occurs.
- JSON is limited to 200 KB and the total search-node request is bounded to protect desktop responsiveness.

**Research direction for a future exact solver:** A constraint-programming formulation with optional 2D rectangles and `NoOverlap2D`, plus polygon/height-map CAD restrictions and explicit wiring topology. OR-Tools' [NoOverlap2D model](https://github.com/google/or-tools/blob/stable/ortools/sat/cp_model.proto) is a useful reference, but OR-Tools is **not implemented in this release**. A whole-pack energy-density estimate must also account for structure, cooling and protection overhead; see NASA's [Battery Cell-to-Pack Scaling Trends](https://ntrs.nasa.gov/api/citations/20210017488/downloads/Battery_Cell_to_Pack_Scaling_Trends_for_Electric_Aircraft_6_14.pdf).

## What is needed from a real battery engineer

A privacy-safe acceptance test can use **normalized / synthetic coordinates** with the same abstract shape and boundary constraints, but no customer/product identifiers. The reviewer should decide whether the diagram makes it quicker to reject obviously impossible 2D proposals; that is a distinct claim from optimizing an actual pack.

A future production-quality workflow should offer:

1. **Geometry:** sanitized DXF or STEP import, polygonal boundaries, varying Z height maps, tabs and module keepouts; explicit source provenance and tolerance stackups.
2. **Cell catalogue:** local database of authorized cell dimensions, thickness-in-service, allowed rotations, qualified voltage windows, Wh and mass.
3. **Optimization:** optional exact solver with proven bound/solver status, Pareto frontier of Wh, mass, true pack-volume utilization, cost, assembly difficulty and routing penalties; enforce required S/P and connector locations.
4. **Engineering validation:** manufacturable BMS/PCB, routing, resistance, current, thermal/electrical risk, lifecycle and supply chain checks, plus traceable export back to MCAD/ECAD.

This PR only implements the bounded first stage. It intentionally avoids manufacturing, charging and safety approval claims.

## Tests

```bash
dotnet run --project desktop/PackFit.Engineering.Checks --configuration Release
dotnet run --project desktop/PackFit.Engineering.Cli --configuration Release -- layout desktop/samples/irregular-layout-synthetic.json --format json
npm run check
npm test
```

GitHub Actions additionally compiles and exercises the real native WinUI XAML MainWindow and layout smoke test. Passing CI is not real-hardware validation.
