/** Theme resolution is pure so system preference behavior can be tested. */
export const MODES=Object.freeze(["system","light","dark"]);
export function normalizeTheme(value){
  return MODES.includes(value)?value:"system";
}
export function resolveTheme(mode,systemDark){
  const preference=normalizeTheme(mode);
  return preference==="system"?(systemDark?"dark":"light"):preference;
}
export function nextTheme(mode){
  const values=MODES;
  return values[(values.indexOf(normalizeTheme(mode))+1)%values.length];
}
