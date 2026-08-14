#requires -Version 5.1
<#
  .SYNOPSIS
  Regenerates docs/release-integrity.md from a built release zip.

  .DESCRIPTION
  Every hash in that document is derived from the zip's own bytes, so the document is only true
  for the artifact it was generated from. Regenerate it whenever the zip changes, or it becomes
  worse than useless: a stale integrity manifest reads as evidence of tampering to exactly the
  careful reader it exists to reassure.

  Hashes are computed from the decompressed entry, which is what a player ends up with on disk
  after unzipping, so `Get-FileHash` on their installed file matches the table.

  .EXAMPLE
  powershell.exe -NoProfile -File scripts\generate-release-manifest.ps1 -ZipPath dist\ChattyValley-0.2.0.zip
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $ZipPath,
    [string] $OutFile
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

if (-not $OutFile) {
    $OutFile = Join-Path (Split-Path -Parent $PSScriptRoot) 'docs\release-integrity.md'
}

$resolved = (Resolve-Path $ZipPath).Path
$zipName  = Split-Path $resolved -Leaf
$zipBytes = (Get-Item $resolved).Length
$zipHash  = (Get-FileHash $resolved -Algorithm SHA256).Hash.ToLower()

# Hash every entry from its decompressed bytes.
$zip = [System.IO.Compression.ZipFile]::OpenRead($resolved)
$rows = @()
try {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    foreach ($e in $zip.Entries) {
        if ($e.FullName.EndsWith('/')) { continue }
        $stream = $e.Open()
        try { $hash = ($sha.ComputeHash($stream) | ForEach-Object { $_.ToString('x2') }) -join '' }
        finally { $stream.Dispose() }
        # Strip the leading "ChattyValley/" so paths read as they do inside the Mods folder.
        $rel = $e.FullName -replace '^ChattyValley/', ''
        $rows += [pscustomobject]@{ Path = $rel; Name = (Split-Path $rel -Leaf); Size = $e.Length; Sha = $hash }
    }
}
finally { $zip.Dispose() }

# Decimal MB/KB, deliberately. PowerShell's 1MB constant is 1,048,576, so dividing by it and
# labelling the result "MB" reports mebibytes under a megabyte label: that is what made the archive
# read as 763.72 MB against the 800 MB GitHub shows for the very same file. GitHub, the README and
# this document now all quote the same number, and the byte count on the archive line is exact.
function Format-Size([long] $b) {
    if ($b -ge 1000000) { return ('{0:N2} MB' -f ($b / 1000000)) }
    if ($b -ge 1000)    { return ('{0:N0} KB' -f ($b / 1000)) }
    return ("$b B")
}

# Group by what a reader actually cares about, not by folder.
# Classify native libraries by PATH, not by a hardcoded name list. The first version listed the four
# names known at the time and silently filed everything else under the Microsoft fold, which put
# llama.cpp's four mtmd.dll builds under a heading claiming they were Microsoft-signed. Anything under
# runtimes/<rid>/native/ came from a native NuGet package by definition, so the path is the honest
# test and it survives upstream adding libraries.
$ours      = $rows | Where-Object { $_.Name -like 'ChattyValley.*' -and $_.Name -match '\.(exe|dll)$' }
$native    = $rows | Where-Object { $_.Path -match '(^|/)runtimes/[^/]+/native/' -and $_.Name -match '\.(exe|dll)$' }
$models    = $rows | Where-Object { $_.Name -like '*.gguf' }
$nativePaths = $native | ForEach-Object { $_.Path }
$runtimeMs = $rows | Where-Object {
    $_.Name -match '\.(exe|dll)$' -and $_.Name -notlike 'ChattyValley.*' -and $nativePaths -notcontains $_.Path
}

function Emit-Table($items, [switch] $NoScan) {
    $out = @('| File | Size | SHA256 |', '|---|---|---|')
    foreach ($i in $items | Sort-Object Path) {
        $cell = if ($NoScan) { '`' + $i.Sha + '`' }
                else { '[`' + $i.Sha.Substring(0,16) + '...`](https://www.virustotal.com/gui/file/' + $i.Sha + ')' }
        $out += ('| `{0}` | {1} | {2} |' -f $i.Path, (Format-Size $i.Size), $cell)
    }
    return $out
}

$uniqueNative = ($native | Group-Object Sha).Count
$msBytes = ($runtimeMs | Measure-Object -Property Size -Sum).Sum

