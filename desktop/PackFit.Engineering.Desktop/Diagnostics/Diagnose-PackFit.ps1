param(
    [switch]$SafeUi,
    [switch]$InventoryOnly
)

# Local-only diagnostic capture. Nothing is uploaded or sent to GitHub.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $root 'PackFit.Engineering.exe'
$logs = Join-Path $env:LOCALAPPDATA 'PackFitOpen\Logs'
$log = Join-Path $logs 'launch-check.txt'
New-Item -ItemType Directory -Path $logs -Force | Out-Null
$started = Get-Date
$output = New-Object System.Collections.Generic.List[string]
function Write-Report([string]$line) {
    $output.Add($line)
    Write-Host $line
}
Write-Report '=== PackFit Engineering — local startup diagnostic ==='
Write-Report ('Captured local time: ' + $started.ToString('yyyy-MM-dd HH:mm:ss'))
Write-Report ('OS version: ' + [Environment]::OSVersion.Version)
Write-Report ('64-bit OS: ' + [Environment]::Is64BitOperatingSystem)
Write-Report ('Program file present: ' + (Test-Path $exe))
if (-not (Test-Path $exe)) {
    Write-Report 'ERROR: PackFit.Engineering.exe is missing. Extract the complete official ZIP first.'
    $output | Set-Content -LiteralPath $log -Encoding UTF8
    exit 2
}
$files = @(Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue)
Write-Report ('Files beside EXE: ' + $files.Count)
Write-Report ('DLLs beside EXE: ' + @($files | Where-Object Extension -eq '.dll').Count)
Write-Report ('Single-file-looking folder: ' + ($files.Count -lt 10))
Write-Report 'Reminder: EXE-only copies of the folder-based distribution cannot run.'

if (-not $InventoryOnly) {
    try {
        $args = @()
        if ($SafeUi) {
            $args = @('--safe-ui')
            Write-Report 'Launching with safe UI mode (no custom title bar or optional transitions).'
        }
        $process = Start-Process -FilePath $exe -WorkingDirectory $root -ArgumentList $args -PassThru
        Start-Sleep -Seconds 6
        $process.Refresh()
        if ($process.HasExited) {
            Write-Report ('Process exited during startup. Exit code: ' + $process.ExitCode)
        }
        else {
            Write-Report 'Process was still running after 6 seconds; inspect the window.'
            Write-Report 'A running process is not proof the interface fully initialized.'
        }
    }
    catch {
        Write-Report ('Windows could not launch the executable: ' + $_.Exception.Message)
    }
    try {
        $events = @(Get-WinEvent -FilterHashtable @{
            LogName = 'Application'
            StartTime = $started.AddMinutes(-2)
        } -ErrorAction SilentlyContinue |
            Where-Object {
                $_.Message -match 'PackFit\.Engineering|Microsoft\.UI\.Xaml|WindowsAppRuntime'
            } | Select-Object -First 8)
        foreach ($event in $events) {
            Write-Report ('Windows event: ' + $event.TimeCreated + ' / ' +
                $event.ProviderName + ' / ID ' + $event.Id)
            # The event text may reveal private paths; keep it local.
            $excerpt = [string]$event.Message
            if ($excerpt.Length -gt 850) { $excerpt = $excerpt.Substring(0,850) }
            Write-Report $excerpt
        }
        if ($events.Count -eq 0) {
            Write-Report 'No matching Windows Application event was found in the captured time window.'
        }
    }
    catch {
        Write-Report ('Event Log access unavailable: ' + $_.Exception.Message)
    }
}

$startup = Join-Path $logs 'startup.log'
Write-Report ('PackFit startup log present: ' + (Test-Path $startup))
Write-Report 'Privacy: the diagnostic report can include user folder names and local paths.'
Write-Report 'Review and redact the report before sharing it anywhere.'
$output | Set-Content -LiteralPath $log -Encoding UTF8
Write-Host ('Local diagnostic report: ' + $log)
