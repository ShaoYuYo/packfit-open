import {calculatePack, getPackInputKeys} from "./calc.js";
const STORE_KEY = "packfit:scenarios:v1";
const SETTINGS_KEY = "packfit:settings:v1";
export const MAX_SCENARIOS = 4;
export function readSettings() {
  try {
    const value = JSON.parse(localStorage.getItem(SETTINGS_KEY) || "{}");
    return {language: value.language === "en" ? "en" : "zh-TW", theme: value.theme === "light" ? "light" : "dark"};
  } catch { return {language:"zh-TW",theme:"dark"}; }
}
export function writeSettings(settings) {
  try { localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings)); return true; } catch { return false; }
}
export function validateScenario(record) {
  if (!record || typeof record !== "object" || typeof record.name !== "string" || !record.name.trim() ||
      record.name.length > 60 || typeof record.id !== "string" || record.id.length > 100 ||
      !record.input || typeof record.input !== "object") return false;
  try { calculatePack(record.input); return true; } catch { return false; }
}
export function readScenarios() {
  try {
    const parsed = JSON.parse(localStorage.getItem(STORE_KEY) || "[]");
    return Array.isArray(parsed) ? parsed.filter(validateScenario).slice(0, MAX_SCENARIOS).map((record) => ({
      id:record.id, name:record.name.trim(), input:Object.fromEntries(getPackInputKeys().map((key) => [key, Number(record.input[key])]))
    })) : [];
  } catch { return []; }
}
export function writeScenarios(scenarios) {
  if (!Array.isArray(scenarios) || scenarios.length > MAX_SCENARIOS || scenarios.some((x) => !validateScenario(x))) return false;
  try { localStorage.setItem(STORE_KEY, JSON.stringify(scenarios)); return true; } catch { return false; }
}