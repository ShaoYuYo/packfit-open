# PackFit Premium Workbench: UX and evidence integrity

This is an implementation note for the native WinUI engineering workbench. It does not claim a hardware certification, a usability study, or formal accessibility compliance.

## Design direction

- Use Windows 11's native **Mica**, **Segoe UI Variable**, **NavigationView**, system theme brushes and native focus visuals; do not load web fonts into the desktop application.
- Keep section titles short and functional, with a restrained accent rail and readable typography rather than excessive decorative color.
- Show **data provenance** at the top of the workspace. Users must know whether a summary is based on their imported data, synthetic samples, manually entered values, or a mixture.
- Keep important actions keyboard friendly: **Ctrl+S** on the overview exports the Markdown engineering handoff.
- Reset scrolling on page changes and stack command groups at narrower window widths.
- Prioritize explicit empty, error and incomplete states. A screen with no evidence is not a PASS result.

## Workflows

1. Open **工程工作台** and review the source status hero and each metric's source caption.
2. Use **載入合成示範** only for the synthetic walkthrough. Replacing an existing session requires confirmation.
3. In **量測 CSV 分析**, inspect complete finding-code totals. Search by issue code, severity, row or text to filter the first 1,000 retained detailed findings. The UI shows at most 250 at once and explicitly reports how many more details are unretained; a filtered list is **not** a whole-log search.
4. Use **清除分析資料** to clear data and chart state without clearing the engineer's threshold/project settings or deleting already-exported files.
5. Export a Markdown report. Its first section lists the source of battery, telemetry, validation, pack, batch, and standby results. Any synthetic fixture triggers a conspicuous demo warning.

## Acceptance and outstanding QA

- [ ] JavaScript contract and syntax tests on GitHub Actions.
- [ ] .NET native WinUI folder publish and real MainWindow XAML-load smoke test.
- [ ] Native chart/demo smoke tests, including a sample watermark and reset contract.
- [ ] Accessibility: screen reader, keyboard focus and 200% text scale on an actual laptop.
- [ ] Window sizing at ~700, ~900 and ~1400 effective pixels.
- [ ] Manual imported-log and two-battery HTML tests on a real Windows 10/11 laptop.
- [ ] Confirm that the full, unsigned ZIP opens on the original user hardware.

The sample fixtures are entirely synthetic. Do not submit employer, supplier or customer measurements to this public repository.
