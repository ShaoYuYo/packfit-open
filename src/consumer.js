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
function strip(html){
  return html.replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi," ")
    .replace(/<style\b[^>]*>[\s\S]*?<\/style>/gi," ")
    .replace(/<[^>]+>/g," ").replace(/&nbsp;|&#160;|&#xA0;/gi," ")
    .replace(/&#(\d+);/g,(_,d)=>String.fromCodePoint(Math.min(Number(d),0x10ffff)))
    .replace(/\s+/g," ").trim();
}
export function parseWindowsReport(html){
  if(typeof html!=="string"||html.length>2500000)throw new Error("Invalid size or format");
  const rows=[...html.matchAll(/<tr\b[^>]*>([\s\S]*?)<\/tr\s*>/gi)].map(r=>
    [...r[1].matchAll(/<t[dh]\b[^>]*>([\s\S]*?)<\/t[dh]\s*>/gi)].map(c=>strip(c[1]))).filter(c=>c.length>=2);
  const found={};
  for(const row of rows){
    const label=row[0].replace(/[:：]$/,"").toUpperCase().trim();
    for(const [key,variants] of Object.entries(labels))
      if(found[key]===undefined&&variants.includes(label))found[key]=row[1];
  }
  const energy=(raw)=>{
    const match=String(raw||"").match(/([\d][\d\s,.]*)\s*(mWh|Wh)\b/i);
    if(!match)return null;
    const n=Number(match[1].replace(/[\s,]/g,""));
    if(!Number.isFinite(n)||n<=0)return null;
    return match[2].toLowerCase()==="mwh"?n/1000:n;
  };
  const design=energy(found.design),full=energy(found.full);
  if(design===null||full===null)throw new Error("Missing battery capacities");
  const capacity=calculateRetention(design,full);
  const cyclesMatch=(found.cycles||"").match(/\d[\d,]*/);
  return {...capacity,cycles:cyclesMatch?Number(cyclesMatch[0].replace(/,/g,"")):null};
}