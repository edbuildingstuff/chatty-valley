# scripts/probe-multi-adapter.ps1
#requires -Version 5.1
<#
  .SYNOPSIS
  End-to-end probe of multi-character routing: one sidecar, one base, the Linus LoRA registered
  under two names, requests alternating between them, plus the unknown_character error path.

  .DESCRIPTION
  Publishes the sidecar, starts it with --adapter Linus=<lora> --adapter Linus2=<lora>, reads the
  handshake, and sends three requests (Linus, Linus2, Nobody). Passes when both named characters
  reply in voice and the third returns {"error":"unknown_character"}. Run on a machine with the
  shipping GGUFs (the play machine's installed mod assets work: pass -ModelsDir there).
#>
[CmdletBinding()]
param(
    [string] $ModelsDir,
    [string] $BaseGguf = 'LFM2.5-1.2B-Instruct-Q4_K_M.gguf',
    [string] $AdapterGguf = 'linus-12b-v8dpo2-lora-f16.gguf'
)
$ErrorActionPreference = 'Stop'
$repo = Resolve-Path (Join-Path $PSScriptRoot '..')
if (-not $ModelsDir) { $ModelsDir = Join-Path $repo 'models' }
$basePath = Join-Path $ModelsDir $BaseGguf
$loraPath = Join-Path $ModelsDir $AdapterGguf
foreach ($p in $basePath, $loraPath) {
    if (-not (Test-Path $p)) { throw "model not found: $p (pass -ModelsDir at the shipping GGUFs)" }
}

# Publish the sidecar fresh so the probe runs the current code.
$sidecarDir = Join-Path $repo 'dist/_probe-sidecar'
& (Join-Path $PSScriptRoot 'publish-sidecar.ps1') -OutDir $sidecarDir
$sidecarExe = Join-Path $sidecarDir 'ChattyValley.Sidecar.exe'

# The adapter-mode system prompt, matching PromptBuilder.BuildSystem exactly (training format
# equals inference format), with the LFM2 ChatML template and the ResponseStart prefill.
function New-Prompt([string] $name) {
    $system = "You are $name, a resident of Pelican Town in Stardew Valley. " +
              "Current situation: spring, clear morning, the mountains, 2 hearts"
    return "<|im_start|>system`n$system<|im_end|>`n" +
           "<|im_start|>user`nHello! How are you today?<|im_end|>`n" +
           "<|im_start|>assistant`n"
}

$pipeName = 'ChattyValley.Probe.' + [guid]::NewGuid().ToString('N')
# Windows PowerShell 5.1's Start-Process -ArgumentList joins array elements with a bare space and
# does NOT quote elements that themselves contain spaces (confirmed: an unquoted "C:\Program Files
# (x86)\..." path splits into multiple argv entries at the sidecar, which then fails to find
# "C:\Program"). The shipping GGUFs live under Program Files, so this bites for real -- quote every
# element that carries a path.
$proc = Start-Process -FilePath $sidecarExe -PassThru -NoNewWindow -ArgumentList @(
    '--base', "`"$basePath`"", '--pipe', $pipeName, '--gpu-layers', '0',
    '--adapter', "`"Linus=$loraPath`"", '--adapter', "`"Linus2=$loraPath`"")
try {
    $pipe = New-Object System.IO.Pipes.NamedPipeClientStream('.', $pipeName,
        [System.IO.Pipes.PipeDirection]::InOut)
    # The sidecar creates the pipe only after the base loads (8-10s on the play machine).
    $pipe.Connect(120000)
    $reader = New-Object System.IO.StreamReader($pipe, [System.Text.Encoding]::UTF8)
    $writer = New-Object System.IO.StreamWriter($pipe, (New-Object System.Text.UTF8Encoding($false)))
    $writer.AutoFlush = $true

    $failures = @()

    # 1. handshake: both adapters validated ok
    $hs = $reader.ReadLine() | ConvertFrom-Json
    Write-Host "handshake: ready=$($hs.ready) base=$($hs.base) adapters=$(($hs.adapters | ForEach-Object { "$($_.name):$($_.status)" }) -join ', ')"
    if (-not $hs.ready) { $failures += 'handshake not ready' }
    foreach ($n in 'Linus', 'Linus2') {
        if (-not ($hs.adapters | Where-Object { $_.name -ceq $n -and $_.status -ceq 'ok' })) {
            $failures += "handshake does not report $n as ok"
        }
    }

    # 2. both characters route and reply (same LoRA under two names, so both reply in voice;
    #    the point is that BOTH names route and infer, i.e. per-request adapter selection works)
    foreach ($n in 'Linus', 'Linus2') {
        $req = @{ prompt = (New-Prompt 'Linus'); character = $n; temp = 0.35; maxTokens = 64;
                  repeatPenalty = 1.1; frequencyPenalty = 0.1 } | ConvertTo-Json -Compress
        $writer.WriteLine($req)
        $resp = $reader.ReadLine() | ConvertFrom-Json
        if ($resp.PSObject.Properties['error']) { $failures += "request as '$n' errored: $($resp.error)" }
        elseif (-not $resp.reply) { $failures += "request as '$n' returned an empty reply" }
        else { Write-Host "[$n] $($resp.reply)" }
    }

    # 3. an unregistered character is a typed error, and the sidecar survives it
    $req = @{ prompt = (New-Prompt 'Nobody'); character = 'Nobody'; temp = 0.35; maxTokens = 16 } |
        ConvertTo-Json -Compress
    $writer.WriteLine($req)
    $resp = $reader.ReadLine() | ConvertFrom-Json
    if ($resp.error -cne 'unknown_character') {
        $failures += "expected unknown_character for 'Nobody', got: $($resp | ConvertTo-Json -Compress)"
    } else { Write-Host "[Nobody] unknown_character, as expected" }

    if ($failures.Count) {
        $failures | ForEach-Object { Write-Host "FAIL  $_" }
        throw "$($failures.Count) probe check(s) failed"
    }
    Write-Host 'probe OK: two-name routing and unknown-character handling verified'
}
finally {
    if ($proc -and -not $proc.HasExited) { $proc.Kill() }
}
