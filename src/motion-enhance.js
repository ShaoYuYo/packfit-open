/**
 * Optional UI transitions for static GitHub Pages.
 *
 * Motion (MIT): https://github.com/motiondivision/motion
 * Version-pinned ESM is loaded asynchronously, ONLY as an enhancement.
 * No calculation or navigation depends on this third-party request.
 * When blocked or offline, native Web Animations API is used.
 */
const MOTION_URL = "https://cdn.jsdelivr.net/npm/motion@13.2.0/+esm";
let motionAnimate = null;
let requested = false;
const active = new WeakMap();

function allowed(element) {
  if (!(element instanceof Element) || document.hidden) return false;
  if (window.matchMedia?.("(prefers-reduced-motion: reduce)").matches) return false;
  return element.getClientRects().length > 0;
}
function animateSafe(element, start, end, duration) {
  if (!allowed(element)) return;
  const previous = active.get(element);
  if (previous && typeof previous.cancel === "function") {
    try { previous.cancel(); } catch { /* completion already settled */ }
  }
  const options = {duration, ease:"easeOut"};
  if (typeof motionAnimate === "function") {
    try {
      const handle = motionAnimate(
        element,
        {opacity:[start.opacity,end.opacity],transform:[start.transform,end.transform]},
        {duration:duration / 1000,ease:"easeOut"}
      );
      active.set(element,handle);
      return;
    } catch { /* Use native fallback below. */ }
  }
  if (typeof element.animate === "function") {
    const handle = element.animate([start,end],{
      duration,
      easing:"cubic-bezier(.22,.72,.28,1)",
      fill:"none"
    });
    active.set(element,handle);
  }
}
export function animatePanelEntry(element) {
  animateSafe(element,
    {opacity:.72,transform:"translateY(7px)"},
    {opacity:1,transform:"translateY(0)"},
    240
  );
}
export function animateCalculation(element) {
  animateSafe(element,
    {opacity:.68,transform:"translateY(2px)"},
    {opacity:1,transform:"translateY(0)"},
    170
  );
}
export function setupMotionLibrary() {
  if (requested || typeof window === "undefined") return;
  requested = true;
  if (window.matchMedia?.("(prefers-reduced-motion: reduce)").matches) return;
  if (navigator.onLine === false) return;
  // Isolate the dependency from first-render/critical calculation paths.
  // A failed import is deliberately nonfatal.
  void import(MOTION_URL).then((module)=>{
    if (typeof module.animate === "function") motionAnimate = module.animate;
  }).catch(()=>{motionAnimate = null;});
}