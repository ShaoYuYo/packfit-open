import test from "node:test";
import assert from "node:assert/strict";
import {normalizeHardwareHints,resourceProfile,safeGpuLabel,graphicsCapabilities} from "../src/device.js";
test("missing or protected browser hardware values stay unknown",()=>{
  const basic=normalizeHardwareHints({});
  assert.equal(basic.threads,null);
  assert.equal(basic.memory,null);
  assert.equal(resourceProfile(basic).level,"unknown");
});
test("hardware concurrency and RAM are treated as coarse hints, not CPU identity",()=>{
  const h=normalizeHardwareHints({hardwareConcurrency:12,deviceMemory:8,
    userAgentData:{platform:"Windows"},gpu:{}});
  assert.deepEqual(h,{threads:12,memory:8,platform:"Windows",webgpu:true});
  assert.deepEqual(resourceProfile(h),{level:"roomy",confidence:"low"});
});
test("lower resource hints are not claimed to be tested benchmarks",()=>{
  assert.equal(resourceProfile({threads:4,memory:4}).level,"typical");
  assert.equal(resourceProfile({threads:2,memory:2}).level,"limited");
  assert.equal(resourceProfile({threads:8,memory:null}).level,"unknown");
});
test("GPU labels are sanitized before rendering to the DOM",()=>{
  assert.equal(safeGpuLabel("Example GPU\u0000\n"),"Example GPU");
  assert.equal(safeGpuLabel(null),"");
  assert.equal(safeGpuLabel("a".repeat(200)).length,120);
});
test("lack of canvas support does not invent a GPU",()=>{
  assert.deepEqual(graphicsCapabilities(null),{webgl:null,renderer:"",maxTexture:null});
});
