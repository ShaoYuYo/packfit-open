import {calculatePack} from "./calc.js";
function cell(value) {
  const s = String(value == null ? "" : value);
  // Prevent spreadsheet formula injection when opening the download in Excel.
  const safe = /^[\s]*[=+\-@\t\r]/.test(s) ? "'" + s : s;
  return '"' + safe.replace(/"/g, '""') + '"';
}
export function scenariosToCsv(scenarios) {
  const fields = ["Name","Series S","Parallel P","Cell nominal V","Cell mAh","Cell mass g","Overhead mass g",
    "X mm","Y mm","Z mm","Load W","Nominal pack V","Pack Ah","Nominal Wh","Mass g","Volume L","Wh/kg","Wh/L","Ideal runtime h"];
  const rows = scenarios.map(({name,input}) => {
    const result = calculatePack(input);
    return [name,result.series,result.parallel,result.nominalCellV,result.cellCapacityMah,result.cellMassG,
      result.overheadMassG,result.xMm,result.yMm,result.zMm,result.loadW,result.nominalVoltageV,result.capacityAh,
      result.energyWh,result.totalMassG,result.envelopeVolumeL,result.gravimetricWhKg,result.volumetricWhL,result.idealRuntimeHours];
  });
  return "\uFEFF" + [fields,...rows].map((r)=>r.map(cell).join(",")).join("\r\n") + "\r\n";
}
export function downloadCsv(content, filename) {
  const url = URL.createObjectURL(new Blob([content],{type:"text/csv;charset=utf-8"}));
  const link = document.createElement("a");
  link.href = url; link.download = filename; document.body.append(link); link.click(); link.remove();
  setTimeout(()=>URL.revokeObjectURL(url),1000);
}