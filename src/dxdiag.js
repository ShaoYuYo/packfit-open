/**
 * Local, non-executable parsing of an optional DxDiag plain-text export.
 * Only a handful of non-identifying hardware fields are extracted.
 * Never persist or upload the original diagnostic report.
 */
export function cleanDiagnosticField(value){
  return String(value??"").replace(/[\u0000-\u001f\u007f]/g," ").replace(/\s+/g," ").trim().slice(0,150);
}
export function parseDxDiagReport(text){
  if(typeof text!=="string"||text.length>2_000_000)throw new Error("size");
  const lines=text.replace(/\r\n?/g,"\n").split("\n");
  let cpu="",ram="",os="";
  const gpus=[];
  for(const original of lines){
    const line=original.trim();
    const match=line.match(/^([^:：]{2,45})\s*[:：]\s*(.+)$/);
    if(!match)continue;
    const key=match[1].trim().toLowerCase(),value=cleanDiagnosticField(match[2]);
    if(!cpu&&["processor","處理器","处理器"].includes(key))cpu=value;
    if(!ram&&["memory","記憶體","内存","記憶體容量"].includes(key))ram=value;
    if(!os&&["operating system","作業系統","操作系统"].includes(key))os=value;
    if(["card name","顯示卡名稱","显示卡名称"].includes(key) && value &&
      !gpus.includes(value) && gpus.length<5)gpus.push(value);
  }
  if(!cpu&&!ram&&!gpus.length)throw new Error("format");
  return Object.freeze({cpu,ram,os,gpus});
}
