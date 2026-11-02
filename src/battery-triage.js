/**
 * A symptom-first, offline decision aid for people who have not imported a report.
 * No diagnosis is inferred from battery percentage, browser APIs, or user behavior.
 * Guidance is deliberately actionable without requiring access to personal files.
 */
export const SYMPTOM_IDS = Object.freeze([
  "short-runtime",
  "overnight-drain",
  "not-charging",
  "unexpected-shutdown",
  "hot-or-swollen"
]);

const guide = {
  "zh-TW":{
    "short-runtime":{
      label:"續航突然變短",
      summary:"電池容量下降、程式耗電與使用情境，都可能讓續航變短。先比較同樣使用情境下的情況。",
      steps:[
        "先回想：是不是最近換了使用方式、螢幕亮度或常開的程式？同樣工作內容下比較才有意義。",
        "打開 Windows「設定 → 系統 → 電源與電池」，查看最近的電池使用情形，辨識異常耗電的程式。",
        "需要比較電池容量時，再產生 Windows batteryreport；滿充容量與設計容量的比例只是容量參考，不是安全或續航保證。"
      ],
      help:"如果續航嚴重衰退、電腦反覆無預警關機，請聯絡設備原廠或合格維修人員。"
    },
    "overnight-drain":{
      label:"合上筆電，隔天掉很多電",
      summary:"先釐清是睡眠、休眠還是關機；「合上蓋子」不一定代表完全停止耗電。",
      steps:[
        "記下合蓋前與隔天打開時的電量、經過幾小時，以及是否接著外接裝置。",
        "確認 Windows 的電源與睡眠設定，以及合上蓋子時要執行的動作。",
        "在支援的 Windows 裝置上，可使用系統內建的 SleepStudy 或電池報告進一步比對；不支援時不應推測睡眠耗電來源。"
      ],
      help:"如果掉電反覆發生，先提供發生條件與時間給原廠支援；不要僅憑一次掉電就判定電池故障。"
    },
    "not-charging":{
      label:"插上充電器，卻沒充進去",
      summary:"充電上限、電源供應器、接頭或系統設定都可能影響充電；先做安全、非拆機的檢查。",
      steps:[
        "先確認充電線、接頭與充電器是否符合原廠規格，並觀察是否有明顯破損或異常發熱。",
        "查看廠商設定是否開啟了延長電池壽命的充電上限；停在 80% 並不一定是故障。",
        "如果沒有任何充電反應，可嘗試合適的原廠電源供應器及正常插座；不要拆開電池或自行短接端子。"
      ],
      help:"持續無法充電、接頭過熱或出現異味時停止使用，交由合格人員檢查。"
    },
    "unexpected-shutdown":{
      label:"還有電，電腦卻突然關機",
      summary:"電量百分比不能單獨證明電池故障；系統、溫度、供電和電量估算都有可能相關。",
      steps:[
        "記下關機當下的電量、是否接電源、正在執行什麼，以及是否有發熱或錯誤提示。",
        "查看 Windows 的可靠性監視器或事件檢視器，找出關機時間附近的紀錄；事件本身不是根因結論。",
        "若可以安全重現，記錄同樣條件是否持續發生。避免反覆刻意將電池放到完全沒電。"
      ],
      help:"若持續無預警關機，請向原廠或合格維修人員提供時間與系統紀錄。"
    },
    "hot-or-swollen":{
      label:"電池異常發熱、膨脹或有異味",
      summary:"這是需要優先處理的安全問題，不適合靠軟體分數判斷是否還能繼續使用。",
      steps:[
        "若發現電池膨脹、冒煙或異味，停止使用與充電；如可安全操作，依設備指引關閉電源。",
        "不要擠壓、刺穿、拆解電池，也不要嘗試自行維修或重新充電來測試。",
        "聯絡原廠、合格維修人員；若有立即起火或冒煙危險，遠離危險並依當地緊急程序求助。"
      ],
      help:"不要把 PackFit 的電池容量數字當成「可以繼續充電」的安全判定。"
    }
  },
  en:{
    "short-runtime":{
      label:"Battery life became much shorter",
      summary:"Reduced capacity, more demanding apps, and different usage can all shorten runtime. Compare similar conditions first.",
      steps:[
        "Consider whether the workload, screen brightness or background apps have changed. Compare like-for-like usage.",
        "Open Windows Settings → System → Power & battery and check which apps are consuming battery power.",
        "For capacity context, generate a Windows battery report. Full-charge versus design capacity is not a safety or runtime guarantee."
      ],
      help:"For major runtime loss or recurring sudden shutdowns, contact your device manufacturer or a qualified technician."
    },
    "overnight-drain":{
      label:"Battery drains while the lid is closed",
      summary:"First check whether the computer is sleeping, hibernating or shutting down. Closing the lid does not always stop power use.",
      steps:[
        "Record battery percentage before closing the lid and the next morning, elapsed hours and attached accessories.",
        "Review Windows power and sleep settings, including the configured lid-close action.",
        "If supported, inspect Windows SleepStudy or battery reports. Do not infer sleep-state behavior when evidence is unavailable."
      ],
      help:"If it repeats, share the timeline and conditions with device support; one observation does not establish battery failure."
    },
    "not-charging":{
      label:"Plugged in, but not charging",
      summary:"Charging limits, power adapters, connectors and firmware settings can affect charging; start with safe external checks.",
      steps:[
        "Check that the power adapter and cable meet the manufacturer's requirements and are not damaged or unusually hot.",
        "Look for an OEM battery-care charge limit; stopping around 80% can be intentional.",
        "Where practical, try an appropriate manufacturer-approved charger and known working outlet. Do not open the battery pack."
      ],
      help:"Stop using a damaged or overheating connector and contact qualified device support."
    },
    "unexpected-shutdown":{
      label:"Laptop shuts down with charge remaining",
      summary:"A displayed charge percentage cannot distinguish battery, OS, thermal or power-delivery causes on its own.",
      steps:[
        "Write down the charge level, AC connection, running apps, temperature symptoms and any error message.",
        "Review Windows Reliability Monitor or Event Viewer around the shutdown time; events are clues, not root-cause proof.",
        "If it happens again during normal, safe use, note whether the conditions are repeatable. Do not intentionally deep-discharge a pack."
      ],
      help:"For recurring shutdowns, share the approximate timestamps and system observations with qualified support."
    },
    "hot-or-swollen":{
      label:"Battery is swollen, smells unusual or overheats",
      summary:"Treat this as a possible safety hazard. Software capacity scores cannot establish that the device is safe to use.",
      steps:[
        "Stop using or charging a device with a swollen, smoking or unusually smelling battery. Power down only if safe to do so.",
        "Never puncture, crush, disassemble or attempt to recharge the battery as a test.",
        "Contact the manufacturer or qualified service. If there is an immediate fire or smoke hazard, move away and follow local emergency procedures."
      ],
      help:"Do not treat a PackFit capacity number as permission to continue charging."
    }
  }
};

/**
 * Every symptom produces steps without requesting a hardware identifier or upload.
 * Explicit unknown inputs are rejected rather than silently showing unrelated advice.
 */
export function getSymptomGuide(locale,symptom){
  if(!SYMPTOM_IDS.includes(symptom))throw new RangeError("Unknown battery symptom");
  const language=locale==="en"?"en":"zh-TW";
  const item=guide[language][symptom];
  return Object.freeze({id:symptom,label:item.label,summary:item.summary,
    steps:Object.freeze([...item.steps]),help:item.help});
}
