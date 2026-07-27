#requires -Version 5.1
<#
  .SYNOPSIS
  Publishes ChattyValley.Sidecar as a self-contained win-x64 build.

  .DESCRIPTION
  Players do not have the .NET 10 runtime, so the sidecar ships with its own. Single-file publish is
  deliberately OFF: llama.cpp native libraries do not survive it reliably, and a plain folder is also
  easier for a suspicious player to inspect, which matters on a page that ships an executable.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $OutDir
)
$ErrorActionPreference = 'Stop'

$proj = Join-Path $PSScriptRoot '../src/ChattyValley.Sidecar/ChattyValley.Sidecar.csproj'

dotnet publish $proj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    -o $OutDir

# $ErrorActionPreference does NOT govern a native command's exit code, so check it
# explicitly. Without this, a failed publish into a directory holding a stale exe
# would pass the Test-Path check below and report success into the packaging pipeline.
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

$exe = Join-Path $OutDir 'ChattyValley.Sidecar.exe'
if (-not (Test-Path $exe)) { throw "sidecar publish produced no exe at $exe" }

$size = [math]::Round((Get-ChildItem $OutDir -Recurse -File | Measure-Object Length -Sum).Sum / 1MB, 1)
Write-Host "sidecar published: $exe ($size MB)"
