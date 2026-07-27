#requires -Version 5.1
<#
  .SYNOPSIS
  Builds the distributable Chatty Valley release zip.

  .DESCRIPTION
  Produces dist/ChattyValley-<version>.zip in standard SMAPI layout. Requires Stardew Valley to be
  installed, because ModBuildConfig resolves the game assemblies at compile time even with deploy
  disabled. EnableModDeploy is forced off so packaging never touches the local game folder.
#>
[CmdletBinding()]
param(
    [string] $Version    = '0.2.0',
    [string] $ModelsDir,
    [string] $OutDir
)
$ErrorActionPreference = 'Stop'

# $PSScriptRoot is empty when referenced directly in a [CmdletBinding()] param block's default
# value expressions under Windows PowerShell 5.1, so those defaults are resolved here instead.
if (-not $ModelsDir) { $ModelsDir = Join-Path $PSScriptRoot '../models' }
if (-not $OutDir)    { $OutDir    = Join-Path $PSScriptRoot '../dist' }

$repo    = Resolve-Path (Join-Path $PSScriptRoot '..')
$stage   = Join-Path $OutDir 'ChattyValley'
$baseGguf    = 'LFM2.5-1.2B-Instruct-Q4_K_M.gguf'
$adapterGguf = 'linus-12b-v8dpo2-lora-f16.gguf'

# 1. clean stage
if (Test-Path $stage) { Remove-Item -Recurse -Force $stage }
New-Item -ItemType Directory -Force -Path $stage, "$stage/assets", "$stage/characters" | Out-Null

# 2. build the mod without deploying into the local game folder
$modProj = Join-Path $repo 'src/ChattyValley.Mod/ChattyValley.Mod.csproj'
$modOut  = Join-Path $OutDir '_mod-build'
dotnet build $modProj -c Release -p:EnableModDeploy=false -p:EnableModZip=false -o $modOut

foreach ($f in 'ChattyValley.Mod.dll', 'ChattyValley.Core.dll') {
    Copy-Item (Join-Path $modOut $f) $stage
}
Copy-Item (Join-Path $repo 'src/ChattyValley.Mod/manifest.json') $stage
Copy-Item (Join-Path $repo 'characters/*.json') "$stage/characters"

# 3. the release config, renamed to what SMAPI reads
Copy-Item (Join-Path $repo 'packaging/config.release.json') (Join-Path $stage 'config.json')

# 4. docs and license
Copy-Item (Join-Path $repo 'packaging/README.txt') $stage
Copy-Item (Join-Path $repo 'packaging/LICENSE-LFM.txt') $stage

# 5. models
foreach ($g in $baseGguf, $adapterGguf) {
    $src = Join-Path $ModelsDir $g
    if (-not (Test-Path $src)) { throw "missing model: $src" }
    Copy-Item $src "$stage/assets"
}

# 6. self-contained sidecar
& (Join-Path $PSScriptRoot 'publish-sidecar.ps1') -OutDir (Join-Path $stage 'sidecar')

# 7. zip
# Compress-Archive under PowerShell 5.1 is banned for this artifact: it cannot reliably handle an
# ~800 MB stage with a single 698 MB entry and was observed to die mid-write, leaving a truncated
# zip on disk with no error and no process still holding the file.
#
# ZipFile.CreateFromDirectory is also banned here, even though it does not truncate: Windows
# PowerShell 5.1 runs on .NET Framework, and CreateFromDirectory writes entry names with backslash
# separators on Windows. The ZIP spec (APPNOTE 4.4.17.1) requires forward slashes. Windows Explorer
# and 7-Zip tolerate the violation, which is exactly why this passed local smoke-checks, but macOS
# Archive Utility, Linux unzip, and some mod managers do not: they create one file literally named
# "ChattyValley\manifest.json" instead of a folder tree. Do not "simplify" this back to
# CreateFromDirectory; write every entry by hand and normalise the separator instead.
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = Join-Path $OutDir "ChattyValley-$Version.zip"
if (Test-Path $zip) { Remove-Item -Force $zip }

$archive = [System.IO.Compression.ZipFile]::Open($zip, 'Create')
try {
    # $stage itself may contain an unresolved ".." (e.g. when $OutDir defaults from $PSScriptRoot
    # via '..\dist'), but Get-ChildItem's FileInfo.FullName is always fully resolved by .NET, which
    # collapses any "..". Doing Substring() math against an UNRESOLVED $stage (or its unresolved
    # parent) silently chops the wrong number of characters off every entry name instead of erroring,
    # which is exactly how this broke once already: resolve $stage first so the lengths agree.
    $stageResolved = (Resolve-Path $stage).Path.TrimEnd('\')
    $stageLeaf     = Split-Path $stageResolved -Leaf
    foreach ($f in Get-ChildItem $stage -Recurse -File) {
        $relFromStage = $f.FullName.Substring($stageResolved.Length + 1) -replace '\\', '/'
        $rel = "$stageLeaf/$relFromStage"
        [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $archive, $f.FullName, $rel, [System.IO.Compression.CompressionLevel]::Optimal)
    }
} finally { $archive.Dispose() }

# A truncated zip, and a zip with backslash-separated entries, both look like success unless
# checked. Prove both are absent before reporting success.
$probe = [System.IO.Compression.ZipFile]::OpenRead($zip)
try {
    $entryCount  = $probe.Entries.Count
    $backslashed = @($probe.Entries.FullName | Where-Object { $_ -match '\\' }).Count
} finally { $probe.Dispose() }
if ($entryCount -lt 1) { throw "packaged zip opened but contains no entries" }
if ($backslashed -gt 0) { throw "$backslashed zip entries use backslash separators; the ZIP spec requires forward slashes" }
Write-Host "zip verified readable: $entryCount entries, all forward-slash separated"

Remove-Item -Recurse -Force $modOut
$mb = [math]::Round((Get-Item $zip).Length / 1MB, 1)
Write-Host "packaged: $zip ($mb MB)"
