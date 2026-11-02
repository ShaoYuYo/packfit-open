# PackFit release and engineering acceptance checklist

This checklist separates **automated evidence**, **manual acceptance**, and **external usage**. Checkboxes reflect actual evidence, not intentions. A green CI badge does not establish hardware behavior or independent adoption.

## Automation, release integrity

- [x] Browser syntax, calculation and integrity checks run in GitHub Actions. [Web CI](https://github.com/ShaoYuYo/packfit-open/actions/workflows/ci.yml).
- [x] Pure .NET engineering test vectors run in Windows CI. [Desktop workflow](https://github.com/ShaoYuYo/packfit-open/actions/workflows/desktop-winui.yml).
- [x] First public beta was built and published with Windows artifacts and SHA256 checksums. [v0.7.0-beta.1](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.1).
- [x] The second public beta [v0.7.0-beta.2](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.7.0-beta.2) was built from one verified checkout with browser, C# and Windows build gates. [Release workflow](https://github.com/ShaoYuYo/packfit-open/actions/workflows/publish-beta2.yml).
- [ ] Verify Windows binary signature and managed-device distribution before recommending corporate deployment.

## Accuracy and safety

- [x] Synthetic public datasets and formula-based test cases exist in the repo.
- [x] The Windows battery parser has test coverage for the installed-battery section, Chinese labels, historical capacity tables and two distinct batteries.
- [x] Telemetry CSV/TSV calculations support explicit electrical units, sign and user-defined thresholds.
- [ ] Independent domain review of formulas and atypical battery report structures.
- [ ] Check malformed, excessively large and unexpected supplier file formats on actual devices.
- [ ] Verify all thresholds against project-specific documentation before calling any measurement safe or unsafe.

## Native v0.8 visualization checks

- [x] Pure C# test vectors cover capacity history, user-entered standby SOC/Wh estimates, bounded chart samples and next-step recommendation rules.
- [x] WinUI full and compatible fallback code compiles on a hosted Windows runner.
- [x] Hosted Windows smoke tests instantiate both native workspaces, import synthetic sample data and construct the corresponding diagrams.
- [x] [v0.8.0-beta.1](https://github.com/ShaoYuYo/packfit-open/releases/tag/v0.8.0-beta.1) is publicly downloadable with SHA-256 hashes.
- [ ] Verify on a physical Windows laptop that all 5 diagrams render at different display scales and high contrast settings.
- [ ] Validate chart output with independent real, authorized and anonymized Windows battery reports and supplier-neutral Log fixtures.
- [ ] Check keyboard navigation, screen reader support and visibility of long issue text in narrow windows.
- [ ] Confirm correct warning and documentation when SleepStudy is unsupported or powercfg requires permissions.

## Manual user acceptance (not yet confirmed)

- [ ] Windows 11 x64: launch **the released binary** from the ZIP on a real laptop.
- [ ] Windows 10 x64: launch and verify fallback window materials and accessible navigation.
- [ ] Multi-battery test with an authorized, anonymized real report; verify no capacity mixing.
- [ ] SleepStudy / energy / powercfg checks on supported devices and correct permissions.
- [ ] Test responsive layout, focus order, high DPI and screen reader.
- [ ] Web trial on Chrome, Firefox, Safari/iOS including Battery API permission/fallback.
- [ ] PWA offline behavior, reload and scenario import/export round-trip.

## Community evidence (not yet confirmed)

- [ ] Receive at least one concrete, reproducible report from a genuine trial user.
- [ ] Show issue triage and a test-backed fix in response to that report.
- [ ] Obtain at least one independent code review or mathematical cross-check.
- [ ] Collect consented real-world use feedback without exposing company or personal data.
- [ ] Publish follow-up releases for bugfixes and keep the roadmap and known limitations updated.

Use [the 15-minute trial](TRY_PACKFIT.md). Never purchase Stars, invent feedback or upload private reports to public GitHub.

## Final release gate

A release build must pass browser CI and .NET core checks, build the Windows app on an actual Windows runner, attach binaries/hashes, and document limitations. A release with these automated properties **remains a beta until manual hardware acceptance is recorded**.
