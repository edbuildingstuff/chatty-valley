# Downloads the base GGUF for the Chatty Valley inference harness (a plumbing / latency probe).
# Default: LFM2.5-350M Q4_K_M (LiquidAI), the on-brand base family Ertas fine-tunes (same lineage as
# Canvas Copilot). 350M is the first-choice base for size and latency; the fine-tune carries the voice.
# Larger alternative (better stock voice, ~3x slower): LFM2.5-1.2B-Instruct-GGUF / LFM2.5-1.2B-Instruct-Q4_K_M.gguf
param(
  [string]$Url = "https://huggingface.co/LiquidAI/LFM2.5-350M-GGUF/resolve/main/LFM2.5-350M-Q4_K_M.gguf",
  [string]$Out = (Join-Path $PSScriptRoot "..\models\LFM2.5-350M-Q4_K_M.gguf")
)

$dir = Split-Path $Out -Parent
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }

Write-Host "Downloading $Url"
curl.exe -L --fail --progress-bar -o $Out $Url
if ($LASTEXITCODE -ne 0) { Write-Error "Download failed (curl exit $LASTEXITCODE)"; exit 1 }
Write-Host "Saved to $Out"
