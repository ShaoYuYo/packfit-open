import test from "node:test";
import assert from "node:assert/strict";
import {readFileSync,existsSync} from "node:fs";
import {dirname,resolve} from "node:path";
import {fileURLToPath} from "node:url";
const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const read=(path)=>readFileSync(resolve(root,path),"utf8");

test("manifest offers a scoped offline browser app",()=>{
  const manifest=JSON.parse(read("manifest.webmanifest"));
  assert.equal(manifest.scope,"./");
  assert.ok(manifest.start_url.startsWith("./"));
  assert.equal(manifest.display,"standalone");
  assert.ok(manifest.icons.some((icon)=>existsSync(resolve(root,icon.src))));
});
test("precache asset list is complete and stays within the repository",()=>{
  const worker=read("sw.js");
  const arr=worker.match(/const RESOURCES = \[([\s\S]*?)\];/);
  assert.ok(arr,"Expected a static RESOURCES array");
  const resources=[...arr[1].matchAll(/"(\.[^"]+)"/g)].map(m=>m[1]);
  assert.ok(resources.length>=15);
  for(const asset of resources){
    assert.ok(asset.startsWith("./"),"Use scoped relative assets only: "+asset);
    assert.ok(existsSync(resolve(root,asset)),"Missing offline asset: "+asset);
  }
  assert.ok(resources.includes("./src/scenario-transfer.js"));
  assert.ok(resources.includes("./src/pwa.js"));
  assert.ok(resources.includes("./manifest.webmanifest"));
});
test("page links manifest and app registers offline support",()=>{
  assert.ok(read("index.html").includes('rel="manifest"'));
  assert.ok(read("src/app.js").includes("setupOfflineSupport();"));
  assert.ok(read("src/pwa.js").includes('serviceWorker.register("./sw.js"'));
});
test("offline worker only handles GET and same-origin scoped resources",()=>{
  const worker=read("sw.js");
  assert.match(worker,/request\.method!=="GET"/);
  assert.match(worker,/url\.origin!==self\.location\.origin/);
  assert.match(worker,/url\.href\.startsWith\(self\.registration\.scope\)/);
  assert.match(worker,/request\.mode==="navigate"/);
});