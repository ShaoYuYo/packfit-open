## User problem and linked issue

Describe the observed problem, why it matters, the expected behavior, and the smallest safe change. Link an issue when available.

## Exact changes and evidence

- Affected modules and supported input formats:
- Units, clock offsets, discharge-current sign and threshold assumptions:
- Synthetic fixture or redacted reproduction (never proprietary logs):
- What *remains unverified* after this PR:

## Verification (record what actually ran)

- [ ] `npm run check` — browser syntax
- [ ] `npm test` — JS and structural regressions
- [ ] `dotnet run --project desktop/PackFit.Engineering.Checks` — pure C# vectors (or confirm Windows CI)
- [ ] WinUI Windows CI: native build, main-window XAML and independent compatible-shell smoke tests
- [ ] Affected file and large-input limits / error messages tested
- [ ] Keyboard, reduced-motion and narrow-window checks if changing UI
- [ ] English / 繁體中文 and actual user instructions reviewed
- [ ] Public test data is synthetic or explicitly authorized
- [ ] No claim of safety certification, automatic root cause or independent adoption

## Maintainer review

- Relevant test result or GitHub Actions URL:
- Whether this was tested on a physical Windows machine (OS/build, no identifiers):
- Security/privacy or regression risk:
- Documentation/release notes updated:
- Human reviewer and follow-up owner (if any):

**AI-assisted contributions are welcome, but the maintainer remains responsible for the diff, test evidence, security and public claims.**
