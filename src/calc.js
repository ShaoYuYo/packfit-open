/**
 * Pure battery calculations. Inputs are explicit SI-related units; no DOM or I/O.
 * All estimates assume ideal cells, constant voltage at nominal value and a
 * rectangular external envelope. See docs/METHODOLOGY.md.
 */
export class CalculationError extends RangeError {
  constructor(field, message) {
    super(message);
    this.name = "CalculationError";
    this.field = field;
  }
}
function finiteInRange(raw, field, min, max, integer = false) {
  const value = Number(raw);
  if (raw === "" || raw === null || raw === undefined || !Number.isFinite(value)
      || value < min || value > max || (integer && !Number.isInteger(value))) {
    throw new CalculationError(field, field + " must be " + (integer ? "an integer" : "a finite number") + " between " + min + " and " + max);
  }
  return value;
}
export function calculateRuntime(input) {
  const capacityWh = finiteInRange(input.capacityWh, "capacityWh", 0.01, 2000);
  const powerW = finiteInRange(input.powerW, "powerW", 0.01, 5000);
  const reservePct = finiteInRange(input.reservePct, "reservePct", 0, 90);
  const efficiencyPct = finiteInRange(input.efficiencyPct, "efficiencyPct", 1, 100);
  const usableFraction = (1 - reservePct / 100) * (efficiencyPct / 100);
  const usableWh = capacityWh * usableFraction;
  return Object.freeze({capacityWh, powerW, reservePct, efficiencyPct, usableFraction, usableWh, hours: usableWh / powerW});
}
export function calculatePack(input) {
  const series = finiteInRange(input.series, "series", 1, 32, true);
  const parallel = finiteInRange(input.parallel, "parallel", 1, 32, true);
  const nominalCellV = finiteInRange(input.nominalCellV, "nominalCellV", 1, 5);
  const cellCapacityMah = finiteInRange(input.cellCapacityMah, "cellCapacityMah", 10, 50000);
  const cellMassG = finiteInRange(input.cellMassG, "cellMassG", 0.1, 2000);
  const overheadMassG = finiteInRange(input.overheadMassG, "overheadMassG", 0, 10000);
  const xMm = finiteInRange(input.xMm, "xMm", 0.1, 10000);
  const yMm = finiteInRange(input.yMm, "yMm", 0.1, 10000);
  const zMm = finiteInRange(input.zMm, "zMm", 0.1, 10000);
  const loadW = finiteInRange(input.loadW, "loadW", 0.01, 5000);
  const count = series * parallel;
  const nominalVoltageV = series * nominalCellV;
  const capacityAh = parallel * cellCapacityMah / 1000;
  const energyWh = nominalVoltageV * capacityAh;
  const totalMassG = count * cellMassG + overheadMassG;
  const envelopeVolumeL = xMm * yMm * zMm / 1e6;
  const gravimetricWhKg = energyWh / (totalMassG / 1000);
  const volumetricWhL = energyWh / envelopeVolumeL;
  const idealRuntimeHours = energyWh / loadW;
  const warnings = [];
  if (energyWh > 100) warnings.push("above100Wh");
  if (series > 8 || parallel > 8) warnings.push("highCellCount");
  if (overheadMassG === 0) warnings.push("zeroOverhead");
  return Object.freeze({series, parallel, count, nominalCellV, cellCapacityMah, cellMassG, overheadMassG,
    xMm,yMm,zMm,loadW,nominalVoltageV,capacityAh,energyWh,totalMassG,envelopeVolumeL,
    gravimetricWhKg,volumetricWhL,idealRuntimeHours,warnings});
}
export function getPackInputKeys() {
  return ["series","parallel","nominalCellV","cellCapacityMah","cellMassG","overheadMassG","xMm","yMm","zMm","loadW"];
}