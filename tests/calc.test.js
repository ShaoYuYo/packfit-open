import test from "node:test";
import assert from "node:assert/strict";
import {calculateRuntime,calculatePack,CalculationError} from "../src/calc.js";
import {formatDuration} from "../src/format.js";
import {PACK_PRESETS} from "../src/presets.js";
import {scenariosToCsv} from "../src/csv.js";

test("runtime applies reserve and conversion efficiency exactly once",()=>{
  const result=calculateRuntime({capacityWh:56,powerW:12,reservePct:10,efficiencyPct:90});
  assert.ok(Math.abs(result.usableWh-45.36)<1e-9);
  assert.ok(Math.abs(result.hours-3.78)<1e-9);
});
test("zero reserve and full efficiency equal theoretical Wh/W",()=>{
  assert.equal(calculateRuntime({capacityWh:80,powerW:20,reservePct:0,efficiencyPct:100}).hours,4);
});
test("rejects negative, infinity, NaN, missing and invalid fields",()=>{
  for (const value of ["",NaN,Infinity,-5,0,"abc"]) {
    assert.throws(()=>calculateRuntime({capacityWh:value,powerW:10,reservePct:10,efficiencyPct:90}),CalculationError);
  }
  assert.throws(()=>calculateRuntime({capacityWh:80,powerW:0,reservePct:0,efficiencyPct:100}),CalculationError);
  assert.throws(()=>calculateRuntime({capacityWh:80,powerW:10,reservePct:95,efficiencyPct:100}),CalculationError);
});
test("3S1P nominal energy, voltage and full-pack density",()=>{
  const result=calculatePack(PACK_PRESETS.slim);
  assert.ok(Math.abs(result.energyWh-57.75)<1e-8);
  assert.ok(Math.abs(result.nominalVoltageV-11.55)<1e-8);
  assert.equal(result.count,3);
  assert.equal(result.totalMassG,236);
  assert.ok(Math.abs(result.envelopeVolumeL-0.11424)<1e-10);
  assert.ok(Math.abs(result.gravimetricWhKg-57.75/0.236)<1e-8);
  assert.ok(Math.abs(result.volumetricWhL-57.75/0.11424)<1e-8);
});
test("parallel strings scale capacity without changing voltage",()=>{
  const a=calculatePack(PACK_PRESETS.slim);
  const b=calculatePack({...PACK_PRESETS.slim,parallel:2});
  assert.equal(b.nominalVoltageV,a.nominalVoltageV);
  assert.equal(b.capacityAh,a.capacityAh*2);
  assert.equal(b.energyWh,a.energyWh*2);
  assert.equal(b.count,a.count*2);
});
test("series count must be a positive integer",()=>{
  assert.throws(()=>calculatePack({...PACK_PRESETS.slim,series:1.5}),CalculationError);
  assert.throws(()=>calculatePack({...PACK_PRESETS.slim,series:0}),CalculationError);
});
test("100 Wh advisory uses nominal energy; exact threshold is not over",()=>{
  const base={...PACK_PRESETS.slim,series:4,parallel:1,nominalCellV:4,cellCapacityMah:6250};
  assert.equal(calculatePack(base).energyWh,100);
  assert.equal(calculatePack(base).warnings.includes("above100Wh"),false);
  assert.equal(calculatePack({...base,cellCapacityMah:6260}).warnings.includes("above100Wh"),true);
});
test("duration rounding handles minute rollover",()=>{
  assert.equal(formatDuration(1+59.6/60,"en"),"2h 00m");
  assert.equal(formatDuration(-1,"en"),"—");
});
test("CSV exports BOM and neutralizes formulas in scenario names",()=>{
  const csv=scenariosToCsv([{name:"=1+1",input:PACK_PRESETS.slim}]);
  assert.ok(csv.startsWith("\uFEFF"));
  assert.ok(csv.includes("'=1+1"));
  assert.ok(csv.includes("57.75"));
});