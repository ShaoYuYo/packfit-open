using System.Globalization;

namespace PackFit.Engineering.Core;

/// <summary>Nominal only; does not establish electrical, thermal, mechanical or safety compliance.</summary>
public sealed record PackDesign(
    int Series, int Parallel, double CellVoltageV, double CellCapacityAh,
    double CellMassG, double NonCellMassG, double LengthMm, double WidthMm,
    double HeightMm, double LoadW);

public sealed record PackResult(
    int CellCount, double NominalVoltageV, double CapacityAh,
    double EnergyWh, double TotalMassG, double VolumeL,
    double GravimetricWhKg, double VolumetricWhL,
    double IdealRuntimeHours);

public static class PackCalculator
{
    public static PackResult Calculate(PackDesign p)
    {
        Require(p.Series is >= 1 and <= 32, "Series must be 1–32.");
        Require(p.Parallel is >= 1 and <= 32, "Parallel must be 1–32.");
        Check(p.CellVoltageV, 1, 5, nameof(p.CellVoltageV));
        Check(p.CellCapacityAh, .01, 50, nameof(p.CellCapacityAh));
        Check(p.CellMassG, .01, 2000, nameof(p.CellMassG));
        Check(p.NonCellMassG, 0, 20000, nameof(p.NonCellMassG));
        Check(p.LengthMm, 1, 2000, nameof(p.LengthMm));
        Check(p.WidthMm, 1, 2000, nameof(p.WidthMm));
        Check(p.HeightMm, .1, 200, nameof(p.HeightMm));
        Check(p.LoadW, .01, 2000, nameof(p.LoadW));

        int count = checked(p.Series * p.Parallel);
        double voltage = p.Series * p.CellVoltageV;
        double ah = p.Parallel * p.CellCapacityAh;
        double energy = voltage * ah;
        double mass = count * p.CellMassG + p.NonCellMassG;
        double volumeL = p.LengthMm * p.WidthMm * p.HeightMm / 1_000_000;
        return new(count, voltage, ah, energy, mass, volumeL,
            energy / (mass / 1000), energy / volumeL, energy / p.LoadW);
    }

    private static void Check(double value, double min, double max, string name)
        => Require(double.IsFinite(value) && value >= min && value <= max,
            $"{name} must be {min.ToString(CultureInfo.InvariantCulture)} to {max.ToString(CultureInfo.InvariantCulture)}.");

    private static void Require(bool ok, string message)
    {
        if (!ok) throw new ArgumentOutOfRangeException(nameof(message), message);
    }
}
