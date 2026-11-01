/**
 * Portable, versioned scenario file format.
 *
 * Never trust imported JSON. Revalidate every field through the pack model,
 * canonicalize records and prevent prototype-key/object field pollution.
 */
import {calculatePack,getPackInputKeys} from "./calc.js";
import {MAX_SCENARIOS} from "./storage.js";

export const SCENARIO_FORMAT = "packfit.scenarios";
export const SCENARIO_SCHEMA_VERSION = 1;
export const MAX_JSON_CHARS = 200_000;

export class ScenarioFileError extends Error {
  constructor(code){super(code);this.name="ScenarioFileError";this.code=code;}
}
const isObject = (x) => x!==null && typeof x==="object" && !Array.isArray(x);
function canonicalize(record,index){
  if(!isObject(record)||typeof record.name!=="string"||record.name.trim().length===0
    ||record.name.trim().length>60||!isObject(record.input))throw new ScenarioFileError("invalidScenario");
  const input={};
  for(const key of getPackInputKeys()){
    // Imported inputs must be actual JSON numbers, not strings, booleans or null.
    const value=record.input[key];
    if(typeof value!=="number"||!Number.isFinite(value))throw new ScenarioFileError("invalidScenario");
    input[key]=value;
  }
  try{calculatePack(input);}catch{throw new ScenarioFileError("invalidScenario");}
  // File identifiers aren't trusted; assign stable, locally scoped IDs instead.
  return {id:"import-"+index+"-"+Math.random().toString(36).slice(2,12),
    name:record.name.trim(),input};
}
export function serializeScenarios(records){
  if(!Array.isArray(records)||records.length===0||records.length>MAX_SCENARIOS)
    throw new ScenarioFileError("invalidCount");
  const scenarios=records.map(canonicalize);
  return JSON.stringify({format:SCENARIO_FORMAT,version:SCENARIO_SCHEMA_VERSION,
    scenarios:scenarios.map(({name,input})=>({name,input}))},null,2)+"\n";
}
export function parseScenarioFile(json){
  if(typeof json!=="string"||json.length>MAX_JSON_CHARS)
    throw new ScenarioFileError("fileTooLarge");
  let value;
  try{value=JSON.parse(json);}catch{throw new ScenarioFileError("badJson");}
  if(!isObject(value)||value.format!==SCENARIO_FORMAT||value.version!==SCENARIO_SCHEMA_VERSION)
    throw new ScenarioFileError("unsupportedVersion");
  if(!Array.isArray(value.scenarios)||value.scenarios.length===0||value.scenarios.length>MAX_SCENARIOS)
    throw new ScenarioFileError("invalidCount");
  return value.scenarios.map(canonicalize);
}