# Fluent implementation choices — WinUI 3

The original desktop prototype used WinUI 3 but lacked the current native app-shell structure.

The refactor uses Microsoft's [modern WinUI 3 structure](https://learn.microsoft.com/en-us/windows/apps/develop/ui/windows-app-sdk-app-structure):

- \`Window.SystemBackdrop / MicaBackdrop\` for the Windows 11 native desktop material; Windows 10 falls back without hand-copied effects.
- \`TitleBar\` registered via \`ExtendsContentIntoTitleBar = true\` and \`SetTitleBar(AppTitleBar)\`. The native TitleBar owns the pane-toggle affordance and reserves Windows system caption-button space.
- \`NavigationView\` with icons and a fixed TitleBar hamburger toggle, for keyboard- and small-window-friendly navigation.
- \`InfoBar\` above the content viewport for success/failure messages without interrupting the engineering workflow.
- \`Expander\` and \`CheckBox\` for evidence-based help and optional advanced instructions.
- \`ProgressRing\` for nonblocking Windows system diagnostics.
- Theme resource brushes for cards; no opaque root page background that would hide Mica.

## Design decisions

Navigation is grouped around engineering work: Overview, Battery, Telemetry, Validation, Pack, Report, Investigation, Windows Diagnostics, and Official Tools. Each workspace contains the **primary action** near its inputs, with explanatory assumptions next to results, not hidden in a distant README.

Windows native light/dark contrast and Fluent typography are preferred over injecting web fonts or recreating acrylic with custom opacity hacks.

The code still needs manual review at common Windows DPI/text scale and window widths; static XAML validation and CI alone do not prove accessibility compliance.

## Build and API references

- [Microsoft app shell](https://learn.microsoft.com/en-us/windows/apps/develop/ui/windows-app-sdk-app-structure)
- [WinUI TitleBar + NavigationView integration](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/title-bar)
- [Mica material](https://learn.microsoft.com/en-us/windows/apps/design/style/mica)
- [Expander control](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/expander)
- [Windows powercfg commands](https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/powercfg-command-line-options)
