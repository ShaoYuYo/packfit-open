import {estimateDayPlan} from "./planner.js";
import {formatNumber} from "./format.js";

export const PLANNER_TRANSLATIONS={
  "zh-TW":{
    title:"今天的電量夠不夠？",intro:"輸入現在的電量，以及等等要做哪些事，就能估計需不需要先充電。",
    capacity:"電池容量",starting:"目前電量",reserve:"最低保留電量",
    activity:"預計使用活動",assumption:"以下耗電為示範數值",
    office:"文書／網頁",video:"影音娛樂",game:"遊戲／高負載",
    hours:"使用時間",watts:"平均耗電",explain:"這是定功率模型，未計入睡眠、電池老化、溫度或電池放電曲線。請依自己的設備調整功耗數字。",
    estimate:"今天的電量預算",required:"活動需要",available:"保留底線後可用",
    ending:"結束後預估電量",enough:"電量足夠",short:"電量不足",
    enoughText:"依照設定的平均功率，今天的活動可完成並保留預定電量。",
    shortText:"依照目前設定，電量不足以在保留底線前完成全部活動。還差約 ",
    error:"輸入不正確，請檢查電量百分比、使用時數和平均功率。"
  },
  en:{
    title:"Enough battery for today?",intro:"Set your current charge and what you plan to do. See whether you should plug in first.",
    capacity:"Battery size",starting:"Current charge",reserve:"Minimum reserve",
    activity:"Planned activities",assumption:"Power values are illustrative",
    office:"Office / web",video:"Streaming / video",game:"Gaming / heavy use",
    hours:"Duration",watts:"Average power",explain:"Assumes constant loads. It does not simulate idle time, aging, temperature or discharge curves. Adjust watts to match your device.",
    estimate:"Your energy budget",required:"Activities require",available:"Available above reserve",
    ending:"Estimated ending charge",enough:"Enough power",short:"Not enough power",
    enoughText:"The modeled tasks fit within available energy while preserving your chosen reserve.",
    shortText:"The modeled tasks exceed the energy available above your reserve by about ",
    error:"Check your input percentages, activity hours and average watts."
  }
};
const el=id=>document.getElementById(id);
let locale="zh-TW";
const t=k=>PLANNER_TRANSLATIONS[locale][k]||k;
const fmt=(v,n=1)=>formatNumber(v,locale,n);
function update(){
  const ids=[["plan-work-hours","plan-work-watts"],["plan-video-hours","plan-video-watts"],["plan-game-hours","plan-game-watts"]];
  const tasks=ids.map(([hours,watts])=>({hours:el(hours).value,watts:el(watts).value}));
  try{
    const result=estimateDayPlan({
      capacityWh:el("plan-wh").value,startPct:el("plan-start").value,
      reservePct:el("plan-reserve").value,tasks
    });
    el("plan-error").hidden=true;
    el("plan-main").textContent=t(result.fits?"enough":"short");
    el("plan-used").textContent=fmt(result.totalWh,2)+" Wh";
    el("plan-available").textContent=fmt(result.usableWh,2)+" Wh";
    el("plan-ending").textContent=fmt(result.endingPct,1)+"%";
    el("plan-fill").style.width=Math.min(100,Math.max(0,result.usedFraction*100))+"%";
    el("plan-status").textContent=result.fits?t("enoughText"):t("shortText")+fmt(result.shortfallWh,2)+" Wh";
    const output=el("plan-main").closest(".plan-output");
    output.classList.toggle("is-short",!result.fits);
  }catch{
    el("plan-error").hidden=false;
    el("plan-error").textContent=t("error");
    ["plan-main","plan-used","plan-available","plan-ending"].forEach(id=>el(id).textContent="—");
    el("plan-fill").style.width="0%";
    el("plan-status").textContent="";
  }
}
export function setPlanLocale(lang){
  locale=lang==="en"?"en":"zh-TW";
  document.querySelectorAll("[data-plan]").forEach(node=>{node.textContent=t(node.dataset.plan);});
  update();
}
export function initializePlanner(getLanguage){
  for(const id of ["plan-wh","plan-start","plan-reserve","plan-work-hours","plan-work-watts","plan-video-hours","plan-video-watts","plan-game-hours","plan-game-watts"])
    el(id).addEventListener("input",update);
  setPlanLocale(getLanguage());
}
