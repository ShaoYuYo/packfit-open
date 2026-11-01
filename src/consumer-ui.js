import {estimateChargeCost,estimatePowerBank,estimateChargeTime,calculateRetention,parseWindowsReport} from "./consumer.js";
import {formatNumber,formatDuration} from "./format.js";
import {consumerText} from "./consumer-i18n.js";
let language="zh-TW";
let battery=null;
let batteryStatus="notChecked";
let healthResult=null;
const q=(id)=>document.getElementById(id);
const t=(key)=>consumerText(language,key);
const fmt=(number,digits=1)=>formatNumber(number,language,digits);
const set=(id,value)=>{q(id).textContent=String(value);};
const get=(id)=>q(id).value;
function showCalculators(){
  const calculations=[
    {run:()=>estimateChargeCost({wh:get("pf-cost-wh"),eff:get("pf-cost-eff"),price:get("pf-cost-rate"),times:get("pf-cost-times")}),
     error:"pf-cost-error",ids:["pf-cost-month","pf-cost-once","pf-cost-kwh"],
     render:r=>{set("pf-cost-month",fmt(r.monthly,2));set("pf-cost-once",fmt(r.cost,2));set("pf-cost-kwh",fmt(r.gridKwh,3)+" kWh");}},
    {run:()=>estimatePowerBank({mah:get("pf-bank-mah"),v:get("pf-bank-v"),eff:get("pf-bank-eff"),targetWh:get("pf-bank-target")}),
     error:"pf-bank-error",ids:["pf-bank-charges","pf-bank-rated","pf-bank-delivered"],
     render:r=>{set("pf-bank-charges",fmt(r.charges,2));set("pf-bank-rated",fmt(r.nominalWh,1)+" Wh");set("pf-bank-delivered",fmt(r.deliveredWh,1)+" Wh");}},
    {run:()=>estimateChargeTime({wh:get("pf-time-wh"),start:get("pf-time-start"),end:get("pf-time-end"),watts:get("pf-time-w"),eff:get("pf-time-eff")}),
     error:"pf-time-error",ids:["pf-time-duration","pf-time-energy"],
     render:r=>{set("pf-time-duration",formatDuration(r.hours,language));set("pf-time-energy",fmt(r.requiredWh,1)+" Wh");}}
  ];
  for(const item of calculations){
    try{item.render(item.run());q(item.error).hidden=true;}
    catch{item.ids.forEach(id=>set(id,"—"));set(item.error,t("badInput"));q(item.error).hidden=false;}
  }
}
function updateBattery(){
  if(!battery)return;
  const percentage=Math.min(100,Math.max(0,Math.round(battery.level*100)));
  set("pf-level",Number.isFinite(percentage)?percentage+"%":"—");
  q("pf-ring").style.setProperty("--ring",Number.isFinite(percentage)?percentage+"%":"0%");
  set("pf-charge",battery.charging?t("charging"):t("discharging"));
  const seconds=battery.charging?battery.chargingTime:battery.dischargingTime;
  const duration=Number.isFinite(seconds)&&seconds>=0&&seconds<604800;
  set("pf-time",duration?formatDuration(seconds/3600,language):t("noTime"));
}
function renderHealth(){
  if(!healthResult)return;
  q("pf-retention").hidden=false;
  const h=healthResult;
  set("pf-health-percent",fmt(h.percent,1)+"%");
  q("pf-health-ring").style.setProperty("--ring",Math.min(100,Math.max(0,h.percent))+"%");
  set("pf-out-design",fmt(h.designWh,2)+" Wh");
  set("pf-out-full",fmt(h.fullWh,2)+" Wh");
  set("pf-out-cycles",h.cycles===null?t("na"):fmt(h.cycles,0));
  const advice=h.percent>105?"over":h.percent>=85?"high":h.percent>=75?"mid":"low";
  set("pf-health-advice",t(advice));
}
export function consumerSectionTitle(id,lang){
  return consumerText(lang,id==="health"?"navHealth":"navEveryday");
}
export function translateConsumer(lang){
  language=lang==="en"?"en":"zh-TW";
  document.querySelectorAll("[data-pf]").forEach(el=>{el.textContent=t(el.dataset.pf);});
  set("pf-status",t(batteryStatus));
  if(battery)updateBattery();
  renderHealth();
  showCalculators();
}
export function initializeConsumer(getLanguage){
  const ids=["pf-cost-wh","pf-cost-eff","pf-cost-rate","pf-cost-times","pf-bank-mah","pf-bank-v",
    "pf-bank-eff","pf-bank-target","pf-time-wh","pf-time-w","pf-time-start","pf-time-end","pf-time-eff"];
  ids.forEach(id=>q(id).addEventListener("input",showCalculators));
  q("pf-connect").addEventListener("click",async()=>{
    if(typeof navigator.getBattery!=="function"){batteryStatus="unsupported";translateConsumer(language);return;}
    const button=q("pf-connect");button.disabled=true;
    try{
      const manager=await navigator.getBattery();
      // Remove listeners from an old manager when rechecking, to avoid duplicates.
      if(!battery||battery!==manager){
        battery=manager;
        ["chargingchange","levelchange","chargingtimechange","dischargingtimechange"]
          .forEach(event=>manager.addEventListener(event,updateBattery));
      }
      batteryStatus="liveNotice";updateBattery();
    }catch{batteryStatus="denied";}
    finally{button.disabled=false;set("pf-status",t(batteryStatus));}
  });
  q("pf-copy").addEventListener("click",async()=>{
    try{await navigator.clipboard.writeText(q("pf-command").textContent);set("pf-copy",t("copied"));}
    catch{set("pf-report-status",t("badInput"));}
  });
  q("pf-report").addEventListener("change",async(event)=>{
    const file=event.target.files?.[0];if(!file)return;
    if(file.size>2000000){set("pf-report-status",t("bigReport"));event.target.value="";return;}
    try{
      healthResult=parseWindowsReport(await file.text());
      renderHealth();
      q("pf-design").value=healthResult.designWh;
      q("pf-full").value=healthResult.fullWh;
      set("pf-report-status",file.name+" ✓");
    }catch{set("pf-report-status",t("badReport"));}
    finally{event.target.value="";}
  });
  q("pf-analyze").addEventListener("click",()=>{
    try{healthResult={...calculateRetention(get("pf-design"),get("pf-full")),cycles:null};
      renderHealth();set("pf-report-status","");}
    catch{set("pf-report-status",t("badInput"));}
  });
  translateConsumer(getLanguage());
}
