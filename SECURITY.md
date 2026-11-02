# Security and privacy policy

PackFit Open has **two local-first clients**: a static browser application hosted on GitHub Pages and a self-contained C#/WinUI 3 Windows workbench. The project does **not** operate an application backend, user account system, report-upload API or remote telemetry endpoint.

## What goes over the network

- Opening the public site downloads the page and its same-origin static resources from GitHub Pages. Once cached, its PWA can operate offline within supported browser conditions.
- The default browser interface uses **system fonts and the browser-native Web Animations API**. It does not need to fetch Google Fonts or load executable animation modules from a third-party CDN.
- Users can explicitly click official documentation links (GitHub, Microsoft, OpenAI). Those links navigate to external sites; they are not automatic upload destinations.
- The Windows app may open a browser for the user's chosen official tool/documentation links. Battery data, CSV, HTML and Markdown are analyzed locally.

**Important:** no-telemetry is not the same as zero network traffic: visiting the public GitHub Pages site inherently contacts its host, and external links contact their own destinations.

## Input and local data boundaries

- Never put proprietary employer, customer or supplier hardware specs into public issues, screenshots or fixtures.
- Browser scenarios/settings may be stored in localStorage. Clear the site's data to remove locally saved content and PWA cache.
- Windows battery reports, CSV/TSV measurement files and validation matrices can contain usernames, machine identifiers, model data and private timestamps. They remain local, but are potentially sensitive **on disk**.
- The native workbench enforces bounded byte reads for imported HTML/CSV files as an additional safeguard against oversized/unexpected inputs. The format parsers also reject ambiguous data and unsupported units.
- The Windows startup diagnostics write a bounded log under `%LOCALAPPDATA%\PackFitOpen\Logs\startup.log`. Exception text may contain local file paths; **redact before sharing**.
- Spreadsheet exports prefix potentially dangerous formula cells. Imported HTML/CSV is treated as data, not evaluated as script.
- None of these controls guarantees a file is safe, or substitutes for OS security, antivirus or corporate endpoint policy.

## Official Windows binary distribution

- GitHub Releases provide a complete ZIP containing WinUI runtime dependencies; `PackFit.Engineering.exe` is an optional self-extracting distribution.
- Windows binaries are currently **unsigned**. Never disable Windows SmartScreen or bypass employer security policy merely to install a beta.
- Release assets have SHA-256 checksum files. New Windows CI artifacts are intended to have GitHub build provenance attestations; confirm a successful attestation in the relevant workflow before claiming any particular binary is attested.
- Building on GitHub Actions and passing CodeQL do not guarantee a binary is safe or compatible with every Windows setup.
- The software does not implement charging firmware, inject kernel drivers, alter Windows power plans or establish any regulatory/safety certification.

## Reporting a vulnerability privately

Use the GitHub repository's **Security → Report a vulnerability** flow if private vulnerability reporting is enabled. If it is not, contact the repository owner privately through an appropriate GitHub channel; do not publish exploit chains, secrets, raw device reports or unredacted local logs. A public issue can be used for a general non-sensitive reproducible symptom.

Please include an affected version, minimal synthetic reproduction, possible impact and suggested mitigation. The maintainer will investigate, attempt to reproduce, document the fix and coordinate appropriate disclosure. There is currently **no guaranteed response SLA or formal external security audit**.

See [maintenance workflow](docs/MAINTAINER_WORKFLOW.md), [engineering limitations](docs/METHODOLOGY.md), and [Release checklist](docs/RELEASE_CHECKLIST.md).
