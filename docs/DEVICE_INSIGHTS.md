# Device Insights: how it works and what it cannot do

Device Insights is a privacy-first browser resource overview, **not** a system inventory or a validated game/CPU/GPU benchmark.

## Automatically displayed browser hints

- `navigator.hardwareConcurrency`: browser-provided available **logical** processing threads, which may be lower than actual device threads.
- `navigator.deviceMemory`: optional and highly rounded approximate memory in GiB. Browser vendors may omit it or cap the reported value.
- `navigator.userAgentData.platform` or `navigator.platform`: a coarse operating platform hint, not a hardware model.
- `Boolean(navigator.gpu)`: whether the browser exposes the WebGPU API, **not** whether a specific graphics card can run a game.

The **resource overview** is shown only when both thread and coarse memory hints are present. Its low-confidence buckets describe reported multitasking resources, not measured speed:

| Browser-reported values | Description |
| --- | --- |
| At least 8 logical threads and 8 GiB | More headroom for multitasking |
| At least 4 logical threads and 4 GiB | Typical resource availability |
| Otherwise | More limited reported resources |
| Either input missing | Unknown; do not guess |

These thresholds are UI heuristics only, **not scientific cutoffs**, and cannot compare Intel, AMD, Qualcomm or Apple CPU performance. GPU identity, memory bandwidth, cooling, thermal throttling and drivers are not captured by these inputs.

## Optional graphics details

Click **Check GPU details** to create a tiny, local WebGL context and inspect WebGL support and maximum texture dimension. If the `WEBGL_debug_renderer_info` extension is available, it may reveal a renderer/driver label. The result can be generic, incomplete or hidden by privacy settings. Renderer strings are sanitized and only inserted into the page via `textContent`; nothing is uploaded or persisted.

WebGL support and reported maximum texture size are **not** gaming FPS benchmarks. This check does not expose a verified full GPU model list or all physically installed adapters.

## Optional local computation test

Click **Run local test** to launch an isolated Web Worker executing three runs of the same arithmetic workload. The page displays the median elapsed milliseconds and terminates the worker; it does not save an identifier or a benchmark history.

This test is deliberately **not** mapped onto fabricated performance tiers. Cross-browser timers, JIT engines, scheduler delays, power modes and thermal state strongly affect results. Compare only repeated runs in similar conditions in the same browser, with appropriate skepticism.

## Theme behavior

Appearance defaults to **System** and observes `matchMedia('(prefers-color-scheme: dark)')`. The UI immediately resolves the theme before the first visible content, to limit light/dark flashes. The setting can be changed to Light or Dark and later reset to System. Manual overrides are saved locally using a versioned preference field. Legacy earlier default colors migrate to System.

## Privacy

There is no registration, remote hardware collection, upload or tracking. New hardware hints, renderer labels and benchmarks are **in memory only**. External font and optional motion-CDN requests may still occur when online, but they are not sent hardware or report contents. In a privacy-restricted browser, hidden/unknown data stays unknown.

## Source references

- [MDN hardwareConcurrency](https://developer.mozilla.org/en-US/docs/Web/API/Navigator/hardwareConcurrency)
- [MDN deviceMemory](https://developer.mozilla.org/en-US/docs/Web/API/Navigator/deviceMemory)
- [MDN WEBGL_debug_renderer_info](https://developer.mozilla.org/en-US/docs/Web/API/WEBGL_debug_renderer_info)
- [MDN prefers-color-scheme](https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/At-rules/%40media/prefers-color-scheme)
- [MDN MediaQueryList change](https://developer.mozilla.org/en-US/docs/Web/API/MediaQueryList/change_event)

## Known limitations and validation needed

- Browser-dependent support and resource values.
- Full GPU identification unavailable on many privacy-minded devices.
- Performance probe is not standardized; desktop/mobile devices may behave very differently.
- Independent cross-browser, accessibility and real device QA is still required.
