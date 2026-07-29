# Downloads the base GGUF for the Chatty Valley inference harness (a plumbing / latency probe).
#
# Default: LFM2.5-1.2B-Instruct Q4_K_M (LiquidAI), which is the SHIPPING base. An earlier version of
# this script defaulted to the 350M, which was the original pick for size and latency. That call was
# overturned by an in-game A/B: a fine-tuned 350M holds voice and format but not open-ended
# conversational coherence, while the 1.2B clears that bar. Do not restore the 350M default; the
# adapters that ship are trained against this base and a mismatched pair produces nonsense.
#
# Smaller arm, kept for A/B and constrained surfaces (greeting lines, scripted beats):
#   -Url https://huggingface.co/LiquidAI/LFM2.5-350M-GGUF/resolve/main/LFM2.5-350M-Q4_K_M.gguf
#   -Out ../models/LFM2.5-350M-Q4_K_M.gguf
param(
  [string]$Url = "https://huggingface.co/LiquidAI/LFM2.5-1.2B-Instruct-GGUF/resolve/main/LFM2.5-1.2B-Instruct-Q4_K_M.gguf",
  [string]$Out = (Join-Path $PSScriptRoot "..\models\LFM2.5-1.2B-Instruct-Q4_K_M.gguf")
)

$dir = Split-Path $Out -Parent
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }

Write-Host "Downloading $Url"
curl.exe -L --fail --progress-bar -o $Out $Url
if ($LASTEXITCODE -ne 0) { Write-Error "Download failed (curl exit $LASTEXITCODE)"; exit 1 }
Write-Host "Saved to $Out"
