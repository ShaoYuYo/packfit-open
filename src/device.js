/**
 * Browser-exposed hardware hints — approximate, incomplete and local-only.
 * This module does NOT identify exact CPU models, installed RAM, game FPS
 * or cross-browser benchmark rankings.
 */
export function normalizeHardwareHints(nav){
  const threads=Number.isInteger(nav?.hardwareConcurrency)&&nav.hardwareConcurrency>0
    ? Math.min(256,nav.hardwareConcurrency):null;
  const memory=typeof nav?.deviceMemory==="number" && Number.isFinite(nav.deviceMemory)
    && nav.deviceMemory>0?Math.min(1024,nav.deviceMemory):null;
  const platform=typeof nav?.userAgentData?.platform==="string"
    ? nav.userAgentData.platform.slice(0,36)
    : typeof nav?.platform==="string"?nav.platform.slice(0,36):"";
  return {threads,memory,platform,webgpu:Boolean(nav?.gpu)};
}
export function resourceProfile(hints){
  const {threads,memory}=hints;
  // Browser-provided resource brackets, NOT a CPU/GPU benchmark.
  // Incomplete hints must never be converted to a definitive performance tier.
  if(threads===null||memory===null)return {level:"unknown",confidence:"low"};
  if(threads>=8&&memory>=8)return {level:"roomy",confidence:"low"};
  if(threads>=4&&memory>=4)return {level:"typical",confidence:"low"};
  return {level:"limited",confidence:"low"};
}
export function safeGpuLabel(value){
  if(typeof value!=="string")return "";
  return value.replace(/[\u0000-\u001f\u007f]/g," ").trim().slice(0,120);
}
export function graphicsCapabilities(canvas){
  if(!canvas || typeof canvas.getContext!=="function")return {webgl:null,renderer:"",maxTexture:null};
  let gl=null,webgl=null,renderer="",maxTexture=null;
  try{
    gl=canvas.getContext("webgl2",{powerPreference:"low-power"}) ||
       canvas.getContext("webgl",{powerPreference:"low-power"});
    if(gl){
      webgl=typeof WebGL2RenderingContext!=="undefined" &&
        gl instanceof WebGL2RenderingContext?"WebGL 2":"WebGL 1";
      maxTexture=gl.getParameter(gl.MAX_TEXTURE_SIZE);
      const ext=gl.getExtension("WEBGL_debug_renderer_info");
      if(ext)renderer=safeGpuLabel(gl.getParameter(ext.UNMASKED_RENDERER_WEBGL));
    }
  }catch{return {webgl:null,renderer:"",maxTexture:null};}
  finally{gl?.getExtension("WEBGL_lose_context")?.loseContext?.();}
  return {webgl,renderer,maxTexture:Number.isFinite(maxTexture)?maxTexture:null};
}
