/* Isolated, opt-in browser JS arithmetic probe; never scores the CPU model. */
self.addEventListener("message",()=>{
  // Each run executes the same number of arithmetic iterations.
  // Browser/thermal/scheduling differences make cross-device comparison unreliable.
  const samples=[];
  let checksum=0;
  for(let run=0;run<3;run++){
    let state=2166136261;
    const start=performance.now();
    for(let i=0;i<2000000;i++){
      state=Math.imul(state^i,16777619)>>>0;
      state^=state>>>13;
    }
    samples.push(performance.now()-start);
    checksum^=state;
  }
  samples.sort((a,b)=>a-b);
  self.postMessage({medianMs:samples[1],checksum});
  self.close();
});
