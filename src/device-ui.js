import {normalizeHardwareHints,resourceProfile,graphicsCapabilities} from "./device.js";
import {formatNumber} from "./format.js";
import {parseDxDiagReport} from "./dxdiag.js";

export const DEVICE_COPY={
 "zh-TW":{
  navDevice:"裝置分析",pageTag:"裝置概況 / BROWSER INSIGHTS",pageTitle:"這台電腦的硬體如何？",
  pageIntro:"網站會先讀取瀏覽器允許的基本資料。想看顯示晶片或跑一次測試，再自己按下按鈕。",
  deviceLabel:"目前瀏覽器能讀到的資料",basicLead:"以下不代表 Windows 系統資訊，也不能顯示完整硬體型號。",
  platform:"作業平台",threads:"邏輯執行緒",memory:"概略記憶體",graphics:"圖形 API",
  resourceTitle:"資源概況",resourceIntro:"依瀏覽器提供的執行緒與概略 RAM 分組；屬低信心的參考，不代表 CPU／GPU 跑分。",
  roomy:"多工資源較充裕",typical:"一般使用資源",limited:"瀏覽器回報資源較有限",
  unknown:"目前資料不足，無法判斷",unknownValue:"瀏覽器未提供",lowConfidence:"參考性質 · 低信心",
  graphicsTitle:"深入看顯示晶片",graphicsIntro:"由你決定是否讀取 WebGL 可能提供的 GPU 資訊。某些瀏覽器會隱藏實際型號。",
  probeGpu:"查看 GPU 資訊",gpuName:"瀏覽器回報的顯示晶片",maxTexture:"最大紋理尺寸",webgl:"WebGL 支援",
  gpuPending:"尚未要求辨識",gpuUnavailable:"瀏覽器沒有提供可辨識的顯示晶片名稱",
  webglUnavailable:"此瀏覽器未提供 WebGL",webgpu:"WebGPU",supported:"支援 API",notSupported:"未提供",
  benchmarkTitle:"測一下瀏覽器的運算速度",benchmarkIntro:"按下後才會執行短暫的本機 JavaScript 測試。結果會受到瀏覽器、溫度及背景工作影響，不能與專業 CPU 跑分比較。",
  runBenchmark:"開始本機測試",benchmarkRunning:"測試進行中…",benchmarkResult:"這次運算耗時（中位數）",
  benchmarkIdle:"尚未測試",benchmarkFailed:"測試未能完成；瀏覽器可能限制背景工作或 Worker。",
  benchmarkHelp:"同一台電腦、同一瀏覽器的數字較有參考性；時間越短代表這次完成得越快。",
  privacyTitle:"隱私與限制",privacyText:"裝置資料只在本頁顯示，不會上傳、追蹤或儲存。瀏覽器為保護隱私，可能故意隱藏、簡化或限制 CPU、RAM、GPU 相關資訊。網站不能可靠判定實際遊戲 FPS。",
  importTitle:"想看更完整的 Windows 硬體型號？",importIntro:"使用 Windows 的 DxDiag 匯出報告。檔案只在本機解析，既不上傳也不儲存。",
  importSteps:"按 Win + R，輸入 dxdiag，點「儲存所有資訊」，將文字報告存成 .txt。",
  importButton:"選擇 DxDiag 文字報告",reportCpu:"CPU 型號",reportRam:"記憶體",reportGpu:"顯示晶片",reportOs:"作業系統",
  reportMissing:"尚未匯入",reportDone:"已從本機報告讀取硬體規格",reportError:"無法辨識這份 DxDiag 文字報告。請確認檔案格式。",reportTooLarge:"報告超過 2 MB，無法匯入。",
  refresh:"重新讀取",justNow:"從目前瀏覽器讀取",unitThreads:"執行緒",unitGiB:"GB（概略）"
 },
 en:{
  navDevice:"Device Insights",pageTag:"DEVICE / BROWSER INSIGHTS",pageTitle:"What can this device handle?",
  pageIntro:"We start with basic details the browser makes available. GPU identification and a performance probe only run when you choose.",
  deviceLabel:"Information exposed by this browser",basicLead:"These are browser hints, not a complete Windows hardware inventory.",
  platform:"Platform",threads:"Logical threads",memory:"Approximate memory",graphics:"Graphics API",
  resourceTitle:"Resource overview",resourceIntro:"Based on exposed thread count and approximate RAM. Low-confidence capacity hints, not CPU/GPU benchmark scores.",
  roomy:"More headroom for multitasking",typical:"Typical resource availability",limited:"More limited reported resources",
  unknown:"Not enough information to assess",unknownValue:"Not available",lowConfidence:"Approximate · low confidence",
  graphicsTitle:"Inspect graphics details",graphicsIntro:"You choose whether to query WebGL graphics information. Some browsers hide the real GPU model.",
  probeGpu:"Check GPU details",gpuName:"Reported graphics renderer",maxTexture:"Maximum texture size",webgl:"WebGL support",
  gpuPending:"Not checked",gpuUnavailable:"Browser did not reveal a recognizable GPU name",
  webglUnavailable:"WebGL unavailable",webgpu:"WebGPU",supported:"API supported",notSupported:"Not available",
  benchmarkTitle:"Try a browser computation probe",benchmarkIntro:"A short, local JavaScript workload only runs after you click. Results vary with browser, temperature and background activity; this is not a standardized CPU score.",
  runBenchmark:"Run local test",benchmarkRunning:"Running…",benchmarkResult:"Time for this run (median)",
  benchmarkIdle:"Not tested",benchmarkFailed:"Test could not finish; the browser may restrict workers or background tasks.",
  benchmarkHelp:"Comparing runs on the same device/browser is more useful. Lower milliseconds means faster completion in this specific test.",
  privacyTitle:"Privacy & limitations",privacyText:"Hardware hints stay on this page: no upload, tracking or persistent storage. Browsers may deliberately limit, approximate or conceal CPU/RAM/GPU information. No reliable game FPS prediction is possible.",
  importTitle:"Want more exact Windows hardware names?",importIntro:"Export a Windows DxDiag report. The file is parsed locally and never uploaded or stored.",
  importSteps:"Press Win + R, type dxdiag, select Save All Information and save the text report as a .txt file.",
  importButton:"Choose DxDiag text report",reportCpu:"CPU model",reportRam:"Memory",reportGpu:"Graphics adapters",reportOs:"Operating system",
  reportMissing:"Not imported",reportDone:"Hardware details read locally",reportError:"Could not read this DxDiag text report. Check the file format.",reportTooLarge:"Report exceeds the 2 MB limit.",
  refresh:"Read again",justNow:"Browser-provided snapshot",unitThreads:"threads",unitGiB:"GB (coarse)"
 }
};
let locale="zh-TW";
let hints=null,graphics=null,probe=null,diagnostic=null,reportMessage="";
const $=id=>document.getElementById(id);
const t=key=>DEVICE_COPY[locale][key]||key;
function show(id,value){$(id).textContent=String(value);}
function rerender(){
  if(!hints)return;
  show("device-platform",hints.platform||t("unknownValue"));
  show("device-threads",hints.threads===null?t("unknownValue"):hints.threads+" "+t("unitThreads"));
  show("device-memory",hints.memory===null?t("unknownValue"):hints.memory+" "+t("unitGiB"));
  show("device-webgpu",hints.webgpu?t("supported"):t("notSupported"));
  const resource=resourceProfile(hints);
  show("device-profile",t(resource.level));
  show("device-confidence",t("lowConfidence"));
  show("device-renderer",graphics?(graphics.renderer||t("gpuUnavailable")):t("gpuPending"));
  show("device-webgl",graphics?(graphics.webgl||t("webglUnavailable")):t("gpuPending"));
  show("device-texture",graphics&&graphics.maxTexture?formatNumber(graphics.maxTexture,locale,0)+" px":t("unknownValue"));
  show("device-benchmark",probe?formatNumber(probe.medianMs,locale,1)+" ms":t("benchmarkIdle"));
  show("device-report-cpu",diagnostic?.cpu||t("reportMissing"));
  show("device-report-ram",diagnostic?.ram||t("reportMissing"));
  show("device-report-gpu",diagnostic?.gpus?.join(" / ")||t("reportMissing"));
  show("device-report-os",diagnostic?.os||t("reportMissing"));
  show("device-report-status",reportMessage?t(reportMessage):"");
}
export function translateDevice(language){
  locale=language==="en"?"en":"zh-TW";
  document.querySelectorAll("[data-device-i18n]").forEach(node=>{
    node.textContent=t(node.dataset.deviceI18n);
  });
  rerender();
}
function refreshHints(){
  hints=normalizeHardwareHints(navigator);
  rerender();
}
function probeGraphics(){
  const canvas=document.createElement("canvas");
  canvas.width=1;canvas.height=1;
  graphics=graphicsCapabilities(canvas);
  rerender();
}
function runProbe(){
  const button=$("device-run-bench");
  button.disabled=true;
  show("device-benchmark",t("benchmarkRunning"));
  let worker;
  try{worker=new Worker(new URL("./device-benchmark-worker.js",import.meta.url),{type:"module"});}
  catch{
    button.disabled=false;show("device-benchmark",t("benchmarkFailed"));return;
  }
  let finished=false;
  const cleanup=()=>{worker.terminate();button.disabled=false;clearTimeout(timeout);};
  const timeout=setTimeout(()=>{
    if(finished)return;
    finished=true;cleanup();show("device-benchmark",t("benchmarkFailed"));
  },7000);
  worker.onmessage=(event)=>{
    if(finished)return;
    finished=true;
    const ms=Number(event.data?.medianMs);
    probe=Number.isFinite(ms)&&ms>=0&&ms<7000?{medianMs:ms}:null;
    cleanup();
    if(probe)rerender();else show("device-benchmark",t("benchmarkFailed"));
  };
  worker.onerror=()=>{
    if(finished)return;
    finished=true;cleanup();show("device-benchmark",t("benchmarkFailed"));
  };
  worker.postMessage({start:true});
}
export function initializeDevice(getLanguage){
  $("device-refresh").addEventListener("click",refreshHints);
  $("device-probe-gpu").addEventListener("click",probeGraphics);
  $("device-run-bench").addEventListener("click",runProbe);
  $("device-report-file").addEventListener("change",async(event)=>{
    const input=event.target,file=input.files?.[0];
    if(!file)return;
    if(file.size>2_000_000){
      reportMessage="reportTooLarge";rerender();input.value="";return;
    }
    try{
      diagnostic=parseDxDiagReport(await file.text());
      reportMessage="reportDone";
    }catch{
      reportMessage="reportError";
    }finally{
      input.value="";
      rerender();
    }
  });
  refreshHints();
  translateDevice(getLanguage());
}
