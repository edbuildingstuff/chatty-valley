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
    foreach ($r in $required) { if ($names -notcontains $r) { $fail += "missing: $r" } }

    # the 350M pair must never ship
    foreach ($n in $names) { if ($n -match '350[Mm]') { $fail += "stale 350M artifact shipped: $n" } }

    # Entry paths must use forward slashes. .NET Framework's CreateFromDirectory writes
    # backslashes, which extract as one literally-named file on macOS and Linux.
    $backslashed = @($names | Where-Object { $_ -match '\\' })
    if ($backslashed.Count) { $fail += "$($backslashed.Count) entries use backslash separators (ZIP spec requires '/')" }

    # Every entry must sit under ChattyValley/. A prior defect truncated entry names to
    # "y/manifest.json": forward slashes were correct and the entry count matched, so a check
    # that only tests those two properties sails straight past it. Checking the required list
    # above would catch it only if the corrupted name happened to collide with a required path;
    # this checks every entry, required or not.
    $misprefixed = @($names | Where-Object { $_ -notlike 'ChattyValley/*' })
    if ($misprefixed.Count) {
        $sample = ($misprefixed | Select-Object -First 5) -join ', '
        $fail += "$($misprefixed.Count) entries are not under the 'ChattyValley/' prefix: $sample"
    }

    # a LLamaSharp assembly in the mod root means the thin-client boundary broke
    foreach ($n in $names) {
        if ($n -match '^ChattyValley/LLama.*\.dll$') { $fail += "LLamaSharp leaked into the mod root: $n" }
    }

    function Read-Entry([string] $path) {
        $e = $zip.GetEntry($path); if (-not $e) { return $null }
        $r = New-Object System.IO.StreamReader($e.Open())
        try { $r.ReadToEnd() } finally { $r.Dispose() }
    }

    # Read-Entry returns $null when the exact forward-slash path was not found in the archive
    # (e.g. every entry got mangled to backslashes, or lost its ChattyValley/ prefix). The
    # required-file loop above already records that as "missing: ...", but ConvertFrom-Json
    # rejects a $null pipeline input with a hard parameter-binding exception rather than
    # returning $null, which would otherwise abort the script before the collected $fail list
    # ever gets printed. Guard each parse so a structurally-broken zip still reports every
    # other failure instead of crashing on the first one.
    $cfgText = Read-Entry 'ChattyValley/config.json'
    if ($null -eq $cfgText) {
        $fail += 'cannot parse config: ChattyValley/config.json not found at that exact path'
    } else {
        $cfg = $cfgText | ConvertFrom-Json
        if ($cfg.Temperature -ne 0.35)    { $fail += "config Temperature is $($cfg.Temperature), expected 0.35" }
        if ($cfg.ChatLogEnabled -ne $false) { $fail += 'config ChatLogEnabled is true, expected false' }
        if ($cfg.FalsePremiseGuard -ne $true) { $fail += 'config FalsePremiseGuard is false, expected true' }
        if ($cfg.BaseModelPath -ne '')    { $fail += "config BaseModelPath is not blank: $($cfg.BaseModelPath)" }
        if ($cfg.LinusAdapterPath -ne '') { $fail += "config LinusAdapterPath is not blank: $($cfg.LinusAdapterPath)" }
    }

    $manText = Read-Entry 'ChattyValley/manifest.json'
    if ($null -eq $manText) {
        $fail += 'cannot parse manifest: ChattyValley/manifest.json not found at that exact path'
    } else {
        $man = $manText | ConvertFrom-Json
        if (-not $man.UpdateKeys -or $man.UpdateKeys.Count -eq 0) { $fail += 'manifest has no UpdateKeys' }
    }

    # no absolute developer paths anywhere in the shipped text files
    foreach ($t in 'ChattyValley/config.json', 'ChattyValley/manifest.json', 'ChattyValley/README.txt') {
        $body = Read-Entry $t
        if ($body -match '[A-Za-z]:\\Users\\') { $fail += "absolute developer path leaked in $t" }
    }

    if ($fail.Count) {
        $fail | ForEach-Object { Write-Host "FAIL  $_" }
        throw "$($fail.Count) release check(s) failed"
    }
    Write-Host "release OK: $ZipPath"
}
finally { $zip.Dispose() }
