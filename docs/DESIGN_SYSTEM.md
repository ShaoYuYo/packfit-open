# Visual and interaction guidelines — PackFit Open v0.4

PackFit is a practical calculator. Users should understand where inputs go and what the result means before noticing any decoration.

## Language and product voice

- Explain actions and assumptions directly: “電池還能用多久？算過就知道。” is more useful than generic promises about innovation.
- Say **estimate**, **illustrative**, or **not measured** when results are models or example numbers.
- Prefer short Traditional Chinese phrases that a person in Taiwan would actually say.
- Avoid repeated all-caps slogans, forced metaphors, decorative icons, and technical claims that the app cannot substantiate.
- Do not imply browser Battery Status API access exposes battery health, cell data or cycle counts.

## Typography

- **Inter** for Latin text and tabular numeric UI; **Noto Sans TC** for Traditional Chinese. Use local PingFang TC, Microsoft JhengHei and standard system sans-serif as fallbacks.
- Both Google Fonts families are open-source under the SIL Open Font License. No font file is redistributed with this repository.
- Minimize font families and avoid excess uppercase labels.
- Main reading copy targets 13–14 px on desktop; input labels use 12 px or larger and standard controls stay at least 44 px tall. Heading size scales down on phones.
- Keep visible focus rings and reasonable text scaling; test at 200% browser zoom.

## Color and layout

- Quiet neutral surfaces, a single green accent for controls/results, generous gutters, subtle borders.
- Use typography and spacing first; do not stack generic gradients/glass effects to force a “premium” feeling.
- Charts, battery status and form values must remain distinguishable without relying on color alone.
- The 56 Wh hero graphic is clearly marked DEMO DATA, not an actual battery readout.

## Motion

- Never add continuous floating, pulsing or rotating marketing animations.
- Limit feedback to brief page entry, focus, hover and changed calculation values.
- Prefer opacity and transform for smooth compositing; avoid animations that block interaction or change the meaning of values.
- Honor prefers-reduced-motion even when the system preference changes while the page is open.
- Do not animate a status in a way that implies actual hardware measurement or a changing reading.
- No autoplay sounds, scroll hijacking or parallax.

## Implementation

- `styles.css`: core layout and global defaults.
- `assets/premium.css`: original v0.2 feature layouts.
- `assets/editorial.css`: v0.4 visual layer and motion cleanup. This deliberately overrides decorative effects from older styles; a later refactor can consolidate duplicated rules.
- `src/app.js`: small 170 ms animation on changed important calculation numbers; only when motion is allowed.
- `sw.js`: cache revision required whenever a CSS/JS file changes.
- No Apple proprietary typefaces, screenshots, artwork or assets are copied. Design follows general clarity and interaction principles.

## References

- [Apple Human Interface Guidelines — Motion](https://developer.apple.com/design/human-interface-guidelines/motion)
- [Apple Human Interface Guidelines — Typography](https://developer.apple.com/design/human-interface-guidelines/typography)
- [Chrome for Developers — Motion](https://developer.chrome.com/docs/modern-web-guidance/guides/ui-behaviors/motion)
- [MDN — Reduced motion](https://developer.mozilla.org/en-US/docs/Web/CSS/Guides/Media_queries/Using_for_accessibility)
- [Noto Sans TC metadata and OFL](https://github.com/google/fonts/blob/main/ofl/notosanstc/METADATA.pb)

Visual appearance still requires independent desktop/mobile and assistive technology QA; code changes alone do not prove accessibility compliance.
