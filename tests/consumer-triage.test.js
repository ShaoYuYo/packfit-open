import test from "node:test";
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
import {fileURLToPath} from "node:url";
import {resolve,dirname} from "node:path";
import {SYMPTOM_IDS,getSymptomGuide} from "../src/battery-triage.js";
import {CONSUMER_I18N} from "../src/consumer-i18n.js";

const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const read=path=>readFileSync(resolve(root,path),"utf8");

test("five real-world battery symptoms have bilingual, non-diagnostic guidance",()=>{
  assert.equal(SYMPTOM_IDS.length,5);
  for(const id of SYMPTOM_IDS){
    for(const lang of ["zh-TW","en"]){
      const item=getSymptomGuide(lang,id);
      assert.equal(item.id,id);
      assert.ok(item.label.trim().length>=3);
      assert.ok(item.summary.trim().length>=12);
      assert.equal(item.steps.length,3);
      assert.ok(item.steps.every(s=>s.trim().length>=12));
      assert.ok(item.help.trim().length>=12);
      assert.ok(Object.isFrozen(item.steps));
    }
  }
  assert.throws(()=>getSymptomGuide("zh-TW","fake"),RangeError);
  assert.notEqual(getSymptomGuide("zh-TW","short-runtime").label,
    getSymptomGuide("en","short-runtime").label);
});

test("unsafe battery symptoms stop further software-testing or charging",()=>{
  const zh=getSymptomGuide("zh-TW","hot-or-swollen");
  const en=getSymptomGuide("en","hot-or-swollen");
  assert.match(zh.steps.join(" "),/停止使用與充電/);
  assert.match(zh.help,/安全判定/);
  assert.match(en.steps.join(" "),/Stop using or charging/);
  assert.doesNotMatch(en.summary,/percent|%/i);
});

test("every symptom controller label is translated and tied to semantic controls",()=>{
  const html=read("index.html");
  for(const id of ["pf-symptom-title","pf-symptom","pf-symptom-copy",
    "pf-symptom-intro","pf-symptom-result","pf-symptom-summary",
    "pf-symptom-steps","pf-symptom-help","pf-symptom-status"]){
    assert.match(html,new RegExp('id="'+id+'"'));
  }
  for(const id of SYMPTOM_IDS)
    assert.match(html,new RegExp('value="'+id+'"'));
  assert.match(html,/<ol id="pf-symptom-steps"><\/ol>/);
  assert.match(html,/pf-symptom-result"[^>]+aria-live="polite"/);
  const used=[...html.matchAll(/data-pf="([^"]+)"/g)].map(m=>m[1]);
  for(const locale of ["zh-TW","en"])
    assert.deepEqual(used.filter(key=>!(key in CONSUMER_I18N[locale])),[]);
});

test("symptom instructions use text nodes and never remote uploads or HTML injection",()=>{
  const ui=read("src/consumer-ui.js");
  const model=read("src/battery-triage.js");
  assert.match(ui,/getSymptomGuide\(language,selectedSymptom\)/);
  assert.match(ui,/li\.textContent=step/);
  assert.match(ui,/steps\.replaceChildren\(\)/);
  assert.match(ui,/addEventListener\("change",event=>/);
  assert.match(ui,/navigator\.clipboard\.writeText\(symptomClipboardText\(\)\)/);
  assert.doesNotMatch(ui,/innerHTML\s*=/);
  assert.doesNotMatch(model,/\b(fetch|XMLHttpRequest|sendBeacon)\s*\(/);
});

test("editing user-entered Wh invalidates stale percentages before new calculation",()=>{
  const ui=read("src/consumer-ui.js");
  assert.match(ui,/function invalidateHealthResult\(\)/);
  assert.match(ui,/healthResult=null;\s*q\("pf-retention"\)\.hidden=true;/);
  assert.match(ui,/for\(const id of \["pf-design","pf-full"\]\)/);
  assert.match(ui,/q\(id\)\.addEventListener\("input",\(\)=>\{/);
  assert.match(ui,/set\("pf-report-status",t\("pendingRecalc"\)\)/);
  assert.match(ui,/catch\{\s*invalidateHealthResult\(\);\s*set\("pf-report-status",t\("badInput"\)\)/);
});

test("guidance is bundled for offline first use with an updated cache key",()=>{
  const sw=read("sw.js");
  assert.match(sw,/packfit-static-v0\.6\.5/);
  assert.match(sw,/"\.\/src\/battery-triage\.js"/);
  const css=read("assets/workspace.css");
  assert.match(css,/\.pf-next-step-result/);
  assert.match(css,/\.pf-next-step-picker select:focus-visible/);
});
