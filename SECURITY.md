# Security policy

PackFit Open is a static browser application; it has no application server, account system, API credentials or remote telemetry. Saved scenarios and preferences live in localStorage on the user's device.

- Do not enter sensitive or proprietary battery specifications unless you accept they will be stored locally on that device.
- Do not copy unknown code into the repository or trust arbitrary CSV data as executable formulas.
- CSV exports prefix potentially dangerous spreadsheet formula cells to reduce formula-injection risk.
- The Google Fonts stylesheet is optional and external; the interface has local fallback fonts. For completely network-isolated use, remove the @import rule in styles.css.
- Keep browser and device security up to date.

If you discover a vulnerability, prefer private contact with the repository maintainer instead of publishing exploit details. If private reporting is not enabled, open an issue that omits sensitive exploit steps.

## v0.3 offline cache and scenario backups

The Service Worker caches only known static same-origin files under this GitHub Pages application's scope. It does not intercept arbitrary external requests or store imported report HTML. Clear site data to remove cached files and locally saved scenarios.

Imported portable scenario JSON is size-limited, version-checked, and numeric-field-whitelisted before being stored. Only trusted, canonicalized values may become editable scenarios; imported text is displayed through safe DOM text methods. Use public or synthetic example data when reporting bugs.

The web app's optional Google Fonts import makes a request to a third-party font provider while online; this does not send battery readings, scenario values or imported report content. Users who require a fully isolated deployment can remove that stylesheet import.
