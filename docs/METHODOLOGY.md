# Calculation methodology / 計算方法

PackFit Open is an educational, first-pass **nominal** battery feasibility calculator. It does not simulate discharge chemistry, verify cell arrangement or establish safety/compliance.

## 1. Runtime estimate / 續航

**Runtime (hours) = Capacity (Wh) × (1 − Reserve %) × Conversion efficiency / Average load (W)**

Reserve and conversion efficiency are expressed as fractions between 0 and 1. Capacity is stored in Wh; average load is the modeled downstream load. Do not apply an extra efficiency derating to power data that already includes the full system's losses. If input power comes from wall-socket measurements, adjust methodology rather than assuming this formula directly models DC battery draw.

The Pack Studio's **ideal runtime** uses nominal Wh / selected W, with no reserve or conversion loss. Therefore the two workspace runtime values are not directly comparable unless reserve=0 and efficiency=100%.

No temperature model, cell-aging curve, voltage/capacity rate effect, low-voltage cutoff, EC/firmware behavior, sleep/standby profile or real load-time distribution is included.

## 2. Nominal pack electrical values / 電池組名目電氣規格

- Cell count N = Series S × Parallel P
- Pack nominal voltage Vpack = S × Vcell
- Pack rated capacity Ahpack = P × (cell rated mAh / 1000)
- Pack nominal energy Wh = Vpack × Ahpack = S × P × Vcell × (mAhcell / 1000)

These are scalar idealized estimates. They do **not** derive charge voltage, cutoffs, current limits, usable battery capacity, BMS characteristics or any safe charge/discharge strategy.

## 3. Pack mass / 重量

- Estimated mass (g) = N × per-cell mass (g) + manually entered non-cell/overhead mass (g)
- Gravimetric energy density (Wh/kg) = nominal Wh / (total mass in g / 1000)

Overhead must be supplied by the user. Do not substitute zero and then claim realistic pack-level energy density.

## 4. External envelope volume / 外部矩形包絡體積

- Volume (L) = X mm × Y mm × Z mm / 1,000,000
- Volumetric energy density (Wh/L) = nominal Wh / rectangular envelope volume (L)

This assumes a *bounding rectangular envelope*, not displacement volume, usable cavity volume, CAD volume or proof that cells and BMS can physically fit.

## 5. Warnings and units / 提示及單位

A prompt appears if nominal energy exceeds 100 Wh, to remind users that transport rules may apply. It is **not** an aviation or transportation compliance determination. Higher S/P counts and zero overhead mass also display modeling warnings.

All formulas use the displayed units. Internal values are calculated with JavaScript Number and only rounded for display; CSV retains full numeric values.

## 6. Validation and review / 輸入檢查

Input values must be finite and within bounded ranges; S and P must be positive integers. Numerical bounds are UI guardrails, not engineering safe operating limits.

For regressions run Node.js 20+: npm test. Model tests include 3S1P, parallel/series scaling, exact 100 Wh boundary, malformed input, runtime derating and CSV formula-injection safety.

## 7. Scope and safety

Do not use PackFit Open to design, construct, charge, repair or certify a physical battery pack, to establish flight/transport eligibility, or to determine safe operating limits. Physical products require qualified engineers, manufacturer datasheets, relevant standards and proper validation.

Report issues at https://github.com/ShaoYuYo/packfit-open/issues .
## 8. Consumer models

- **Charging cost**: nominal battery Wh / charging efficiency / 1000 × user-entered currency per kWh. Multiply by monthly full-charge equivalents for monthly cost.
- **Power bank**: cell-side rated mAh / 1000 × nominal cell voltage (V) yields Wh. Multiply by assumed conversion efficiency for estimated usable output; divide by target device battery Wh for full-charge equivalents. Real charging losses beyond power-bank output may further reduce charges.
- **Charge time**: rated battery Wh × (target SOC − start SOC) / 100 / (assumed battery charging power in W × efficiency) × 1.15 (heuristic buffer). **A 65W adapter does not mean the battery always charges at 65W.** Charging current tapers, especially near full charge.
- **Capacity retention**: Windows full-charge capacity Wh ÷ design capacity Wh × 100. This is not a comprehensive health or safety diagnostic.
- **Browser Battery Status API**: optional live percentage/charging state; browser-limited, may be rounded or absent; never exposes capacity retention reliably.
