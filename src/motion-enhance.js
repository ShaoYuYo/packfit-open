/**
 * Local-only optional UI transitions. Uses the browser-native Web Animations API.
 * No remote modules, tracking, font files or runtime network dependencies.
 * Reduced-motion users and offline browsers work with the same code path.
 */
const active = new WeakMap();

function allowed(element) {
  if (typeof Element==="undefined" || !(element instanceof Element) ||
      (typeof document!=="undefined" && document.hidden)) return false;
  if (typeof window!=="undefined" &&
      window.matchMedia?.("(prefers-reduced-motion: reduce)").matches) return false;
  return element.getClientRects().length > 0;
}

function animateSafe(element,start,end,duration) {
  if (!allowed(element) || typeof element.animate!=="function") return;
  const previous=active.get(element);
  if(previous && typeof previous.cancel==="function"){
    try { previous.cancel(); } catch { /* Already settled */ }
  }
  const handle=element.animate([start,end],{
    duration,easing:"cubic-bezier(.22,.72,.28,1)",fill:"none"
  });
  active.set(element,handle);
}

export function animatePanelEntry(element) {
  animateSafe(element,
    {opacity:.72,transform:"translateY(7px)"},
    {opacity:1,transform:"translateY(0)"},240);
}

export function animateCalculation(element) {
  animateSafe(element,
    {opacity:.68,transform:"translateY(2px)"},
    {opacity:1,transform:"translateY(0)"},170);
}

/** Kept as a compatibility API; no external library or CDN is loaded. */
export function setupMotionLibrary() {
  // Browser-native element.animate() is available when motion is allowed.
}
