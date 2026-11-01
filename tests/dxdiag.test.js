import test from "node:test";
import assert from "node:assert/strict";
import {parseDxDiagReport,cleanDiagnosticField} from "../src/dxdiag.js";
const fixture=`------------------
System Information
------------------
Computer Name: REDACTED-PC
Operating System: Windows 11 Home 64-bit
Processor: Example CPU 8-Core (16 CPUs)
Memory: 16384MB RAM
---------------
Display Devices
---------------
Card name: Example Integrated Graphics
Dedicated Memory: 128 MB
Card name: Example Discrete GPU
Dedicated Memory: 4096 MB
`;
test("extracts CPU, RAM, OS and multiple GPUs without computer name",()=>{
 const r=parseDxDiagReport(fixture);
 assert.equal(r.cpu,"Example CPU 8-Core (16 CPUs)");
 assert.equal(r.ram,"16384MB RAM");
 assert.equal(r.os,"Windows 11 Home 64-bit");
 assert.deepEqual(r.gpus,["Example Integrated Graphics","Example Discrete GPU"]);
 assert.equal("computerName" in r,false);
});
test("rejects unrelated or excessively long diagnostic files",()=>{
 assert.throws(()=>parseDxDiagReport("hello world"),/format/);
 assert.throws(()=>parseDxDiagReport("x".repeat(2_000_001)),/size/);
});
test("removes control characters and caps untrusted fields",()=>{
 assert.equal(cleanDiagnosticField("Sample\u0000\n Graphics"),"Sample Graphics");
 assert.equal(cleanDiagnosticField("x".repeat(300)).length,150);
});
test("can read common Chinese field names",()=>{
 const r=parseDxDiagReport("處理器：示範處理器\n記憶體：8192MB RAM\n顯示卡名稱：示範顯示晶片");
 assert.equal(r.cpu,"示範處理器");
 assert.equal(r.gpus[0],"示範顯示晶片");
});