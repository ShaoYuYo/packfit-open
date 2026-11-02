import test from "node:test";
import assert from "node:assert/strict";
import {estimateChargeCost,estimatePowerBank,estimateChargeTime,calculateRetention,parseWindowsReport,parseWindowsReportRecords,ConsumerInputError} from "../src/consumer.js";
test("charging cost accounts for efficiency, kWh and charge frequency",()=>{
 const c=estimateChargeCost({wh:100,eff:80,price:4,times:30});
 assert.equal(c.gridKwh,.125);assert.equal(c.cost,.5);assert.equal(c.monthly,15);
});
test("power bank converts cell nominal voltage and mAh to Wh",()=>{
 const x=estimatePowerBank({mah:20000,v:3.7,eff:80,targetWh:15});
 assert.equal(x.nominalWh,74);assert.ok(Math.abs(x.deliveredWh-59.2)<1e-8);
 assert.ok(Math.abs(x.charges-59.2/15)<1e-8);
});
test("charging time uses partial charge range and a declared 15% buffer",()=>{
 const x=estimateChargeTime({wh:60,start:20,end:80,watts:30,eff:90});
 assert.ok(Math.abs(x.requiredWh-36)<1e-8);assert.ok(Math.abs(x.hours-36/27*1.15)<1e-8);
});
test("bad values and reversed charging ranges are rejected",()=>{
 assert.throws(()=>estimateChargeTime({wh:50,start:90,end:10,watts:20,eff:90}),ConsumerInputError);
 assert.throws(()=>estimateChargeCost({wh:"",eff:90,price:4,times:10}),ConsumerInputError);
 assert.throws(()=>estimatePowerBank({mah:-1,v:3.7,eff:80,targetWh:15}),ConsumerInputError);
});
test("capacity retention uses design and full-charge capacity, not charge level",()=>{
 assert.equal(calculateRetention(56,49).percent,87.5);
});
test("Windows battery report is parsed as untrusted HTML text without executing it",()=>{
 const html='<table><tr><td><span>DESIGN CAPACITY</span></td><td>56,000 mWh</td></tr>'+
 '<tr><td>FULL CHARGE CAPACITY</td><td>49,000&nbsp;mWh</td></tr>'+
 '<tr><td>CYCLE COUNT</td><td>234</td></tr></table>';
 const r=parseWindowsReport(html);
 assert.equal(r.designWh,56);assert.equal(r.fullWh,49);assert.equal(r.percent,87.5);assert.equal(r.cycles,234);
 assert.throws(()=>parseWindowsReport("<html>unrelated</html>"),/Missing/);
});
test("Windows report ignores capacity history outside installed batteries",()=>{
 const html='<h2><span>Installed batteries</span></h2><table>'+
  '<tr><td>DESIGN CAPACITY</td><td>56,000 mWh</td></tr>'+
  '<tr><td>FULL CHARGE CAPACITY</td><td>49,000 mWh</td></tr></table>'+
  '<h2>Battery capacity history</h2><table>'+
  '<tr><td>FULL CHARGE CAPACITY</td><td>30,000 mWh</td></tr></table>';
 const records=parseWindowsReportRecords(html);
 assert.equal(records.length,1);
 assert.equal(records[0].percent,87.5);
});

test("Windows reports with two installed batteries preserve separate capacity and cycle counts",()=>{
 const report='<h2>Installed batteries</h2><table>'+
  '<tr><td>NAME</td><td>Fictional A</td></tr>'+
  '<tr><td>DESIGN CAPACITY</td><td>56,000 mWh</td></tr>'+
  '<tr><td>FULL CHARGE CAPACITY</td><td>49,000 mWh</td></tr>'+
  '<tr><td>CYCLE COUNT</td><td>234</td></tr></table>'+
  '<table><tr><td>NAME</td><td>Fictional B</td></tr>'+
  '<tr><td>DESIGN CAPACITY</td><td>30 Wh</td></tr>'+
  '<tr><td>FULL CHARGE CAPACITY</td><td>27 Wh</td></tr></table>'+
  '<h2>Battery capacity history</h2><table>'+
  '<tr><td>DESIGN CAPACITY</td><td>999 Wh</td></tr></table>';
 const records=parseWindowsReportRecords(report);
 assert.deepEqual(records.map(r=>r.percent),[87.5,90]);
 assert.deepEqual(records.map(r=>r.cycles),[234,null]);
 assert.throws(()=>parseWindowsReport(report),/Multiple batteries/);
});

test("Chinese heading and localized capacity labels are accepted",()=>{
 const html='<h2>已安裝電池</h2><table>'+
  '<tr><td>設計容量</td><td>62,000 mWh</td></tr>'+
  '<tr><td>完整充電容量</td><td>55,000 mWh</td></tr></table>';
 assert.equal(parseWindowsReport(html).designWh,62);
});

test("Malformed battery report and ambiguous multi-column table are rejected",()=>{
 const ambiguous='<h2>Installed batteries</h2><table>'+
  '<tr><td>DESIGN CAPACITY</td><td>56,000 mWh</td><td>30,000 mWh</td></tr>'+
  '<tr><td>FULL CHARGE CAPACITY</td><td>49,000 mWh</td></tr></table>';
 assert.throws(()=>parseWindowsReportRecords(ambiguous),/multi-column/);
 const invalid='<h2>Installed batteries</h2><table>'+
  '<tr><td>DESIGN CAPACITY</td><td>Unknown</td></tr>'+
  '<tr><td>FULL CHARGE CAPACITY</td><td>49,000 mWh</td></tr></table>';
 assert.throws(()=>parseWindowsReportRecords(invalid),/units/);
 assert.throws(()=>parseWindowsReportRecords("x".repeat(2_500_001)),/size/);
});
