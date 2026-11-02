/**
 * Consumer battery estimators: pure, validated functions with explicit units.
 * No device hardware access, storage, or network operations.
 */
export class ConsumerInputError extends RangeError {
  constructor(field){super("Invalid input: "+field);this.name="ConsumerInputError";this.field=field;}
}
const bounded=(raw,field,min,max)=>{
  if(raw===""||raw===undefined||raw===null||!Number.isFinite(Number(raw))||Number(raw)<min||Number(raw)>max)
    throw new ConsumerInputError(field);
  return Number(raw);
};
export function estimateChargeCost(i){
  const wh=bounded(i.wh,"wh",.1,2000),eff=bounded(i.eff,"eff",1,100)/100;
  const price=bounded(i.price,"price",0,100),times=bounded(i.times,"times",0,300);
  const gridKwh=wh/1000/eff;
  return Object.freeze({gridKwh,cost:gridKwh*price,monthly:gridKwh*price*times});
}
export function estimatePowerBank(i){
  const mah=bounded(i.mah,"mah",100,1000000),v=bounded(i.v,"v",2,5);
  const eff=bounded(i.eff,"eff",1,100)/100,targetWh=bounded(i.targetWh,"targetWh",.1,1000);
  const nominalWh=mah*v/1000,deliveredWh=nominalWh*eff;
  return Object.freeze({nominalWh,deliveredWh,charges:deliveredWh/targetWh});
}
export function estimateChargeTime(i){
  const wh=bounded(i.wh,"wh",.1,2000),start=bounded(i.start,"start",0,99);
  const end=bounded(i.end,"end",1,100),watts=bounded(i.watts,"watts",.1,1000);
  const eff=bounded(i.eff,"eff",1,100)/100;
  if(end<=start)throw new ConsumerInputError("end");
  const requiredWh=wh*(end-start)/100;
  // Fixed 15% illustrative charging-curve buffer, not a measured profile.
  return Object.freeze({requiredWh,hours:requiredWh/(watts*eff)*1.15});
}
export function calculateRetention(designWh,fullWh){
  designWh=bounded(designWh,"designWh",.1,2000);
  fullWh=bounded(fullWh,"fullWh",.01,2500);
  return Object.freeze({designWh,fullWh,percent:fullWh/designWh*100});
}
const labels={
  design:["DESIGN CAPACITY","設計容量","设计容量"],
  full:["FULL CHARGE CAPACITY","完整充電容量","完全充電容量","完全充电容量"],
  cycles:["CYCLE COUNT","循環次數","循环次数"]
};
const INSTALLED_TITLES=new Set([
  "INSTALLED BATTERIES","已安裝電池","已安裝的電池","已安装的电池","安装的电池"
]);
const MAX_REPORT_CHARS=2_500_000,MAX_INSTALLED_BATTERIES=12;
function strip(html){
  return html.replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi," ")
    .replace(/<style\b[^>]*>[\s\S]*?<\/style>/gi," ")
    .replace(/<[^>]+>/g," ").replace(/&nbsp;|&#160;|&#xA0;/gi," ")
    .replace(/&#(\d+);/g,(_,d)=>String.fromCodePoint(Math.min(Number(d),0x10ffff)))
    .replace(/\s+/g," ").trim();
}
function capacityWh(raw){
  const match=String(raw??"").match(/([\d][\d\s,.]*)\s*(mWh|Wh)\b/i);
  if(!match)return null;
  const value=Number(match[1].replace(/[\s,]/g,""));
  if(!Number.isFinite(value)||value<=0)return null;
  return match[2].toLowerCase()==="mwh"?value/1000:value;
}
function installedSection(html){
  const headings=[...html.matchAll(/<h[1-6]\b[^>]*>([\s\S]*?)<\/h[1-6]\s*>/gi)];
  for(let index=0;index<headings.length;index++){
    const heading=headings[index],label=strip(heading[1]).toUpperCase();
    if(!INSTALLED_TITLES.has(label))continue;
    const start=heading.index+heading[0].length;
    return html.slice(start,headings[index+1]?.index??html.length);
  }
  if(headings.length)throw new Error("Installed batteries section not found");
  // Legacy minimal single-table fixtures remain supported.
  const first=html.match(/<table\b[^>]*>[\s\S]*?<\/table\s*>/i);
  return first?first[0]:html;
}
/**
 * Parses physically distinct battery records, without executing or retaining HTML.
 * Rejects ambiguous multi-column records rather than guessing how values align.
 */
export function parseWindowsReportRecords(html){
  if(typeof html!=="string"||!html.trim()||html.length>MAX_REPORT_CHARS)
    throw new Error("Invalid size or format");
  const section=installedSection(html);
  const tables=[...section.matchAll(/<table\b[^>]*>([\s\S]*?)<\/table\s*>/gi)];
  if(!tables.length)throw new Error("Installed battery table missing");
  const results=[];
  function parseTable(table){
    let current={design:null,full:null,cycles:null},started=false;
    function complete(){
      if(!started)return;
      if(current.design===null||current.full===null)
        throw new Error("Incomplete installed battery capacity");
      if(results.length>=MAX_INSTALLED_BATTERIES)throw new Error("Too many battery records");
      const calculated=calculateRetention(current.design,current.full);
      results.push({...calculated,cycles:current.cycles});
      current={design:null,full:null,cycles:null};started=false;
    }
    const rows=[...table.matchAll(/<tr\b[^>]*>([\s\S]*?)<\/tr\s*>/gi)];
    for(const row of rows){
      const cells=[...row[1].matchAll(/<t[dh]\b[^>]*>([\s\S]*?)<\/t[dh]\s*>/gi)]
        .map(match=>strip(match[1]));
      if(cells.length<2)continue;
      const key=cells[0].replace(/[:：]$/,"").toUpperCase().trim();
      if(["NAME","名稱","名称"].includes(key)){
        if(started)complete();
        continue;
      }
      const kind=Object.keys(labels).find(x=>labels[x].includes(key));
      if(!kind)continue;
      if(cells.length!==2)throw new Error("Ambiguous multi-column battery report");
      if(kind==="cycles"){
        const matched=cells[1].match(/\d[\d,]*/);
        current.cycles=matched?Number(matched[0].replace(/,/g,"")):null;
        continue;
      }
      if(current[kind]!==null)complete();
      current[kind]=capacityWh(cells[1]);
      if(current[kind]===null)throw new Error("Unsupported battery capacity units");
      started=true;
    }
    complete();
  }
  tables.forEach(match=>parseTable(match[1]));
  if(!results.length)throw new Error("Missing battery capacities");
  return results;
}
export function parseWindowsReport(html){
  const records=parseWindowsReportRecords(html);
  if(records.length!==1)throw new Error("Multiple batteries; select a battery separately");
  return records[0];
}
