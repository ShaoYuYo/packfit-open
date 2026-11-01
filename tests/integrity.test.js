import test from "node:test";
import assert from "node:assert/strict";
import {readFileSync,existsSync} from "node:fs";
import {fileURLToPath} from "node:url";
import {resolve,dirname} from "node:path";
import {DICTIONARY} from "../src/i18n.js";
import {CONSUMER_I18N} from "../src/consumer-i18n.js";
import {PLANNER_TRANSLATIONS} from "../src/planner-ui.js";

const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const read=(path)=>readFileSync(resolve(root,path),"utf8");
const html=read("index.html");
const app=read("src/app.js");

test("all directly referenced IDs exist in the markup",()=>{
  const ids=new Set([...html.matchAll(/\bid="([^"]+)"/g)].map((m)=>m[1]));
  const referenced=[...app.matchAll(/\b(?:el|setText)\("([^"]+)"\)/g)].map((m)=>m[1]);
  const missing=[...new Set(referenced.filter((id)=>!ids.has(id)))];
  assert.deepEqual(missing,[]);
});
test("traditional Chinese and English have translation parity",()=>{
  const a=Object.keys(DICTIONARY["zh-TW"]).sort();
  const b=Object.keys(DICTIONARY.en).sort();
  assert.deepEqual(a,b);
  const used=[...html.matchAll(/data-i18n="([^"]+)"/g),...app.matchAll(/\bt\("([^"]+)"\)/g)].map((m)=>m[1]);
  assert.deepEqual([...new Set(used.filter((key)=>!(key in DICTIONARY.en)))],[]);
});
test("all relative script modules and stylesheet files exist",()=>{
  const scripts=[...app.matchAll(/from "(\.[^"]+)"/g)].map((m)=>m[1]);
  scripts.forEach((path)=>assert.ok(existsSync(resolve(root,"src",path)),path));
  assert.ok(existsSync(resolve(root,"styles.css")));
  assert.ok(existsSync(resolve(root,"assets/logo.svg")));
});
test("no runtime CDN JavaScript or remote telemetry endpoint",()=>{
  assert.doesNotMatch(html,/<script[^>]+src="https?:\/\//i);
  assert.doesNotMatch(app,/\bfetch\s*\(|\bsendBeacon\s*\(/);
});
test("consumer workspace translations and reference keys are complete",()=>{
  assert.deepEqual(Object.keys(CONSUMER_I18N["zh-TW"]).sort(),Object.keys(CONSUMER_I18N.en).sort());
  const used=[...html.matchAll(/data-pf="([^"]+)"/g)].map((m)=>m[1]);
  const missing=[...new Set(used.filter((key)=>!(key in CONSUMER_I18N.en)))];
  assert.deepEqual(missing,[]);
});
test("consumer routes have matching navigation controls and panels",()=>{
  for(const route of ["health","everyday"]){
    assert.ok(html.includes('data-tab="'+route+'"'));
    assert.ok(html.includes('data-panel="'+route+'"'));
  }
});
