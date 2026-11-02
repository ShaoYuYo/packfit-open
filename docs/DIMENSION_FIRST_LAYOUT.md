# Dimension-first Battery Pack Screening — without 3D CAD

**Status:** New native WinUI feature under [PR #13](https://github.com/ShaoYuYo/packfit-open/pull/13). The source implementation may change until CI review and merge. It is not in the previously tagged `v0.8.0-beta.1` Windows download.

## Why this workflow exists

In informal needs discovery, a laptop-battery engineer explained that the system team often supplies the battery packaging **L, W and H**, while the battery engineer cannot open the original 3D model. The layout problem is therefore to compare potential cell outlines, nominal energy and tentative arrangement **without requiring CAD access**.

This is a genuine **problem statement**, not a PackFit user trial, endorsement, measured time saving, or permission to publish actual project drawings. Everything in this walkthrough uses invented data.

## Start with L × W × H

Open **Pack design → 不規則空間配置** in the native Windows app:

1. Under **01 / 只要 L × W × H**, fill in the outer battery allocation's `長 L`, `寬 W`, `厚 H` (mm). **These are packaging limits, not proven free interior space.**
2. Enter candidate **Cell A** dimensions (`L/W/H` in mm), nominal capacity (Ah) and cell mass (g). Optionally fill **all five fields** for **Cell B** to compare two different outlines. Both use the same entered nominal voltage, but *different cells are compared, never mixed together within one topology*.
3. Set the required number of **series cells S**, the maximum parallel number **P**, nominal voltage per cell, target Wh and noncell weight assumption. PackFit evaluates `S×1P` through `S×MaxP` (no inference that all topologies are electrically suitable).
4. Explicitly choose **side allowance per edge** (mm), **total reserved Z thickness** (mm) and **minimum cell-to-cell gap** (mm). These values are **project assumptions** and not manufacturer-verified clearances.
5. Leave 90° rotation **unchecked** unless pouch-tab direction, connector exits and production constraints permit rotating the candidate outline.
6. Select **用長寬高找配置方案** and inspect each geometry candidate, its nominal Wh, assumed mass, provisional Wh/kg and limited energy/mass Pareto flag.
7. Export **Engineering Report** to record the exact placement coordinates, input basis, assumptions, and the outstanding engineering checklist.

The JSON and L-shaped zone editor remain available under **Advanced**, *only* when real irregular-space details and height-limited subregions are known.

### Public demonstration, no proprietary measurements

Click **填入完全虛構的尺寸範例**. The sample populates:

| Input | Invented value |
|---|---:|
| Outer L × W × H | 200 × 118 × 7 mm |
| Cell A rigid outline | 62 × 55 × 5.8 mm |
| Cell A nominal / mass | 3.85 V, 5 Ah, 57 g |
| Cell B rigid outline | 42 × 52 × 5.2 mm |
| Cell B nominal / mass | 3.85 V, 3 Ah, 38 g |
| Series / maximum parallel | 3S, up to 2P |
| Edge allowance / Z reserve / gap | 2 / 0.8 / 2 mm |
| Assumed noncell mass | 35 g |
| Nominal target | 50 Wh |

The available gross height *after the manually entered thickness reserve* is 6.2 mm. The case exercises S/P and geometry comparisons using invented values, not qualified battery specifications or a validated pack design.

## Exact scope and mathematics

The new, testable core entry point is `DimensionFirstLayout.Build(request)`. It constructs a single provisional XY rectangle based on the entered outer L/W, with a Z ceiling of `H - TotalThicknessReserve`. The full `LayoutOptimizer` then checks each candidate cell against the assumed side, gap and Z limits and produces sorted layout alternatives.

The model **does not infer internal shape** from outer dimensions. With only L/W/H, the system does **not** know:

- structural ribs, screw towers, speakers, hinge elements, PCB/BMS/connector position;
- pouch seals, tab fold directions, tape/foam/compression, swelling, cooling paths or actual assembly access;
- local varying height (a single `H` is only a package-wide cap, not a Z map);
- OEM-required tolerances, voltage windows, allowable parallel connection or safety requirements.

Thus **“the software found a fit” means only “rigid cell footprints fit the user's simplified constraints.”** It is not evidence a battery can be produced, that the pack is electrically viable, or that it achieves actual pack Wh/L.

The report explicitly labels `OUTER PACK L/W/H ONLY` and retains the user's entered reserve and clearance parameters. The reported energy per gross zone volume is **not finished-pack volumetric ED**. Nominal Wh and assumed Wh/kg do not incorporate charge/discharge efficiency, de-rating, aging or environmental effects.

## Engineering acceptance criteria

Tests verify:

- a single geometry zone correctly keeps outer XY and subtracts the total Z reserve;
- topology is **fixed S** with a user-selected upper bound on **P**, so the software doesn't casually compare different system voltages;
- a second candidate is genuinely evaluated; rotation is disabled unless selected;
- impossible XY or Z allowances, unsupported topology counts and invalid finite numbers are rejected;
- the full native WinUI sample creates a diagram and includes the external-envelope warning in the report;
- editing any outer dimension discards stale results rather than leaving a misleading previous geometry on screen.

GitHub Actions perform Linux .NET checks, Windows real MainWindow/diagram smoke and CodeQL security scanning. Actual engineering acceptance still needs a qualified reviewer who can compare **synthetic or approved redacted** geometry against a separate mechanical CAD check.

### Further validated user research

One unanswered question matters before building a full packaging recommendation engine:

> Are the dimensions the engineer receives a simple rectangular battery **outer-envelope maximum**, or do they come with a 2D diagram identifying **cutouts / restricted regions**?

Do **not** request actual company design files. An abstract answer is enough to decide between a single-box workflow and multi-zone geometry input.

## Source versus published release

This feature is developed in open source and uses no remote server. The older `v0.8.0-beta.1` Windows ZIP will not automatically acquire the new UI. Consult the latest successful native Windows CI build after the PR is merged, and continue to treat it as experimental until independently exercised.
