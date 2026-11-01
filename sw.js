/* Offline-first static assets; navigations are network-first to allow updates. */
const CACHE = "packfit-static-v0.3.0";
const RESOURCES = [
  "./", "./index.html", "./styles.css", "./assets/premium.css",
  "./assets/logo.svg", "./manifest.webmanifest",
  "./src/app.js", "./src/calc.js", "./src/format.js",
  "./src/presets.js", "./src/storage.js", "./src/csv.js",
  "./src/i18n.js", "./src/consumer.js", "./src/consumer-ui.js",
  "./src/consumer-i18n.js", "./src/scenario-transfer.js", "./src/pwa.js"
];
const cacheUrls = new Set(RESOURCES.map((path)=>new URL(path,self.registration.scope).pathname));

self.addEventListener("install",(event)=>{
  event.waitUntil((async()=>{
    const cache=await caches.open(CACHE);
    await cache.addAll(RESOURCES.map((path)=>new URL(path,self.registration.scope)));
    await self.skipWaiting();
  })());
});

self.addEventListener("activate",(event)=>{
  event.waitUntil((async()=>{
    const keys=await caches.keys();
    await Promise.all(keys.filter((key)=>key.startsWith("packfit-static-")&&key!==CACHE)
      .map((key)=>caches.delete(key)));
    await self.clients.claim();
  })());
});

self.addEventListener("fetch",(event)=>{
  const request=event.request;
  if(request.method!=="GET")return;
  const url=new URL(request.url);
  if(url.origin!==self.location.origin)return;
  if(!url.href.startsWith(self.registration.scope))return;
  if(request.mode==="navigate"){
    event.respondWith((async()=>{
      try{
        const response=await fetch(request);
        if(response.ok)return response;
      }catch{/* offline fallback */}
      const cache=await caches.open(CACHE);
      return await cache.match(new URL("./index.html",self.registration.scope)) ||
        Response.error();
    })());
    return;
  }
  if(!cacheUrls.has(url.pathname))return;
  event.respondWith((async()=>{
    const cache=await caches.open(CACHE);
    const cached=await cache.match(request);
    if(cached)return cached;
    return fetch(request);
  })());
});
