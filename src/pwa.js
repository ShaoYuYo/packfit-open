/** Register the scoped offline cache on secure origins only. */
export function setupOfflineSupport(){
  if(!("serviceWorker" in navigator)||!window.isSecureContext)return;
  window.addEventListener("load",()=>{
    navigator.serviceWorker.register("./sw.js",{scope:"./"}).catch(()=>{
      // Some private-browsing modes and enterprise policies intentionally block SW.
      // The website remains fully functional online without a service worker.
    });
  },{once:true});
}
