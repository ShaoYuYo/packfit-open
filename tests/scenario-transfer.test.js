import test from "node:test";
import assert from "node:assert/strict";
import {PACK_PRESETS} from "../src/presets.js";
import {parseScenarioFile,serializeScenarios,ScenarioFileError,MAX_JSON_CHARS} from "../src/scenario-transfer.js";

const valid=()=>({id:"test",name:"  Public sample ",input:{...PACK_PRESETS.slim}});

test("JSON roundtrip preserves numeric input and readable name",()=>{
  const text=serializeScenarios([valid()]);
  const parsed=parseScenarioFile(text);
  assert.equal(parsed.length,1);
  assert.equal(parsed[0].name,"Public sample");
  assert.deepEqual(parsed[0].input,PACK_PRESETS.slim);
  assert.equal(typeof parsed[0].input.nominalCellV,"number");
  assert.ok(text.includes('"version": 1'));
});
test("rejects files with unknown format or version",()=>{
  assert.throws(()=>parseScenarioFile('{"format":"untrusted","version":1,"scenarios":[]}'),ScenarioFileError);
  assert.throws(()=>parseScenarioFile('{"format":"packfit.scenarios","version":2,"scenarios":[]}'),ScenarioFileError);
});
test("rejects missing fields, out-of-range numeric values, and string coercion",()=>{
  const payload=JSON.parse(serializeScenarios([valid()]));
  payload.scenarios[0].input.series="3";
  assert.throws(()=>parseScenarioFile(JSON.stringify(payload)),ScenarioFileError);
  payload.scenarios[0].input.series=33;
  assert.throws(()=>parseScenarioFile(JSON.stringify(payload)),ScenarioFileError);
  delete payload.scenarios[0].input.series;
  assert.throws(()=>parseScenarioFile(JSON.stringify(payload)),ScenarioFileError);
});
test("rejects too many or empty scenarios",()=>{
  assert.throws(()=>serializeScenarios([]),ScenarioFileError);
  assert.throws(()=>serializeScenarios(Array(5).fill(valid())),ScenarioFileError);
});
test("rejects huge or corrupted JSON",()=>{
  assert.throws(()=>parseScenarioFile("x".repeat(MAX_JSON_CHARS+1)),ScenarioFileError);
  assert.throws(()=>parseScenarioFile("not-json"),ScenarioFileError);
});
test("unknown fields are discarded instead of being persisted",()=>{
  const payload=JSON.parse(serializeScenarios([valid()]));
  payload.scenarios[0].input.untrusted="something";
  payload.scenarios[0].extra="unsafe";
  const result=parseScenarioFile(JSON.stringify(payload));
  assert.equal(Object.hasOwn(result[0],"extra"),false);
  assert.equal(Object.hasOwn(result[0].input,"untrusted"),false);
});

test("the downloadable public example parses as a valid import file",async()=>{
  const {readFileSync}=await import("node:fs");
  const {resolve,dirname}=await import("node:path");
  const {fileURLToPath}=await import("node:url");
  const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
  const json=readFileSync(resolve(root,"examples/sample-scenarios.json"),"utf8");
  const scenarios=parseScenarioFile(json);
  assert.equal(scenarios.length,2);
  assert.ok(scenarios.every((s)=>s.name.toLowerCase().includes("illustrative")));
});
