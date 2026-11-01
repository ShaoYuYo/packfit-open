# Release checklist

Do not claim that this checklist has passed until tests are actually performed.

## Correctness and security

- [ ] npm run check and npm test pass on the release commit.
- [ ] Verify all new formulas against independently calculated public/synthetic examples.
- [ ] Reject malformed, excessive and untrusted file inputs.
- [ ] Do not expose personal, employer, supplier or customer battery data.
- [ ] Retain accurate risk and engineering assumptions.

## User experience

- [ ] Check the Overview, My Battery, Everyday Tools, Pack Studio, Compare and Methodology routes.
- [ ] Verify bilingual content, focus order, mobile layout, light/dark themes and reduced-motion setting.
- [ ] Use real browser import/export roundtrips and confirm rejected inputs are clearly explained.
- [ ] Verify Safari/Firefox fallback where live Battery Status API is unavailable.

## PWA and deployment

- [ ] Confirm every static dependency is in the service-worker cache list.
- [ ] Bump service-worker CACHE identifier for new versioned asset changes.
- [ ] Deploy the default branch and verify GitHub Pages actually loads.
- [ ] Test the offline website after first connecting to the network.
- [ ] Confirm browser installation behavior only on environments actually tested.

## Community and releases

- [ ] Update changelog, README, model assumptions and translations.
- [ ] Link issues/PRs that were truly resolved.
- [ ] Create an actual tag/release only after the above checks.
- [ ] Update docs/PROJECT_EVIDENCE.md only with independently verifiable community usage.

## v0.3 status

Source files, documentation and regression tests have been committed. Cross-browser execution, live deployment status, CI result and offline installability still require independent confirmation.
