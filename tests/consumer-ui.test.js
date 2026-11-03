import test from "node:test";
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
import {fileURLToPath} from "node:url";
import {dirname,resolve} from "node:path";
import {CONSUMER_I18N} from "../src/consumer-i18n.js";
const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const read=path=>readFileSync(resolve(root,path),"utf8");

test("HTML offers an explicit installed-battery selector with a status label",()=>{
 const html=read("index.html");
 assert.match(html,/id="pf-battery-selector" hidden/);
 assert.match(html,/id="pf-battery-choice"/);
 assert.match(html,/data-pf="chooseBattery"/);
 assert.match(html,/id="pf-report-status"[^>]*aria-live="polite"/);
});

test("consumer battery selection and failure cleanup are connected",()=>{
 const ui=read("src/consumer-ui.js");
 assert.match(ui,/parseWindowsReportRecords\(await file\.text\(\)\)/);
 assert.match(ui,/selectReportBattery\(Number\(event\.target\.value\)\)/);
 assert.match(ui,/function clearHealthOnImportError\(/);
 assert.match(ui,/if\(file\.size>2000000\)\{clearHealthOnImportError\(\)/);
 assert.match(ui,/function invalidateHealthResult\(\)\{\s*healthResult=null;/);
 assert.match(ui,/function clearHealthOnImportError\(\)\{\s*healthRecords=\[\];\s*invalidateHealthResult\(\);/);
 assert.match(ui,/q\("pf-retention"\)\.hidden=true/);
});

test("battery chooser is bilingual and hidden state overrides layout",()=>{
 for(const locale of ["zh-TW","en"]){
   for(const key of ["chooseBattery","batteryNumber","batteryCount"])
     assert.equal(typeof CONSUMER_I18N[locale][key],"string");
 }
 assert.match(read("assets/workspace.css"),/\[hidden\]\{display:none!important\}/);
});
