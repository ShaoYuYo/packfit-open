import {calculateRuntime, calculatePack, getPackInputKeys} from "./calc.js";
import {formatNumber,formatDuration} from "./format.js";
import {PACK_PRESETS,DEVICE_PRESETS} from "./presets.js";
import {readSettings,writeSettings,readScenarios,writeScenarios,MAX_SCENARIOS} from "./storage.js";
import {scenariosToCsv,downloadCsv} from "./csv.js";
import {serializeScenarios,parseScenarioFile,MAX_JSON_CHARS} from "./scenario-transfer.js";
import {setupOfflineSupport} from "./pwa.js";
import {animatePanelEntry,animateCalculation,setupMotionLibrary} from "./motion-enhance.js";
import {initializePlanner,setPlanLocale} from "./planner-ui.js";
import {translate} from "./i18n.js";
import {initializeConsumer,translateConsumer,consumerSectionTitle} from "./consumer-ui.js";
import {initializeDevice,translateDevice} from "./device-ui.js";
import {resolveTheme,normalizeTheme} from "./theme.js";

const el = (id) => document.getElementById(id);
const $ = (selector) => document.querySelector(selector);
const $$ = (selector) => [...document.querySelectorAll(selector)];
const settings = readSettings();
const systemColor = window.matchMedia?.("(prefers-color-scheme: dark)");
let scenarios = readScenarios();
let packResult = null;
let currentPackInput = null;
const packIds = {
  series:"pack-series",parallel:"pack-parallel",nominalCellV:"pack-cell-v",cellCapacityMah:"pack-cell-mah",
  cellMassG:"pack-cell-mass",overheadMassG:"pack-overhead",xMm:"pack-x",yMm:"pack-y",zMm:"pack-z",loadW:"pack-power"
};
const swatches = ["#76515e","#b39372","#666169","#a78590"];
const t = (key) => translate(settings.language,key);
const num = (value,digits=1) => formatNumber(value,settings.language,digits);
const dur = (value) => formatDuration(value,settings.language);
const metricMotionIds = new Set([
  "runtime-display","usable-wh","pack-energy","pack-voltage",
  "pack-whkg","pack-whl","pack-mass","pack-runtime"
]);
function setText(id,text) {
  const element=el(id);
  const value=String(text);
  if(element.textContent===value)return;
  element.textContent=value;
  if(metricMotionIds.has(id))animateCalculation(element);
}
function getInputs() { return Object.fromEntries(Object.entries(packIds).map(([key,id]) => [key,el(id).value])); }
function clearNumbers(ids) { ids.forEach((id)=>setText(id,"—")); }
function renderRuntime() {
  try {
    const calc = calculateRuntime({capacityWh:el("runtime-wh").value,powerW:el("runtime-watts").value,
      reservePct:el("runtime-reserve").value,efficiencyPct:el("runtime-efficiency").value});
    el("runtime-error").hidden = true;
    setText("runtime-display",dur(calc.hours));
    setText("usable-wh",num(calc.usableWh,1));
    setText("usable-energy",num(calc.usableWh,2)+" Wh");
    setText("rated-energy",num(calc.capacityWh,2)+" Wh");
    const pct = Math.round(calc.usableFraction * 10000) / 100;
    el("energy-track").style.width = pct+"%";
    el("runtime-ring").style.background = "conic-gradient(var(--accent-strong) 0 "+pct+"%, var(--track) "+pct+"% 100%)";
  } catch {
    el("runtime-error").hidden = false;
    el("runtime-error").textContent = t("invalidInput");
    clearNumbers(["runtime-display","usable-wh","usable-energy","rated-energy"]);
    el("energy-track").style.width = "0%";
    el("runtime-ring").style.background = "var(--track)";
  }
}
function applyPack(input) {
  for (const key of getPackInputKeys()) el(packIds[key]).value = input[key];
  renderPack();
}
function showPackResult(result) {
  setText("pack-energy",num(result.energyWh,2));
  setText("pack-voltage",num(result.nominalVoltageV,2));
  setText("pack-capacity-display",num(result.capacityAh,2)+" Ah");
  setText("pack-cells-display",num(result.count,0)+" "+(settings.language==="en"?"cells":"顆電芯"));
  setText("pack-whkg",num(result.gravimetricWhKg,1));
  setText("pack-whl",num(result.volumetricWhL,1));
  setText("pack-mass",num(result.totalMassG,1)+" g");
  setText("pack-volume",num(result.envelopeVolumeL,4)+" L");
  setText("pack-runtime",dur(result.idealRuntimeHours));
  setText("pack-count",num(result.count,0));
  setText("pack-topology",result.series+"S"+result.parallel+"P");
  const warnings = el("pack-warnings");
  warnings.replaceChildren();
  const codes = result.warnings.length ? result.warnings : ["noWarnings"];
  for (const code of codes) {
    const item = document.createElement("div");
    item.className = code==="noWarnings" ? "success" : "warning";
    item.textContent = (code==="noWarnings"?"✓ ":"! ") + t(code);
    warnings.append(item);
  }
  const diagram = el("cell-diagram");
  diagram.replaceChildren();
  const visibleRows = Math.min(result.parallel,4);
  const visibleCols = Math.min(result.series,9);
  for (let row=0; row<visibleRows; row++) {
    const line = document.createElement("div");
    line.className = "cell-row";
    for(let col=0;col<visibleCols;col++) {
      const cell = document.createElement("span"); cell.className = "cell-unit";
      cell.textContent = "+";
      line.append(cell);
    }
    diagram.append(line);
  }
  if (result.parallel>visibleRows || result.series>visibleCols) {
    const note = document.createElement("div");
    note.className = "diagram-more";
    note.textContent = result.series+"S × "+result.parallel+"P ("+result.count+" "+(settings.language==="en"?"cells":"顆電芯")+")";
    diagram.append(note);
  }
}
function renderPack() {
  try {
    const input = getInputs();
    const result = calculatePack(input);
    packResult = result;
    currentPackInput = Object.fromEntries(getPackInputKeys().map((key)=>[key,Number(input[key])]));
    el("pack-error").hidden = true;
    showPackResult(result);
  } catch {
    packResult = null; currentPackInput = null;
    el("pack-error").hidden = false; el("pack-error").textContent = t("invalidInput");
    clearNumbers(["pack-energy","pack-voltage","pack-capacity-display","pack-cells-display","pack-whkg","pack-whl",
      "pack-mass","pack-volume","pack-runtime","pack-count"]);
    setText("pack-topology","—");
    el("pack-warnings").replaceChildren();
    el("cell-diagram").replaceChildren();
  }
  el("save-scenario").disabled = !packResult;
  el("export-single").disabled = !packResult;
}
function panelTitle(key) { return key==="health"||key==="everyday" ? consumerSectionTitle(key,settings.language) : t({overview:"navOverview",device:"navDevice",studio:"navStudio",compare:"navCompare",method:"navMethod"}[key]); }
function setPanel(panel) {
  const allowed = ["overview","health","device","everyday","studio","compare","method"];
  const active = allowed.includes(panel) ? panel : "overview";
  for (const button of $$("[data-tab]")) {
    const chosen = button.dataset.tab===active;
    button.classList.toggle("is-active",chosen);
    button.setAttribute("aria-selected",String(chosen));
    if(chosen) button.setAttribute("aria-current","page");
    else button.removeAttribute("aria-current");
  }
  for (const section of $$("[data-panel]")) section.hidden = section.dataset.panel!==active;
  const shown = document.querySelector(`[data-panel="${active}"]`);
  if(shown)animatePanelEntry(shown);
  setText("current-section",panelTitle(active));
  if (location.hash!=="#"+active) history.replaceState(null,"","#"+active);
  window.scrollTo({top:0,behavior:"instant"});
}
function updateLocale() {
  document.documentElement.lang = settings.language==="en" ? "en" : "zh-Hant";
  for (const node of $$("[data-i18n]")) {
    // Preserve the decorative gradient span on headings by rebuilding just its contents.
    const translated = t(node.dataset.i18n);
    if (node.id==="studio-title" || node.id==="compare-title" || node.id==="method-title") {
      const parts = translated.split(" / ");
      node.replaceChildren();
      node.append(document.createTextNode(parts[0]));
      if (parts.length>1) {
        const suffix=document.createElement("span");suffix.className="gradient-text";suffix.textContent=" / "+parts.slice(1).join(" / ");node.append(suffix);
      }
    } else node.textContent = translated;
  }
  setText("language-toggle",settings.language==="en"?"繁":"EN");
  el("language-toggle").setAttribute("aria-label",settings.language==="en"?"切換成繁體中文":"Switch to English");
  renderRuntime();renderPack();renderCompare();translateConsumer(settings.language);setPlanLocale(settings.language);translateDevice(settings.language);
  setText("current-section",panelTitle($(".nav-item.is-active")?.dataset.tab||"overview"));
}
function updateTheme() {
  const effective=resolveTheme(settings.theme,Boolean(systemColor?.matches));
  document.body.dataset.theme=effective;
  document.documentElement.style.colorScheme=effective;
  document.querySelector('meta[name="theme-color"]')?.setAttribute(
    "content",effective==="dark"?"#19171a":"#f6f3f0"
  );
  el("theme-mode").value=normalizeTheme(settings.theme);
  el("theme-mode").setAttribute("aria-label",t("appearanceLabel"));
}
function scenarioRows() {
  return scenarios.map((s)=>({scenario:s,result:calculatePack(s.input)}));
}
function renderCompare() {
  setText("scenario-count",scenarios.length+" / "+MAX_SCENARIOS+" "+t("countSaved"));
  el("export-comparison").disabled = scenarios.length===0;
  el("export-json").disabled = scenarios.length===0;
  el("clear-scenarios").disabled = scenarios.length===0;
  el("compare-empty").hidden = scenarios.length>0;
  el("compare-content").hidden = scenarios.length===0;
  if (!scenarios.length) return;
  const rows = scenarioRows();
  const container=el("scenario-cards");
  container.replaceChildren();
  rows.forEach(({scenario,result},index)=>{
    const card=document.createElement("article");card.className="scenario-card";
    card.style.setProperty("--swatch",swatches[index%swatches.length]);
    const heading=document.createElement("h3");heading.textContent=scenario.name;
    const val=document.createElement("div");val.className="scenario-large";val.textContent=num(result.energyWh,2)+" ";
    const unit=document.createElement("span");unit.textContent="Wh";val.append(unit);
    const meta=document.createElement("p");meta.textContent=result.series+"S"+result.parallel+"P · "+num(result.totalMassG,1)+" g";
    const density=document.createElement("p");density.textContent=num(result.gravimetricWhKg,1)+" Wh/kg · "+num(result.volumetricWhL,1)+" Wh/L";
    const remove=document.createElement("button");remove.className="scenario-remove";remove.type="button";
    remove.setAttribute("aria-label",t("remove")+": "+scenario.name);remove.textContent="×";
    remove.addEventListener("click",()=>{
      if(!window.confirm(t("removeConfirm"))) return;
      const updated=scenarios.filter((item)=>item.id!==scenario.id);
      if(writeScenarios(updated)){scenarios=updated;renderCompare();}
    });
    const restore=document.createElement("button");restore.className="text-link";restore.type="button";restore.textContent=t("restore")+" ↗";
    restore.addEventListener("click",()=>{applyPack(scenario.input);$$("[data-pack-preset]").forEach((b)=>b.classList.remove("is-selected"));setPanel("studio");});
    card.append(heading,val,meta,density,restore,remove);
    container.append(card);
  });
  const metrics=[
    ["metricEnergy","energyWh","Wh"],
    ["metricWhKg","gravimetricWhKg","Wh/kg"],
    ["metricWhL","volumetricWhL","Wh/L"],
    ["metricMass","totalMassG","g"]
  ];
  const charts=el("comparison-bars");charts.replaceChildren();
  metrics.forEach(([key,prop,unit])=>{
    const group=document.createElement("section");group.className="chart-group";
    const head=document.createElement("h4");head.textContent=t(key);group.append(head);
    const max=Math.max(...rows.map((x)=>x.result[prop]),1);
    rows.forEach(({scenario,result},index)=>{
      const line=document.createElement("div");line.className="chart-line";
      const label=document.createElement("span");label.className="chart-label";label.textContent=scenario.name;label.title=scenario.name;
      const rail=document.createElement("div");rail.className="chart-rail";
      const bar=document.createElement("div");bar.className="chart-fill";
      bar.style.setProperty("--bar-width",Math.max(0,result[prop]/max*100)+"%");
      bar.style.setProperty("--swatch",swatches[index%swatches.length]);
      rail.append(bar);
      const val=document.createElement("strong");val.className="chart-value";val.textContent=num(result[prop],1)+" "+unit;
      line.append(label,rail,val);group.append(line);
    });
    charts.append(group);
  });
  const columns=[["metricTopology",r=>r.series+"S"+r.parallel+"P"],["metricCells",r=>num(r.count,0)],
    ["metricEnergy",r=>num(r.energyWh,2)+" Wh"],["metricVoltage",r=>num(r.nominalVoltageV,2)+" V"],
    ["metricMass",r=>num(r.totalMassG,1)+" g"],["metricWhKg",r=>num(r.gravimetricWhKg,1)+" Wh/kg"],
    ["metricWhL",r=>num(r.volumetricWhL,1)+" Wh/L"],["metricRuntime",r=>num(r.idealRuntimeHours,2)+" h"]];
  const thead=$("#comparison-table thead");const tbody=$("#comparison-table tbody");
  thead.replaceChildren();tbody.replaceChildren();
  const rowHead=document.createElement("tr");const first=document.createElement("th");first.textContent="METRIC";first.scope="col";rowHead.append(first);
  rows.forEach(({scenario})=>{const cell=document.createElement("th");cell.scope="col";cell.textContent=scenario.name;rowHead.append(cell);});thead.append(rowHead);
  columns.forEach(([key,fn])=>{
    const tr=document.createElement("tr"),title=document.createElement("th");title.scope="row";title.textContent=t(key);tr.append(title);
    rows.forEach(({result})=>{const cell=document.createElement("td");cell.textContent=fn(result);tr.append(cell);});tbody.append(tr);
  });
}
function saveScenario() {
  if (!packResult || !currentPackInput) return;
  const feedback=el("save-feedback");
  if(scenarios.length>=MAX_SCENARIOS){feedback.textContent=t("maxSaved");return;}
  const suggested=t("defaultScenario")+" "+(scenarios.length+1);
  const inputName=window.prompt(t("savePrompt"),suggested);
  if(inputName===null)return;
  const name=inputName.trim().slice(0,60);
  if(!name){feedback.textContent=t("invalidInput");return;}
  const id=(window.crypto && typeof window.crypto.randomUUID==="function") ? window.crypto.randomUUID() : Date.now().toString(36)+"-"+Math.random().toString(36).slice(2);
  const next=[...scenarios,{id,name,input:{...currentPackInput}}];
  if(!writeScenarios(next)){feedback.textContent=t("saveFailed");return;}
  scenarios=next;feedback.textContent=t("saved");renderCompare();
}
function setup() {
  $$("[data-tab]").forEach((b)=>b.addEventListener("click",()=>setPanel(b.dataset.tab)));
  $$("[data-goto]").forEach((b)=>b.addEventListener("click",()=>setPanel(b.dataset.goto)));
  for(const id of ["runtime-wh","runtime-watts","runtime-reserve","runtime-efficiency"])
    el(id).addEventListener("input",()=>{el("device-preset").value="custom";renderRuntime();});
  el("device-preset").addEventListener("change",(event)=>{
    const preset=DEVICE_PRESETS[event.target.value];
    if(!preset)return;
    el("runtime-wh").value=preset.capacityWh;el("runtime-watts").value=preset.powerW;
    el("runtime-reserve").value=preset.reservePct;el("runtime-efficiency").value=preset.efficiencyPct;
    renderRuntime();
  });
  for(const id of Object.values(packIds))el(id).addEventListener("input",()=>{
    $$("[data-pack-preset]").forEach((b)=>b.classList.remove("is-selected"));
    el("save-feedback").textContent="";renderPack();
  });
  $$("[data-pack-preset]").forEach((b)=>b.addEventListener("click",()=>{
    $$("[data-pack-preset]").forEach((button)=>button.classList.toggle("is-selected",button===b));
    applyPack(PACK_PRESETS[b.dataset.packPreset]);
    el("save-feedback").textContent="";
  }));
  el("language-toggle").addEventListener("click",()=>{settings.language=settings.language==="en"?"zh-TW":"en";writeSettings(settings);updateLocale();});
  el("theme-mode").addEventListener("change",(event)=>{
    settings.theme=normalizeTheme(event.target.value);
    writeSettings(settings);updateTheme();
  });
  systemColor?.addEventListener?.("change",()=>{if(settings.theme==="system")updateTheme();});
  el("save-scenario").addEventListener("click",saveScenario);
  el("export-single").addEventListener("click",()=>{
    if(currentPackInput)downloadCsv(scenariosToCsv([{name:"PackFit Studio",input:currentPackInput}]),"packfit-scenario.csv");
  });
  el("import-json").addEventListener("click",()=>el("import-json-file").click());
  el("export-json").addEventListener("click",()=>{
    if(!scenarios.length)return;
    const json=serializeScenarios(scenarios);
    const url=URL.createObjectURL(new Blob([json],{type:"application/json;charset=utf-8"}));
    const link=document.createElement("a");
    link.href=url;link.download="packfit-scenarios.json";
    document.body.append(link);link.click();link.remove();
    setTimeout(()=>URL.revokeObjectURL(url),1000);
  });
  el("import-json-file").addEventListener("change",async(event)=>{
    const input=event.target, file=input.files?.[0];
    if(!file)return;
    const status=el("scenario-import-status");
    try{
      if(file.size>MAX_JSON_CHARS)throw new Error("tooLarge");
      const items=parseScenarioFile(await file.text());
      if(scenarios.length && !window.confirm(t("importConfirm")))return;
      if(!writeScenarios(items))throw new Error("storage");
      scenarios=items;
      status.textContent=t("importSuccess")+": "+items.length;
      renderCompare();
    }catch(error){
      status.textContent=t(error?.message==="tooLarge"?"importOversized":"importFailure");
    }finally{input.value="";}
  });
  el("export-comparison").addEventListener("click",()=>{if(scenarios.length)downloadCsv(scenariosToCsv(scenarios),"packfit-comparison.csv");});
  el("clear-scenarios").addEventListener("click",()=>{
    if(!scenarios.length || !window.confirm(t("clearConfirm")))return;
    if(writeScenarios([])){scenarios=[];renderCompare();}
  });
  updateTheme();updateLocale();
  setPanel(location.hash.slice(1));
  window.addEventListener("hashchange",()=>setPanel(location.hash.slice(1)));
}
setup();
initializeConsumer(()=>settings.language);
initializePlanner(()=>settings.language);
initializeDevice(()=>settings.language);
setupMotionLibrary();
setupOfflineSupport();
