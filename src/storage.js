import {calculatePack, getPackInputKeys} from "./calc.js";
const STORE_KEY = "packfit:scenarios:v1";
const SETTINGS_KEY = "packfit:settings:v1";
export const MAX_SCENARIOS = 4;
export const THEME_OPTIONS = Object.freeze(["system","light","dark"]);
export function readSettings() {
  try {
    const value = JSON.parse(localStorage.getItem(SETTINGS_KEY) || "{}");
    // Legacy v0.5 theme values were saved without explicit user intent, so
    // new installations and upgrades default to the system theme.
    const mode = THEME_OPTIONS.includes(value.themeMode) ? value.themeMode : "system";
    return {language:value.language === "en" ? "en" : "zh-TW",theme:mode};
  } catch { return {language:"zh-TW",theme:"system"}; }
}
export function writeSettings(settings) {
  try {
    const mode = THEME_OPTIONS.includes(settings.theme) ? settings.theme : "system";
    localStorage.setItem(SETTINGS_KEY,JSON.stringify({
      language:settings.language === "en" ? "en" : "zh-TW",
      themeMode:mode,
      preferencesVersion:2
    }));
    return true;
  } catch { return false; }
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