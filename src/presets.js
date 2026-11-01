/** Illustrative demonstration inputs, not product specifications. */
export const DEVICE_PRESETS = Object.freeze({
  ultrabook: {capacityWh: 56, powerW: 10, reservePct: 10, efficiencyPct: 90},
  workstation: {capacityWh: 83, powerW: 34, reservePct: 10, efficiencyPct: 90},
  tablet: {capacityWh: 30, powerW: 6, reservePct: 10, efficiencyPct: 93},
  powerbank: {capacityWh: 74, powerW: 10, reservePct: 10, efficiencyPct: 85}
});
export const PACK_PRESETS = Object.freeze({
  slim: {series:3,parallel:1,nominalCellV:3.85,cellCapacityMah:5000,cellMassG:57,overheadMassG:65,xMm:240,yMm:70,zMm:6.8,loadW:16},
  performance: {series:4,parallel:1,nominalCellV:3.85,cellCapacityMah:6000,cellMassG:76,overheadMassG:82,xMm:280,yMm:88,zMm:7.6,loadW:35},
  compact: {series:2,parallel:1,nominalCellV:3.85,cellCapacityMah:3600,cellMassG:46,overheadMassG:45,xMm:170,yMm:64,zMm:7,loadW:8}
});