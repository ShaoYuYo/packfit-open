export function formatNumber(value, locale = "zh-TW", digits = 1) {
  return Number.isFinite(value) ? new Intl.NumberFormat(locale, {maximumFractionDigits: digits,minimumFractionDigits: 0}).format(value) : "—";
}
export function formatDuration(hours, locale = "zh-TW") {
  if (!Number.isFinite(hours) || hours < 0) return "—";
  const total = Math.round(hours * 60);
  const h = Math.floor(total / 60);
  const m = total % 60;
  if (locale === "en") return h + "h " + String(m).padStart(2,"0") + "m";
  return h + " 小時 " + m + " 分";
}