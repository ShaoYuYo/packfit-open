import test from "node:test";
import assert from "node:assert/strict";
import {normalizeTheme,resolveTheme,nextTheme} from "../src/theme.js";
test("system mode respects OS dark and light preferences",()=>{
  assert.equal(resolveTheme("system",true),"dark");
  assert.equal(resolveTheme("system",false),"light");
});
test("explicit override always wins",()=>{
  assert.equal(resolveTheme("light",true),"light");
  assert.equal(resolveTheme("dark",false),"dark");
});
test("unsupported theme values safely fall back to automatic",()=>{
  assert.equal(normalizeTheme(undefined),"system");
  assert.equal(normalizeTheme("invalid"),"system");
  assert.equal(resolveTheme("invalid",true),"dark");
});
test("theme cycle includes explicit return to automatic mode",()=>{
  assert.equal(nextTheme("system"),"light");
  assert.equal(nextTheme("light"),"dark");
  assert.equal(nextTheme("dark"),"system");
});
