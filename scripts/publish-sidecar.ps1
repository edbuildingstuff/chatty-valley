#requires -Version 7
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

$exe = Join-Path $OutDir 'ChattyValley.Sidecar.exe'
if (-not (Test-Path $exe)) { throw "sidecar publish produced no exe at $exe" }

$size = [math]::Round((Get-ChildItem $OutDir -Recurse -File | Measure-Object Length -Sum).Sum / 1MB, 1)
Write-Host "sidecar published: $exe ($size MB)"