$md = @()
$md += '# Release integrity'
$md += ''
$md += ('Generated from `{0}` by `scripts/generate-release-manifest.ps1`. Every hash below comes from that' -f $zipName)
$md += 'artifact''s own bytes, so this document is only true for that build. It is regenerated with each release.'
$md += ''
$md += ('- **Archive:** `{0}`, {1} ({2:N0} bytes)' -f $zipName, (Format-Size $zipBytes), $zipBytes)
$md += ('- **SHA256:** `{0}`' -f $zipHash)
$md += ''
$md += 'Check any file yourself after unzipping. Hashes are of the extracted file, so this matches what you have on disk:'
$md += ''
$md += '```powershell'
$md += 'Get-FileHash "Stardew Valley\Mods\ChattyValley\sidecar\ChattyValley.Sidecar.exe" -Algorithm SHA256'
$md += '```'
$md += ''
$md += 'Each SHA256 below links to its VirusTotal report. VirusTotal keys reports by hash, so a link resolves'
$md += 'whether or not anyone has scanned that exact file yet; an unscanned file shows as not found until'
$md += 'someone uploads it, and the link starts working from that moment.'
$md += ''
$md += '## The launcher'
$md += ''
$md += 'The one file people ask about. It is a thin native shim that starts the .NET runtime; the logic lives in'
$md += 'the managed assemblies below it.'
$md += ''
$md += (Emit-Table ($ours | Where-Object { $_.Name -like '*.exe' }))
$md += ''
$md += '## llama.cpp inference, native'
$md += ''
$md += ('{0} files, {1} unique binaries: four builds per library for CPU, one per x86 instruction set (avx,' -f $native.Count, $uniqueNative)
$md += 'avx2, avx512, noavx), plus one GPU build set using Vulkan. Your machine loads exactly one set at'
$md += 'runtime, whichever the sidecar selects for your hardware.'
$md += ''
$md += 'The CPU sets come from the `LLamaSharp.Backend.Cpu` NuGet package. The Vulkan set comes from'
$md += '`LLamaSharp.Backend.Vulkan`, a small package of MSBuild props that pulls in the actual win-x64'
$md += 'binaries from its `LLamaSharp.Backend.Vulkan.Windows` dependency. Neither is built by this project.'
$md += ''
$md += '**Every one of them is byte-identical to its published package.** You can check this without trusting'
$md += 'us. For the CPU sets: download `llamasharp.backend.cpu.0.27.0.nupkg` from nuget.org, open it as a zip,'
$md += 'and hash the files under `LLamaSharpRuntimes/win-x64/native/`. For the Vulkan set: download'
$md += '`llamasharp.backend.vulkan.windows.0.27.0.nupkg` (not the `llamasharp.backend.vulkan` package itself,'
$md += 'which carries the props but not the binaries), open it as a zip, and hash the files under'
$md += '`LLamaSharpRuntimes/win-x64/native/vulkan/`. Both match the table below exactly.'
$md += ''
$md += 'That matters because **antivirus engines do sometimes flag these libraries.** They are large, heavily'
$md += 'optimised native code, hand-vectorised for the CPU sets and GPU-compute for the Vulkan set, that'
$md += 'allocates a lot of memory and JITs compute kernels, which is a shape that trips machine-learning'
$md += 'heuristics. When it happens it is usually a single engine out of roughly seventy, with a generic'
$md += 'verdict name rather than a named malware family. Because the binaries are unmodified upstream builds,'
$md += 'any such verdict is a statement about the standard llama.cpp Windows release that thousands of'
$md += 'projects ship, not about anything compiled here. Check the links, and weigh one detection out of'
$md += 'seventy accordingly.'
$md += ''
$md += (Emit-Table $native)
$md += ''
$md += '## This project''s own code, managed'
$md += ''
$md += 'Small because they are just the mod logic. No antivirus engine holds signatures for a single developer''s'
$md += 'assemblies, so a clean result here proves less than reading the source, which is the point of this repo.'
$md += ''
$md += (Emit-Table ($ours | Where-Object { $_.Name -like '*.dll' }))
$md += ''
$md += '## Model weights'
$md += ''
$md += 'Data, not code. Nothing executes these; `llama.cpp` reads them as tensors. Listed for completeness and so'
$md += 'you can confirm the download is intact. The base model exceeds VirusTotal''s upload limit, so these carry'
$md += 'hashes rather than scan links.'
$md += ''
$md += (Emit-Table $models -NoScan)
$md += ''
$md += '## Microsoft .NET runtime'
$md += ''
$md += ('{0} files, {1}. The sidecar ships self-contained so players need no .NET install. These are Microsoft''s,' -f $runtimeMs.Count, (Format-Size $msBytes))
$md += 'unmodified and Microsoft-signed; scanning them tells you about Microsoft rather than about this mod. Listed'
$md += 'in full anyway, because "every executable file" should mean every one.'
$md += ''
$md += '<details><summary>Show all ' + $runtimeMs.Count + ' runtime files</summary>'
$md += ''
$md += (Emit-Table $runtimeMs)
$md += ''
$md += '</details>'
$md += ''
# Derive the leftovers rather than describing them from memory. A hand-written sentence here drifts
# the moment packaging changes, and an integrity document that quietly omits a few files is worse
# than one that never claimed completeness: the omission is what a careful reader will find.
$listed = @($ours) + @($native) + @($models) + @($runtimeMs)
$listedPaths = $listed | ForEach-Object { $_.Path }
$rest = $rows | Where-Object { $listedPaths -notcontains $_.Path } | Sort-Object Path

$md += '## Nothing else ships'
$md += ''
$md += ('The archive holds {0} files. The tables above cover {1} of them: every executable, every native library,' -f $rows.Count, $listed.Count)
$md += ('and both model files. The remaining {0} carry no code and are listed here so the accounting is complete.' -f $rest.Count)
$md += ''
$md += (Emit-Table $rest -NoScan)

# Set-Content -Encoding utf8 writes a BOM on PowerShell 5.1, which leads the file with a stray
# U+FEFF and can stop the first heading rendering. WriteAllText with an explicit no-BOM encoding
# is the only reliable way to get clean UTF-8 out of 5.1. Keep the body ASCII for the same reason:
# this script is read as ANSI by 5.1, so a non-ASCII literal in the source arrives mojibaked.
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($OutFile, (($md -join "`n") + "`n"), $utf8NoBom)
Write-Host ("wrote {0}" -f $OutFile)
Write-Host ("  {0} entries: {1} ours, {2} native ({3} unique), {4} Microsoft, {5} models" -f `
    $rows.Count, $ours.Count, $native.Count, $uniqueNative, $runtimeMs.Count, $models.Count)
