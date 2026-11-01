/**
 * A simple day-plan energy budget using constant, user-editable average loads.
 * This is not a physics simulation; it does not model variable efficiency,
 * sleep/standby, aging, voltage sag or thermal shutdown.
 */
import {ConsumerInputError} from "./consumer.js";
function bound(raw,field,min,max){
 if(raw===""||raw===null||raw===undefined||!Number.isFinite(Number(raw))
   ||Number(raw)<min||Number(raw)>max)throw new ConsumerInputError(field);
 return Number(raw);
}
export function estimateDayPlan(input){
 const capacityWh=bound(input.capacityWh,"capacityWh",.1,2000);
 const startPct=bound(input.startPct,"startPct",0,100);
 const reservePct=bound(input.reservePct,"reservePct",0,90);
 if(!Array.isArray(input.tasks)||input.tasks.length===0||input.tasks.length>8)
   throw new ConsumerInputError("tasks");
 const tasks=input.tasks.map((task,index)=>{
   const hours=bound(task.hours,"task "+index+" hours",0,24);
   const watts=bound(task.watts,"task "+index+" watts",0,1000);
   return Object.freeze({hours,watts,energyWh:hours*watts});
 });
 const totalWh=tasks.reduce((sum,task)=>sum+task.energyWh,0);
 const initialWh=capacityWh*startPct/100;
 const minimumWh=capacityWh*reservePct/100;
 const usableWh=Math.max(0,initialWh-minimumWh);
 const shortfallWh=Math.max(0,totalWh-usableWh);
 const remainingWh=Math.max(0,initialWh-totalWh);
 const endingPct=remainingWh/capacityWh*100;
 const fits=totalWh<=usableWh+1e-9;
 return Object.freeze({capacityWh,startPct,reservePct,tasks,totalWh,usableWh,
   remainingWh,endingPct,shortfallWh,fits,usedFraction:usableWh>0?totalWh/usableWh:(totalWh>0?Infinity:0)});
}