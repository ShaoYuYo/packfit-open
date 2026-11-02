# PackFit Open — Quick User Guide

Use https://shaoyuyo.github.io/packfit-open/ without creating an account or entering an API key.

## Workspace overview

- **Overview:** enter battery Wh and average W for a simple runtime estimate.
- **Daily plan:** enter current charge, minimum reserve, work/video/game duration and average power. The example watts are assumptions, not measurements.
- **My Battery:** opt in to browser-level charge status if supported. Import a Windows report or enter capacities to estimate capacity retention.
- **Everyday Tools:** charging cost, power-bank energy and approximate recharge time.
- **Pack Studio:** nominal series/parallel, pack energy and density estimates.
- **Compare:** back up up to four editable design scenarios to JSON, restore them on another device, or export CSV.

## Windows battery report

Press Win + R, type cmd, then execute:

    powercfg /batteryreport /output "%USERPROFILE%\Desktop\battery-report.html"

Import the HTML report using My Battery. If Desktop is redirected, check the actual file path printed by Windows. You can also manually enter the design capacity and full-charge capacity in Wh.

Capacity retention (%) = full-charge capacity / design capacity × 100. This is not an electrical safety or lifetime diagnosis. Imported reports stay in browser memory and are not uploaded; never post reports containing device identifiers in public issues.

## Example day plan

56 Wh battery at 80%, 10% reserve, office 2 h × 10 W and video 1 h × 14 W: activities need 34 Wh. Energy above reserve is 39.2 Wh. Predicted ending state is approximately 19.3%, based entirely on assumed average watts.

## Backups and offline use

Compare → Backup JSON stores editable parameters; Import JSON restores them and replaces previous saved scenarios after confirmation. CSV is for spreadsheet review, not restoration. Try the [synthetic examples](../examples/sample-scenarios.json).

After a successful online visit, a supported secure browser can cache the static app for offline use. Installation and cache persistence depend on browser and device settings; external Google Fonts may fall back to local system fonts.

This is an educational estimator, not a tool for battery fabrication, repair, BMS certification or flight eligibility. Read [methodology](METHODOLOGY.md) and [support](../SUPPORT.md).

## Multi-battery Windows reports (web v0.6.3)

For Windows `powercfg /batteryreport` HTML files, the importer now reads the **Installed batteries** section without confusing later capacity-history tables with currently installed batteries. If the report includes multiple physical batteries, select an individual battery from the dropdown. Each battery's design and full-charge capacities are calculated separately and are **not silently combined**. Applying its capacity to other calculators uses the **currently selected battery**, not the whole device.

Ambiguous multi-column layouts, missing capacity units and reports over 2 MB are rejected. Failed imports clear the previous capacity result so you cannot accidentally use stale information. For unusual formats, enter independently verified Wh values manually. Capacity retention is an estimate, not a safety or runtime certification.

Files are parsed in your browser and are never uploaded. Original Windows reports may contain sensitive system identifiers; do not post unredacted reports publicly.
