import test from "node:test";
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
import {resolve,dirname} from "node:path";
import {fileURLToPath} from "node:url";

const root=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const palette=readFileSync(resolve(root,"assets/atelier.css"),"utf8");

function luminance(hex){
  const rgb=hex.replace("#","").match(/.{2}/g).map((pair)=>parseInt(pair,16)/255);
  const [r,g,b]=rgb.map((value)=>value<=0.04045?value/12.92:((value+0.055)/1.055)**2.4);
  return r*.2126+g*.7152+b*.0722;
}
function ratio(left,right){
  const a=luminance(left),b=luminance(right);
  return (Math.max(a,b)+.05)/(Math.min(a,b)+.05);
}
test("main interface uses distinct, restrained light/dark palettes",()=>{
  for(const value of ["#f6f3f0","#302d32","#76515e","#b39372","#19171a","#d9aeba"]){
    assert.ok(palette.includes(value),value+" missing");
  }
  assert.match(palette,/body\[data-theme="light"\]/);
  assert.match(palette,/body\[data-theme="dark"\]/);
});
test("primary text and action labels meet 4.5:1 contrast in intended themes",()=>{
  const pairs=[
    ["#302d32","#f6f3f0"],
    ["#6e676b","#fffdfb"],
    ["#76515e","#fffdfb"],
    ["#ffffff","#76515e"],
    ["#f4eeed","#19171a"],
    ["#bdb0b0","#262226"],
    ["#d9aeba","#262226"],
    ["#261a20","#d9aeba"]
  ];
  for(const [foreground,background] of pairs){
    assert.ok(ratio(foreground,background)>=4.5,
      foreground+" on "+background+" insufficient contrast");
  }
});
test("reduced motion, high contrast and status text alternatives remain available",()=>{
  assert.match(palette,/prefers-reduced-motion:reduce/);
  assert.match(palette,/prefers-contrast:more/);
  assert.match(palette,/Status feedback should not be communicated by saturation alone/);
});
