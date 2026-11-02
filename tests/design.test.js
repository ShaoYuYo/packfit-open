import test from "node:test";
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
import {fileURLToPath} from "node:url";
import {dirname,resolve} from "node:path";
import {DICTIONARY} from "../src/i18n.js";

const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const read=(file)=>readFileSync(resolve(root,file),"utf8");

test("the refined stylesheet is referenced and cached offline",()=>{
  const html=read("index.html");
  const worker=read("sw.js");
  const stylesheet=read("assets/editorial.css");
  assert.match(html,/href="\.\/assets\/editorial\.css"/);
  assert.match(worker,/"\.\/assets\/editorial\.css"/);
  assert.match(stylesheet,/prefers-reduced-motion:reduce/);
  assert.match(stylesheet,/\.sample-surface/);
});

test("user-facing hero buttons correspond to real workspaces",()=>{
  const html=read("index.html");
  for(const route of ["health","everyday"]){
    assert.match(html,new RegExp('data-goto="'+route+'"'));
    assert.match(html,new RegExp('data-panel="'+route+'"'));
  }
  assert.match(html,/sample-status" data-i18n="sampleDemo"/);
});

test("design copy keys exist in both supported languages",()=>{
  const expected=[
    "brandCaption","sampleDemo","sampleCapacity","sampleType",
    "ctaBattery","ctaEveryday","heroNote","introEyebrow",
    "plannerEyebrow","modelLabel"
  ];
  for(const lang of ["zh-TW","en"]){
    for(const key of expected)assert.ok(DICTIONARY[lang][key],lang+" missing "+key);
  }
});

test("system font stack is readable offline without remote font stylesheets",()=>{
  const css=read("styles.css");
  const editorial=read("assets/editorial.css");
  assert.match(css,/font-family:system-ui/);
  assert.match(css,/Segoe UI Variable Text/);
  assert.match(css,/Microsoft JhengHei UI/);
  assert.doesNotMatch(css,/@import\s+url\s*\(\s*["']?https?:/i);
  assert.match(editorial,/PingFang TC/);
  assert.match(editorial,/Microsoft JhengHei/);
  assert.match(editorial,/system-ui/);
});

test("continuous decorative animations are disabled in the editorial layer",()=>{
  const css=read("assets/editorial.css");
  assert.match(css,/\.float-chip/);
  assert.match(css,/animation:none!important/);
  assert.match(css,/@media\(prefers-reduced-motion:reduce\)/);
});