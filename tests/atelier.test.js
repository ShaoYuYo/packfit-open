import test from "node:test";
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
import {resolve,dirname} from "node:path";
import {fileURLToPath} from "node:url";
const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const read=(path)=>readFileSync(resolve(root,path),"utf8");

test("warm atelier palette is loaded last and cached for offline use",()=>{
 const html=read("index.html");
 const css=read("assets/atelier.css");
 const worker=read("sw.js");
 assert.ok(html.indexOf("./assets/atelier.css")>html.indexOf("./assets/editorial.css"));
 assert.match(worker,/"\.\/assets\/atelier\.css"/);
 const version=JSON.parse(read("package.json")).version;
 assert.ok(worker.includes(`"packfit-static-v${version}"`),"service worker cache should match package version");
 assert.match(css,/--bg:#f6f3f0/);
 assert.match(css,/--accent:#76515e/);
 assert.match(css,/--accent:#d9aeba/);
});
test("real and demo battery colors share the theme accent",()=>{
 const css=read("assets/atelier.css");
 const app=read("src/app.js");
 assert.match(css,/\.sample-energy span\{background:var\(--accent-strong\)\}/);
 assert.match(css,/\.pf-ring,\.luminous-ring/);
 assert.match(app,/"#76515e"/);
 assert.doesNotMatch(app,/"#7ce4b4"/);
});
test("animation uses only browser-native APIs and works without network access",()=>{
 const ui=read("src/motion-enhance.js");
 const app=read("src/app.js");
 const worker=read("sw.js");
 assert.doesNotMatch(ui,/import\s*\(|cdn\.jsdelivr|https?:\/\//);
 assert.match(ui,/prefers-reduced-motion: reduce/);
 assert.match(ui,/element\.animate/);
 assert.match(ui,/setupMotionLibrary/);
 assert.match(app,/animatePanelEntry\(shown\)/);
 assert.match(app,/animateCalculation\(element\)/);
 assert.match(worker,/"\.\/src\/motion-enhance\.js"/);
});
test("version number, theme color and app stylesheet agree",()=>{
 const pkg=JSON.parse(read("package.json"));
 const html=read("index.html");
 assert.equal(pkg.version,"0.6.4");
 assert.match(html,/VERSION 0\.6\.4/);
 assert.match(html,/name="theme-color" content="#f6f3f0"/);
});
