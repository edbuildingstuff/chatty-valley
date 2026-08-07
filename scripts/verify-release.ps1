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

    # The base GGUF basename, defined once here so the required-file list, the size thresholds,
    # and the DLL cross-check below all agree with each other and cannot silently drift apart the
    # way the DLL, package-release.ps1, and this script's required list once could. Adapter
    # basenames now live in characters/*.json and are checked by the character-file loop below.
    $baseGguf    = 'LFM2.5-1.2B-Instruct-Q4_K_M.gguf'

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
        # Apache 2.0 section 4 requires the licence and the NOTICE attributions to travel with any
        # distribution of the work, so a release missing either is out of compliance with our own
        # terms. LICENSE-LFM.txt is separate and covers the bundled model weights.
        'ChattyValley/LICENSE'
        'ChattyValley/NOTICE'
        'ChattyValley/LICENSE-LFM.txt'
        'ChattyValley/characters/linus.json'
        "ChattyValley/assets/$baseGguf"
        'ChattyValley/sidecar/ChattyValley.Sidecar.exe'
        # These two are the actual sidecar entry assembly and the runtime glue that loads the
        # native llama.cpp backend. Neither was previously asserted: a LLamaSharp bump or RID
        # change could drop either one, pass every other check, and die on model load at runtime.
        'ChattyValley/sidecar/ChattyValley.Sidecar.dll'
        'ChattyValley/sidecar/ChattyValley.Runtime.dll'
        # The Windows GPU backend (DAT-704). Absence means 0.4.0 shipped CPU-only by accident.
        'ChattyValley/sidecar/runtimes/win-x64/native/vulkan/ggml-vulkan.dll'
        'ChattyValley/sidecar/runtimes/win-x64/native/vulkan/llama.dll'
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

    # createdump.exe must never ship. Microsoft's self-contained publish adds it, its
    # process-memory-dump behaviour is heuristic-scanner bait (prime suspect in the 2026-07-31
    # Nexus auto-quarantine), and it does nothing on a player machine. package-release.ps1 strips
    # it; this catches the strip being lost in a refactor. Case-insensitive match on purpose:
    # NTFS would serve "CreateDump.exe" to the scanner just the same.
    foreach ($n in $names) { if ($n -imatch '/createdump\.exe$') { $fail += "createdump.exe shipped: $n" } }

    # Linux natives must never ship: the Vulkan package emits them even on a win-x64 publish and
    # publish-sidecar.ps1 strips them; this catches the strip being lost in a refactor.
    foreach ($n in $names) {
        if ($n -imatch '\.so$' -or $n -cmatch '/runtimes/linux-') { $fail += "linux native shipped: $n" }
    }

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

    # The shipping base GGUF filename lives in six places (ModEntry.cs, package-release.ps1, this
    # script, and three docs/tests) with nothing that ties them together: a model retrain that
    # renames the GGUF, with the two obvious "release" scripts updated but ModEntry.cs missed
    # (or vice versa), builds a zip, passes every check above, and then File.Exists is false for
    # every player and free-chat silently disables itself forever. Close that gap here by reading
    # the actual compiled ChattyValley.Mod.dll and asserting the basename it resolves is the
    # one the zip ships. Adapter basenames no longer live in this fixed list at all: they now
    # live in characters/*.json and are checked against the shipped zip by the character-file
    # loop above.
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

        if ($modDllBytes.IndexOf($baseNeedle, [System.StringComparison]::Ordinal) -lt 0) {
            $fail += "ChattyValley.Mod.dll does not reference base GGUF basename '$baseGguf'; the DLL and the shipped asset have drifted, so File.Exists will be false for every player"
        }
    }
    # else: already reported by the $required check above; no need to fail twice.

    # Minimum payload sizes. Presence-by-name alone lets a zero-byte or truncated GGUF, a stub
    # DLL, or a sidecar exe missing its bundled runtime all pass silently, and every defect on
    # this plan so far has produced an artifact of plausible shape. Thresholds are set well
    # below the real payload sizes (base GGUF 697.0 MB, sidecar exe 162,816 bytes) so a legitimate
    # model swap does not trip them. Adapter GGUFs are no longer a fixed-size entry here: each one
    # is checked against a 1MB floor in the per-character loop above, since the roster of adapters
    # (and each one's basename) is only known once the zip's characters/*.json files are read.
    $minBytes = @{
        "ChattyValley/assets/$baseGguf"                        = 600MB
        'ChattyValley/sidecar/ChattyValley.Sidecar.exe'        = 50KB
        'ChattyValley/ChattyValley.Mod.dll'                    = 8KB
        'ChattyValley/ChattyValley.Core.dll'                   = 4KB
        'ChattyValley/sidecar/runtimes/win-x64/native/vulkan/ggml-vulkan.dll' = 50MB
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
        if ($cfg.Gpu -ne 'auto') { $fail += "config Gpu is '$($cfg.Gpu)', expected 'auto'" }
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

    # Every shipped character file must parse, name a character, and its adapterPath must ship in
    # the zip with a plausible size. Derived from the zip's own characters/ entries, so a new
    # villager is covered with no edit to this script.
    $charEntries = @($names | Where-Object { $_ -cmatch '^ChattyValley/characters/[^/]+\.json$' })
    if ($charEntries.Count -eq 0) { $fail += 'zip ships no characters/*.json' }
    $shippedAdapterPaths = @()
    foreach ($ce in $charEntries) {
        $ceErr = $null
        $char = Read-Json $ce ([ref]$ceErr)
        if ($null -eq $char) { $fail += "cannot parse ${ce}: $ceErr"; continue }
        if (-not $char.name) { $fail += "$ce has no name field"; continue }
        if (-not $char.adapterPath) { $fail += "$ce has no adapterPath (0.3.0 ships adapter-backed villagers only)"; continue }
        $adapterEntry = "ChattyValley/$($char.adapterPath -replace '\\','/')"
        $shippedAdapterPaths += $adapterEntry
        if ($names -cnotcontains $adapterEntry) {
            $fail += "$ce points at '$($char.adapterPath)' but the zip does not ship $adapterEntry"
            continue
        }
        # 1MB floor: catches zero-byte/truncated adapters while leaving room for smaller
        # future adapters (a 350M-base LoRA is a fraction of the 1.2B's 21.2 MB).
        $ae = $zip.GetEntry($adapterEntry)
        if ($ae -and $ae.Length -lt 1MB) {
            $fail += "$adapterEntry is only $([math]::Round($ae.Length/1KB,1)) KB, expected at least 1024 KB"
        }
    }

    # Matches both the raw path (C:\Users\name) and the JSON-escaped form (C:\\Users\\name). The
    # escaped form is not hypothetical: a portable PDB carries a SourceLink map that is literally
    # JSON, so a pattern anchored on a single backslash walks straight past the biggest leak in the
    # package.
    $devPathPattern = '[A-Za-z]:\\{1,2}Users\\{1,2}'

    # no absolute developer paths anywhere in the shipped text files
    foreach ($t in 'ChattyValley/config.json', 'ChattyValley/manifest.json', 'ChattyValley/README.txt') {
        $body = Read-Entry $t
        if ($body -cmatch $devPathPattern) { $fail += "absolute developer path leaked in $t" }
    }

    # Same rule, applied to our compiled output, which is where it actually leaked. A default Release
    # build writes the build machine's absolute paths into both the .pdb (source documents) and the
    # assembly itself (the RSDS debug-directory entry naming that .pdb), so shipping them published a
    # username and a folder layout to every player. Directory.Build.props sets Deterministic + PathMap
    # to normalise those; this asserts it, because a settings file is easy to lose in a merge and the
    # leak is invisible unless something looks. Only our own binaries are checked: the Microsoft
    # runtime and the llama.cpp natives are third-party, unmodified, and not ours to rebuild.
    $ourBinaries = $zip.Entries | Where-Object {
        $_.Name -like 'ChattyValley.*' -and ($_.Name -match '\.(dll|exe|pdb)$')
    }
    foreach ($e in $ourBinaries) {
        $stream = $e.Open()
        try {
            $ms = New-Object System.IO.MemoryStream
            $stream.CopyTo($ms)
            # Latin1 maps every byte to one char, so a binary can be regex-scanned without decoding
            # it as UTF-8 and losing or mangling bytes along the way.
            $text = [System.Text.Encoding]::GetEncoding('iso-8859-1').GetString($ms.ToArray())
            $ms.Dispose()
        }
        finally { $stream.Dispose() }
        if ($text -cmatch $devPathPattern) {
            $fail += "absolute developer path leaked in $($e.FullName)"
        }
    }

    if ($fail.Count) {
        $fail | ForEach-Object { Write-Host "FAIL  $_" }
        throw "$($fail.Count) release check(s) failed"
    }
    Write-Host "release OK: $ZipPath"
}
finally { $zip.Dispose() }
