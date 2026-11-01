import test from "node:test";
import assert from "node:assert/strict";
import {estimateDayPlan} from "../src/planner.js";
import {ConsumerInputError} from "../src/consumer.js";
const base={capacityWh:56,startPct:80,reservePct:10,
  tasks:[{hours:2,watts:10},{hours:1,watts:14},{hours:0,watts:45}]};
test("energy is summed from duration times average load",()=>{
 const r=estimateDayPlan(base);
 assert.equal(r.totalWh,34);
 assert.ok(Math.abs(r.usableWh-39.2)<1e-10);
 assert.ok(Math.abs(r.remainingWh-10.8)<1e-10);
 assert.ok(Math.abs(r.endingPct-(10.8/56*100))<1e-10);
 assert.equal(r.fits,true);
});
test("a gaming task shows explicit shortfall without negative SOC",()=>{
 const r=estimateDayPlan({...base,tasks:[...base.tasks,{hours:1,watts:45}]});
 assert.equal(r.fits,false);
 assert.ok(r.shortfallWh>0);
 assert.ok(r.endingPct>=0);
});
test("bad ranges and missing work segments are rejected",()=>{
 assert.throws(()=>estimateDayPlan({...base,startPct:-1}),ConsumerInputError);
 assert.throws(()=>estimateDayPlan({...base,tasks:[]}),ConsumerInputError);
 assert.throws(()=>estimateDayPlan({...base,tasks:[{hours:1,watts:"bad"}]}),ConsumerInputError);
});
test("starting below reserve gives no usable energy",()=>{
 const r=estimateDayPlan({...base,startPct:5,reservePct:10,
   tasks:[{hours:1,watts:8}]});
 assert.equal(r.usableWh,0);
 assert.equal(r.shortfallWh,8);
 assert.equal(r.fits,false);
});