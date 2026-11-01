import test from "node:test";
import assert from "node:assert/strict";
import {estimateChargeCost,estimatePowerBank,estimateChargeTime,calculateRetention,parseWindowsReport,ConsumerInputError} from "../src/consumer.js";
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