# Security policy

PackFit Open is a static browser application; it has no application server, account system, API credentials or remote telemetry. Saved scenarios and preferences live in localStorage on the user's device.

- Do not enter sensitive or proprietary battery specifications unless you accept they will be stored locally on that device.
- Do not copy unknown code into the repository or trust arbitrary CSV data as executable formulas.
- CSV exports prefix potentially dangerous spreadsheet formula cells to reduce formula-injection risk.
- The Google Fonts stylesheet is optional and external; the interface has local fallback fonts. For completely network-isolated use, remove the @import rule in styles.css.
- Keep browser and device security up to date.

If you discover a vulnerability, prefer private contact with the repository maintainer instead of publishing exploit details. If private reporting is not enabled, open an issue that omits sensitive exploit steps.