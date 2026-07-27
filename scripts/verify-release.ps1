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

$resolvedZipPath = (Resolve-Path $ZipPath).Path
$zip = [System.IO.Compression.ZipFile]::OpenRead($resolvedZipPath)
try {
    $names = $zip.Entries.FullName
    $fail  = @()

    # The two shipping GGUF basenames, defined once here so the required-file list, the size
    # thresholds, and the DLL cross-check below all agree with each other and cannot silently
    # drift apart the way the DLL, package-release.ps1, and this script's required list once could.
    $baseGguf    = 'LFM2.5-1.2B-Instruct-Q4_K_M.gguf'
    $adapterGguf = 'linus-12b-v8dpo2-lora-f16.gguf'

    # The zip filename's version must agree with the manifest inside it (checked once $man is
    # parsed below). Extracted here from the resolved path so a relative -ZipPath still works.
    $zipLeaf = Split-Path $resolvedZipPath -Leaf
    $zipVersion = $null
    if ($zipLeaf -cmatch '^ChattyValley-(.+)\.zip$') {
        $zipVersion = $Matches[1]
    } else {
        $fail += "zip filename '$zipLeaf' does not match the expected ChattyValley-<version>.zip pattern, so its version cannot be checked against the manifest"
    }

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
        "ChattyValley/assets/$baseGguf"
        "ChattyValley/assets/$adapterGguf"
        'ChattyValley/sidecar/ChattyValley.Sidecar.exe'
        # These two are the actual sidecar entry assembly and the runtime glue that loads the
        # native llama.cpp backend. Neither was previously asserted: a LLamaSharp bump or RID
        # change could drop either one, pass every other check, and die on model load at runtime.
        'ChattyValley/sidecar/ChattyValley.Sidecar.dll'
        'ChattyValley/sidecar/ChattyValley.Runtime.dll'
    )
    foreach ($r in $required) { if ($names -cnotcontains $r) { $fail += "missing: $r" } }

    # At least one win-x64 native llama.dll backend (avx / avx2 / avx512 / noavx) must ship under
    # the sidecar. This is the actual inference engine; its absence was previously unchecked and
    # would pass every other assertion while dying on model load for every player.
    $llamaNative = @($names | Where-Object { $_ -cmatch '^ChattyValley/sidecar/runtimes/win-x64/native/[^/]+/llama\.dll$' })
    if ($llamaNative.Count -eq 0) {
        $fail += 'sidecar ships no runtimes/win-x64/native/*/llama.dll backend; it will die on model load for every player'
    }

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

    # The shipping GGUF filenames live in six places (ModEntry.cs, package-release.ps1, this
    # script, and three docs/tests) with nothing that ties them together: a model retrain that
    # renames the GGUFs, with the two obvious "release" scripts updated but ModEntry.cs missed
    # (or vice versa), builds a zip, passes every check above, and then File.Exists is false for
    # every player and free-chat silently disables itself forever. Close that gap here by reading
    # the actual compiled ChattyValley.Mod.dll and asserting the basenames it resolves are the
    # ones the zip ships.
    #
    # The filenames are embedded as UTF-16LE string literals in the #US metadata heap, but a
    # heap entry's byte OFFSET within the file is not guaranteed to be even (it follows a
    # variable-length compressed length prefix), so decoding the whole DLL as one Unicode string
    # from byte 0 and searching for the literal can silently mis-pair every byte from that offset
    # onward and never find it, a false FAIL that would make this gate permanently red. Confirmed
    # against the real build: the base GGUF literal here starts at file offset 34641, which is
    # odd. Search instead via ISO-8859-1 (Latin-1), a single-byte-to-single-char encoding with no
    # pairing/alignment concept at all, so the exact UTF-16LE byte sequence of the search term is
    # found as a substring regardless of where it falls.
    $modDllEntry = $zip.GetEntry('ChattyValley/ChattyValley.Mod.dll')
    if ($modDllEntry) {
        $ms = New-Object System.IO.MemoryStream
        $s = $modDllEntry.Open()
        try { $s.CopyTo($ms) } finally { $s.Dispose() }
        $latin1 = [System.Text.Encoding]::GetEncoding('ISO-8859-1')
        $modDllBytes = $latin1.GetString($ms.ToArray())
        $ms.Dispose()

        $baseNeedle    = $latin1.GetString([System.Text.Encoding]::Unicode.GetBytes($baseGguf))
        $adapterNeedle = $latin1.GetString([System.Text.Encoding]::Unicode.GetBytes($adapterGguf))

        if ($modDllBytes.IndexOf($baseNeedle, [System.StringComparison]::Ordinal) -lt 0) {
            $fail += "ChattyValley.Mod.dll does not reference base GGUF basename '$baseGguf'; the DLL and the shipped asset have drifted, so File.Exists will be false for every player"
        }
        if ($modDllBytes.IndexOf($adapterNeedle, [System.StringComparison]::Ordinal) -lt 0) {
            $fail += "ChattyValley.Mod.dll does not reference adapter GGUF basename '$adapterGguf'; the DLL and the shipped asset have drifted, so File.Exists will be false for every player"
        }
    }
    # else: already reported by the $required check above; no need to fail twice.

    # Minimum payload sizes. Presence-by-name alone lets a zero-byte or truncated GGUF, a stub
    # DLL, or a sidecar exe missing its bundled runtime all pass silently, and every defect on
    # this plan so far has produced an artifact of plausible shape. Thresholds are set well
    # below the real payload sizes (base GGUF 697.0 MB, adapter 21.2 MB, sidecar exe 162,816
    # bytes) so a legitimate model swap does not trip them.
    $minBytes = @{
        "ChattyValley/assets/$baseGguf"                        = 600MB
        "ChattyValley/assets/$adapterGguf"                     = 15MB
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

        # A hotfix that bumps manifest.json's Version without the zip filename following (or
        # vice versa) previously passed every check: the gate never read manifest.Version at all.
        # The Nexus filename is what a player downloads and what SMAPI's own toolbox reports back;
        # they must agree.
        if ($zipVersion -and $man.Version -ne $zipVersion) {
            $fail += "manifest version '$($man.Version)' does not match the zip filename version '$zipVersion' (from '$zipLeaf')"
        }
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
