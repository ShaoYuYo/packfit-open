namespace PackFit.Engineering.Core;

/// <summary>
/// Outer dimensions supplied at the start of a battery-pack request. These are
/// maxima, not proof that every internal millimeter is available for cells.
/// </summary>
public sealed record PackEnvelopeMm(double LengthMm, double WidthMm, double ThicknessMm);

/// <summary>
/// Use when the engineer has an outer L/W/H but cannot open a 3D chassis CAD file.
/// The cell list must use intentionally entered, qualified candidate dimensions.
/// </summary>
public sealed record DimensionFirstRequest(
    PackEnvelopeMm Envelope,
    IReadOnlyList<LayoutCellSpec> Cells,
    int RequiredSeries,
    int MaximumParallel,
    double EdgeAllowanceMm,
    double TotalThicknessReserveMm,
    double MinimumCellGapMm,
    double AssumedNonCellMassG,
    double TargetNominalWh,
    int SearchBudgetPerCombination = 4000,
    IReadOnlyList<LayoutKeepout>? Keepouts = null,
    double KeepoutMarginMm = 0);

public static class DimensionFirstLayout
{
    /// <summary>
    /// Builds conservative, reproducible 2D geometry input from the user-entered
    /// *external* battery envelope. No algorithm can infer internal keepouts from L/W/H.
    /// </summary>
    public static LayoutProblem Build(DimensionFirstRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Envelope);
        ArgumentNullException.ThrowIfNull(request.Cells);

        static void Between(double value, double lo, double hi, string name)
        {
            if (!double.IsFinite(value) || value < lo || value > hi)
                throw new ArgumentOutOfRangeException(name,
                    $"{name} must be finite and in the range {lo}–{hi}.");
        }
        Between(request.Envelope.LengthMm, 1, 2000, nameof(request.Envelope.LengthMm));
        Between(request.Envelope.WidthMm, 1, 2000, nameof(request.Envelope.WidthMm));
        Between(request.Envelope.ThicknessMm, .1, 100, nameof(request.Envelope.ThicknessMm));
        Between(request.EdgeAllowanceMm, 0, 100, nameof(request.EdgeAllowanceMm));
        Between(request.TotalThicknessReserveMm, 0, 100, nameof(request.TotalThicknessReserveMm));
        Between(request.MinimumCellGapMm, 0, 100, nameof(request.MinimumCellGapMm));
        Between(request.KeepoutMarginMm, 0, 100, nameof(request.KeepoutMarginMm));
        Between(request.AssumedNonCellMassG, 0, 20000, nameof(request.AssumedNonCellMassG));
        Between(request.TargetNominalWh, 0, 2000, nameof(request.TargetNominalWh));
        if (request.Envelope.LengthMm - 2 * request.EdgeAllowanceMm <= 0 ||
            request.Envelope.WidthMm - 2 * request.EdgeAllowanceMm <= 0)
            throw new ArgumentException(
                "Side allowances consume the entire outer footprint; reduce them or correct L/W.");
        double availableZ = request.Envelope.ThicknessMm - request.TotalThicknessReserveMm;
        if (availableZ <= 0)
            throw new ArgumentException(
                "Total Z reserve leaves no positive cell thickness; check the entered pack height.");
        if (request.Cells.Count is < 1 or > 6)
            throw new ArgumentException("Supply one to six candidate cell outlines.");
        if (request.RequiredSeries is < 1 or > 12 ||
            request.MaximumParallel is < 1 or > 4 ||
            request.RequiredSeries * request.MaximumParallel > 12)
            throw new ArgumentException(
                "Require 1–12 series and 1–4 parallel, with at most twelve cells in each candidate topology.");
        if (request.SearchBudgetPerCombination is < 100 or > 30000)
            throw new ArgumentOutOfRangeException(nameof(request.SearchBudgetPerCombination));
        var keepouts = request.Keepouts?.ToArray() ?? [];
        if (keepouts.Length > 12)
            throw new ArgumentException("Dimension-first editor supports at most twelve blocked rectangles.");
        foreach (var block in keepouts)
        {
            ArgumentNullException.ThrowIfNull(block);
            Between(block.XMm, 0, request.Envelope.LengthMm, "Keepout.X");
            Between(block.YMm, 0, request.Envelope.WidthMm, "Keepout.Y");
            Between(block.WidthMm, .1, request.Envelope.LengthMm, "Keepout.Width");
            Between(block.HeightMm, .1, request.Envelope.WidthMm, "Keepout.Height");
            if (block.XMm + block.WidthMm > request.Envelope.LengthMm + 1e-7 ||
                block.YMm + block.HeightMm > request.Envelope.WidthMm + 1e-7)
                throw new ArgumentException("Blocked area extends beyond the provided outer L/W envelope.");
        }

        var topologies = Enumerable.Range(1, request.MaximumParallel)
            .Select(p => new LayoutTopology(request.RequiredSeries, p)).ToArray();
        var problem = new LayoutProblem(
            [new LayoutZone(0, 0, request.Envelope.LengthMm,
                request.Envelope.WidthMm, availableZ)],
            keepouts, request.Cells.ToArray(), topologies,
            CellGapMm: request.MinimumCellGapMm,
            BoundaryMarginMm: request.EdgeAllowanceMm,
            KeepoutMarginMm: request.KeepoutMarginMm,
            NonCellMassG: request.AssumedNonCellMassG,
            TargetEnergyWh: request.TargetNominalWh,
            MaxSearchNodes: request.SearchBudgetPerCombination)
        {
            OuterEnvelopeVolumeL = request.Envelope.LengthMm *
                request.Envelope.WidthMm * request.Envelope.ThicknessMm / 1_000_000,
            InputBasis = "OUTER PACK L/W/H ONLY. Rectangular XY envelope; " +
                (keepouts.Length==0
                    ? "NO INTERNAL KEEPOUTS PROVIDED (unknown obstructions may exist); "
                    : $"{keepouts.Length} user-marked 2D FULL-HEIGHT keepout rectangles; ") +
                "Z budget is outer thickness minus the user-supplied total Z reserve. " +
                "Do not treat estimated geometry as cleared CAD or a production pack."
        };
        LayoutOptimizer.Validate(problem);
        return problem;
    }
}
