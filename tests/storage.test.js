import test from "node:test";
import assert from "node:assert/strict";
import {PACK_PRESETS} from "../src/presets.js";
import {readScenarios,writeScenarios,readSettings,writeSettings,MAX_SCENARIOS,validateScenario} from "../src/storage.js";

const store = new Map();
globalThis.localStorage = {
  getItem(key) { return store.has(key) ? store.get(key) : null; },
  setItem(key,value) { store.set(key,String(value)); },
  removeItem(key) { store.delete(key); },
  clear() { store.clear(); }
};
const valid = (id) => ({id:String(id),name:"Scenario "+id,input:{...PACK_PRESETS.slim}});

test("empty or corrupted storage returns safe defaults",()=>{
  store.clear();
  assert.deepEqual(readScenarios(),[]);
  assert.deepEqual(readSettings(),{language:"zh-TW",theme:"dark"});
  store.set("packfit:scenarios:v1","{bad-json");
  assert.deepEqual(readScenarios(),[]);
  store.set("packfit:settings:v1","{bad-json");
  assert.deepEqual(readSettings(),{language:"zh-TW",theme:"dark"});
});
test("saved scenarios round trip and do not leak arbitrary fields",()=>{
  store.clear();
  assert.equal(writeScenarios([valid("1"),valid("2")]),true);
  const rows=readScenarios();
  assert.equal(rows.length,2);
  assert.equal(rows[0].input.series,3);
  assert.equal(rows[1].name,"Scenario 2");
});
test("invalid scenarios cannot be written; malicious stored rows are filtered",()=>{
  store.clear();
  const invalid={...valid("bad"),input:{...PACK_PRESETS.slim,series:0}};
  assert.equal(validateScenario(invalid),false);
  assert.equal(writeScenarios([invalid]),false);
  store.set("packfit:scenarios:v1",JSON.stringify([valid("1"),invalid,valid("2")]));
  assert.equal(readScenarios().length,2);
});
test("scenario limit is enforced on writes and reads",()=>{
  store.clear();
  const many=Array.from({length:MAX_SCENARIOS+2},(_,n)=>valid(n));
  assert.equal(writeScenarios(many),false);
  store.set("packfit:scenarios:v1",JSON.stringify(many));
  assert.equal(readScenarios().length,MAX_SCENARIOS);
});
test("settings preserve supported language and theme only",()=>{
  store.clear();
  assert.equal(writeSettings({language:"en",theme:"light"}),true);
  assert.deepEqual(readSettings(),{language:"en",theme:"light"});
  store.set("packfit:settings:v1",JSON.stringify({language:"invalid",theme:"invalid"}));
  assert.deepEqual(readSettings(),{language:"zh-TW",theme:"dark"});
});