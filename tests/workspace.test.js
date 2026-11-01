import test from "node:test";
import assert from "node:assert/strict";
import {readFileSync,existsSync} from "node:fs";
import {resolve,dirname} from "node:path";
import {fileURLToPath} from "node:url";
import {DEVICE_COPY} from "../src/device-ui.js";
import {readSettings} from "../src/storage.js";
const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const read=(path)=>readFileSync(resolve(root,path),"utf8");
test("every device panel control exists and is reachable by navigation",()=>{
 const html=read("index.html"),ui=read("src/device-ui.js");
 assert.match(html,/data-tab="device"/);
 assert.match(html,/data-panel="device"/);
 const ids=new Set([...html.matchAll(/\bid="([^"]+)"/g)].map(m=>m[1]));
 const refs=[...ui.matchAll(/\$\("([^"]+)"\)/g)].map(m=>m[1]);
 assert.deepEqual([...new Set(refs.filter(id=>!ids.has(id)))],[]);
});
test("device experience is fully bilingual",()=>{
 const html=read("index.html");
 assert.deepEqual(Object.keys(DEVICE_COPY["zh-TW"]).sort(),Object.keys(DEVICE_COPY.en).sort());
 const used=[...html.matchAll(/data-device-i18n="([^"]+)"/g)].map(m=>m[1]);
 assert.deepEqual([...new Set(used.filter(k=>!(k in DEVICE_COPY.en)))],[]);
});
test("theme defaults to OS mode and follows user changes",()=>{
 const html=read("index.html"),app=read("src/app.js"),store=read("src/storage.js");
 assert.match(store,/theme:"system"/);
 assert.match(store,/themeMode:mode/);
 assert.match(html,/prefers-color-scheme: dark/);
 assert.match(app,/systemColor\?\.addEventListener/);
 assert.match(app,/resolveTheme\(settings.theme/);
 assert.match(html,/id="theme-mode"/);
 assert.doesNotMatch(app,/id="theme-toggle"/);
});
test("new layout and worker modules are included in offline precache",()=>{
 const html=read("index.html"),sw=read("sw.js");
 assert.equal((html.match(/href="\.\/assets\/workspace\.css"/g)||[]).length,1);
 for(const path of ["assets/workspace.css","src/theme.js","src/device.js",
   "src/device-ui.js","src/device-benchmark-worker.js"]){
   assert.ok(existsSync(resolve(root,path)),path);
   assert.ok(sw.includes('"./'+path+'"'),path+" absent from cache");
 }
 assert.match(sw,/packfit-static-v0\.6\.0/);
});
test("GPU identification and the benchmark are user-initiated and never uploaded",()=>{
 const ui=read("src/device-ui.js");
 assert.match(ui,/device-probe-gpu"\)\.addEventListener\("click"/);
 assert.match(ui,/device-run-bench"\)\.addEventListener\("click"/);
 assert.doesNotMatch(ui,/fetch\(|sendBeacon|localStorage\./);
 assert.match(read("src/device-benchmark-worker.js"),/self\.postMessage/);
});
