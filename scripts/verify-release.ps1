#requires -Version 5.1
<#
  .SYNOPSIS
  Inspects a built release zip and fails on anything that would break a first-time install.

  .DESCRIPTION
  Independent second gate over the finished dist/ zip. Does not import or call
  package-release.ps1's self-check; this re-opens the artifact cold and re-derives every fact
  from the bytes on disk, the same way a player's unzip would.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $ZipPath
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$zip = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path $ZipPath))
try {
    $names = $zip.Entries.FullName
    $fail  = @()

    # Every comparison below against an entry FullName uses the -c (case-sensitive) operator
    # variant deliberately. PowerShell's default -notcontains / -notlike / -match are
    # case-INSENSITIVE, but zip entry names are case-SENSITIVE on the macOS and Linux
    # filesystems this gate exists to protect: a packaging regression that emitted
    # "Manifest.json" instead of "manifest.json" would pass every check here under the
    # case-insensitive default and then break for players. Do not "simplify" these back to the
    # bare operators.

    $required = @(
        'ChattyValley/manifest.json'
        'ChattyValley/config.json'
        'ChattyValley/ChattyValley.Mod.dll'
        'ChattyValley/ChattyValley.Core.dll'
        'ChattyValley/README.txt'
        'ChattyValley/LICENSE-LFM.txt'
        'ChattyValley/characters/linus.json'
        'ChattyValley/assets/LFM2.5-1.2B-Instruct-Q4_K_M.gguf'
        'ChattyValley/assets/linus-12b-v8dpo2-lora-f16.gguf'
        'ChattyValley/sidecar/ChattyValley.Sidecar.exe'
    )
    foreach ($r in $required) { if ($names -cnotcontains $r) { $fail += "missing: $r" } }

    # the 350M pair must never ship
    foreach ($n in $names) { if ($n -cmatch '350[Mm]') { $fail += "stale 350M artifact shipped: $n" } }

    # Entry paths must use forward slashes. .NET Framework's CreateFromDirectory writes
    # backslashes, which extract as one literally-named file on macOS and Linux.
    $backslashed = @($names | Where-Object { $_ -cmatch '\\' })
    if ($backslashed.Count) { $fail += "$($backslashed.Count) entries use backslash separators (ZIP spec requires '/')" }

    # Every entry must sit under ChattyValley/. A prior defect truncated entry names to
    # "y/manifest.json": forward slashes were correct and the entry count matched, so a check
    # that only tests those two properties sails straight past it. Checking the required list
    # above would catch it only if the corrupted name happened to collide with a required path;
    # this checks every entry, required or not.
    $misprefixed = @($names | Where-Object { $_ -cnotlike 'ChattyValley/*' })
    if ($misprefixed.Count) {
        $sample = ($misprefixed | Select-Object -First 5) -join ', '
        $fail += "$($misprefixed.Count) entries are not under the 'ChattyValley/' prefix: $sample"
    }

    # a LLamaSharp assembly in the mod root means the thin-client boundary broke
    foreach ($n in $names) {
        if ($n -cmatch '^ChattyValley/LLama.*\.dll$') { $fail += "LLamaSharp leaked into the mod root: $n" }
    }

    # Minimum payload sizes. Presence-by-name alone lets a zero-byte or truncated GGUF, a stub
    # DLL, or a sidecar exe missing its bundled runtime all pass silently, and every defect on
    # this plan so far has produced an artifact of plausible shape. Thresholds are set well
    # below the real payload sizes (base GGUF 697.0 MB, adapter 21.2 MB, sidecar exe 162,816
    # bytes) so a legitimate model swap does not trip them.
    $minBytes = @{
        'ChattyValley/assets/LFM2.5-1.2B-Instruct-Q4_K_M.gguf' = 600MB
        'ChattyValley/assets/linus-12b-v8dpo2-lora-f16.gguf'   = 15MB
        'ChattyValley/sidecar/ChattyValley.Sidecar.exe'        = 50KB
        'ChattyValley/ChattyValley.Mod.dll'                    = 8KB
        'ChattyValley/ChattyValley.Core.dll'                   = 4KB
    }
    foreach ($k in $minBytes.Keys) {
        $e = $zip.GetEntry($k)
        if ($e -and $e.Length -lt $minBytes[$k]) {
            $fail += "$k is only $([math]::Round($e.Length/1KB,1)) KB, expected at least $([math]::Round($minBytes[$k]/1KB,1)) KB"
        }
    }

    # A self-contained win-x64 publish always carries hostfxr.dll alongside the exe. Its absence
    # means the sidecar was published framework-dependent and will not run on a player machine
    # without the .NET runtime installed.
    if ($names -cnotcontains 'ChattyValley/sidecar/hostfxr.dll') {
        $fail += 'sidecar is missing hostfxr.dll, so it was not published self-contained'
    }

    function Read-Entry([string] $path) {
        $e = $zip.GetEntry($path); if (-not $e) { return $null }
        $r = New-Object System.IO.StreamReader($e.Open())
        try { $r.ReadToEnd() } finally { $r.Dispose() }
    }

    # Read-Json guards two distinct crash paths, both of which abort the script before the
    # collected $fail list is ever printed if left unguarded:
    #   1. the exact forward-slash path is not found in the archive (Read-Entry returns $null;
    #      piping $null into ConvertFrom-Json throws a hard parameter-binding exception), and
    #   2. the entry exists and is non-empty but is not valid JSON, e.g. a config truncated
    #      mid-write ("{"Temperature": 0.3"); ConvertFrom-Json throws System.ArgumentException.
    # Both are real, reachable defects on this plan, not theoretical: a fully-backslashed zip
    # hits (1) and a mid-write-truncated config hits (2). Either one must degrade to a reported
    # FAIL line, not a stack trace.
    function Read-Json([string] $path, [ref] $errOut) {
        $text = Read-Entry $path
        if ($null -eq $text) { $errOut.Value = "$path not readable"; return $null }
        try { return $text | ConvertFrom-Json }
        catch { $errOut.Value = "$path is not valid JSON: $($_.Exception.Message)"; return $null }
    }

    $cfgErr = $null
    $cfg = Read-Json 'ChattyValley/config.json' ([ref]$cfgErr)
    if ($null -eq $cfg) {
        $fail += "cannot parse config: $cfgErr"
    } else {
        if ($cfg.Temperature -ne 0.35)    { $fail += "config Temperature is $($cfg.Temperature), expected 0.35" }
        if ($cfg.ChatLogEnabled -ne $false) { $fail += 'config ChatLogEnabled is true, expected false' }
        if ($cfg.FalsePremiseGuard -ne $true) { $fail += 'config FalsePremiseGuard is false, expected true' }
        if ($cfg.BaseModelPath -ne '')    { $fail += "config BaseModelPath is not blank: $($cfg.BaseModelPath)" }
        if ($cfg.LinusAdapterPath -ne '') { $fail += "config LinusAdapterPath is not blank: $($cfg.LinusAdapterPath)" }
    }

    $manErr = $null
    $man = Read-Json 'ChattyValley/manifest.json' ([ref]$manErr)
    if ($null -eq $man) {
        $fail += "cannot parse manifest: $manErr"
    } else {
        if (-not $man.UpdateKeys -or $man.UpdateKeys.Count -eq 0) { $fail += 'manifest has no UpdateKeys' }
    }

    $linusErr = $null
    $linus = Read-Json 'ChattyValley/characters/linus.json' ([ref]$linusErr)
    if ($null -eq $linus) {
        $fail += "cannot parse linus.json: $linusErr"
    } elseif (-not $linus.Name) {
        $fail += 'ChattyValley/characters/linus.json has no Name field'
    }

    # no absolute developer paths anywhere in the shipped text files
    foreach ($t in 'ChattyValley/config.json', 'ChattyValley/manifest.json', 'ChattyValley/README.txt') {
        $body = Read-Entry $t
        if ($body -cmatch '[A-Za-z]:\\Users\\') { $fail += "absolute developer path leaked in $t" }
    }

    if ($fail.Count) {
        $fail | ForEach-Object { Write-Host "FAIL  $_" }
        throw "$($fail.Count) release check(s) failed"
    }
    Write-Host "release OK: $ZipPath"
}
finally { $zip.Dispose() }
