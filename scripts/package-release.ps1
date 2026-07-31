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
    [string] $Version,
    [string] $ModelsDir,
    [string] $OutDir
)
$ErrorActionPreference = 'Stop'

# $repo is resolved first (Resolve-Path requires the path to exist, which the repo root always
# does), and $ModelsDir / $OutDir default from $repo rather than from $PSScriptRoot + '..', so an
# unresolved ".." segment never enters $OutDir/$stage in the first place. This closes the path
# class of bug at its source instead of patching it only where it last bit (the zip-entry-naming
# Substring math), so no future path operation on $OutDir/$stage can reintroduce it.
#
# $PSScriptRoot is also empty when referenced directly in a [CmdletBinding()] param block's default
# value expressions under Windows PowerShell 5.1, so none of these defaults can live in param().
$repo = Resolve-Path (Join-Path $PSScriptRoot '..')
if (-not $ModelsDir) { $ModelsDir = Join-Path $repo 'models' }
if (-not $OutDir)    { $OutDir    = Join-Path $repo 'dist' }

# $Version defaults from manifest.json, the single source of truth for the shipped version, rather
# than a literal default. A literal default here is exactly how a manifest version bump (e.g. a
# hotfix to 0.2.1) can ship a zip named and gated against the previous version while the manifest
# inside it disagrees. -Version remains available as an explicit override for a caller that wants
# to force a specific value.
if (-not $PSBoundParameters.ContainsKey('Version')) {
    $manifestPath = Join-Path $repo 'src/ChattyValley.Mod/manifest.json'
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    $Version = $manifest.Version
    if (-not $Version) { throw "could not read a Version from $manifestPath" }
}

$stage       = Join-Path $OutDir 'ChattyValley'
$zip         = Join-Path $OutDir "ChattyValley-$Version.zip"
$baseGguf    = 'LFM2.5-1.2B-Instruct-Q4_K_M.gguf'
$adapterGguf = 'linus-12b-v8dpo2-lora-f16.gguf'

# 1. clean stage and any stale zip from a previous, possibly-failed run. A run that dies between
# staging and the zip write would otherwise leave a same-named zip from an earlier run sitting in
# dist/ with nothing to mark it as stale.
if (Test-Path $stage) { Remove-Item -Recurse -Force $stage }
if (Test-Path $zip)   { Remove-Item -Force $zip }
New-Item -ItemType Directory -Force -Path $stage, "$stage/assets", "$stage/characters" | Out-Null

# 2. build the mod without deploying into the local game folder
$modProj = Join-Path $repo 'src/ChattyValley.Mod/ChattyValley.Mod.csproj'
$modOut  = Join-Path $OutDir '_mod-build'

# Purge stale build output BEFORE building, and check the exit code after (as in
# publish-sidecar.ps1: $ErrorActionPreference does not govern a native command's exit code).
# Without both of these, a regressed mod build leaves last-run DLLs sitting in $modOut, Copy-Item
# below picks them up without error, and the zip reports success while shipping outdated mod code.
if (Test-Path $modOut) { Remove-Item -Recurse -Force $modOut }
dotnet build $modProj -c Release -p:EnableModDeploy=false -p:EnableModZip=false -o $modOut
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }

foreach ($f in 'ChattyValley.Mod.dll', 'ChattyValley.Core.dll') {
    Copy-Item (Join-Path $modOut $f) $stage
}
Copy-Item (Join-Path $repo 'src/ChattyValley.Mod/manifest.json') $stage
Copy-Item (Join-Path $repo 'characters/*.json') "$stage/characters"

# 3. the release config, renamed to what SMAPI reads
Copy-Item (Join-Path $repo 'packaging/config.release.json') (Join-Path $stage 'config.json')

# 4. docs and licences
# LICENSE and NOTICE are the mod's own (Apache 2.0). Section 4 of that licence requires both to
# travel with any distribution of the work, so a release that shipped only LICENSE-LFM.txt would
# be out of compliance with our own terms. LICENSE-LFM.txt covers the bundled model weights, which
# are under different terms entirely (see NOTICE).
Copy-Item (Join-Path $repo 'packaging/README.txt') $stage
Copy-Item (Join-Path $repo 'LICENSE') $stage
Copy-Item (Join-Path $repo 'NOTICE') $stage
Copy-Item (Join-Path $repo 'packaging/LICENSE-LFM.txt') $stage

# 5. models
foreach ($g in $baseGguf, $adapterGguf) {
    $src = Join-Path $ModelsDir $g
    if (-not (Test-Path $src)) { throw "missing model: $src" }
    Copy-Item $src "$stage/assets"
}

# 6. self-contained sidecar
& (Join-Path $PSScriptRoot 'publish-sidecar.ps1') -OutDir (Join-Path $stage 'sidecar')

# 6b. Debug symbols never ship.
#
# Two reasons, and the second is the one that matters. They are worth nothing to a player: no stack
# trace a player can send us is improved by symbols they have but we cannot read without the matching
# source anyway. And a portable PDB embeds a SourceLink map that is literally
# {"documents":{"C:\\Users\\<name>\\...\\chatty-valley\\*": "https://raw.githubusercontent.com/..."}},
# which publishes a username and a folder layout to everyone who downloads the mod.
#
# Directory.Build.props sets PathMap, which cleans the absolute paths out of the assemblies, but it
# deliberately does not touch the SourceLink map: that map's whole job is to point at the real repo
# root so a debugger can fetch matching sources. The right answer for a shipped artifact is to keep
# the symbols locally and leave them out of the zip. verify-release.ps1 fails the build if any of
# our binaries still carries an absolute user path, so this cannot silently regress.
$pdbs = @(Get-ChildItem -Path $stage -Recurse -Filter '*.pdb' -File -ErrorAction SilentlyContinue)
foreach ($p in $pdbs) { Remove-Item $p.FullName -Force }
Write-Host ("stripped {0} debug symbol file(s) from the staged tree" -f $pdbs.Count)

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
# $zip was already computed and any stale copy already cleared at the top of the script (step 1),
# alongside the stage cleanup, so a run that dies before reaching this point cannot leave a
# same-named zip from an earlier run behind with nothing marking it stale.

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

# A truncated zip, a zip with backslash-separated entries, and a zip whose entries lost their
# ChattyValley/ prefix (the exact "y/manifest.json" corruption this script shipped once already,
# which was forward-slashed and had the correct entry count, so checking only those two properties
# passed a broken archive) all look like success unless checked. Prove all three are absent.
$probe = [System.IO.Compression.ZipFile]::OpenRead($zip)
try {
    $entryCount  = $probe.Entries.Count
    # -cmatch / -cnotlike deliberately, matching verify-release.ps1: PowerShell's bare -match /
    # -notlike are case-INSENSITIVE, but zip entry names are case-SENSITIVE on the macOS and Linux
    # filesystems this probe exists to protect (e.g. "Manifest.json" vs "manifest.json").
    $backslashed = @($probe.Entries.FullName | Where-Object { $_ -cmatch '\\' }).Count
    $misprefixed = @($probe.Entries.FullName | Where-Object { $_ -cnotlike "$stageLeaf/*" }).Count
} finally { $probe.Dispose() }
if ($entryCount -lt 1)  { throw "packaged zip opened but contains no entries" }
if ($backslashed -gt 0) { throw "$backslashed zip entries use backslash separators; the ZIP spec requires forward slashes" }
if ($misprefixed -gt 0) { throw "$misprefixed zip entries do not start with '$stageLeaf/'; entry names are being built wrong" }
Write-Host "zip verified readable: $entryCount entries, all under $stageLeaf/ with forward slashes"

Remove-Item -Recurse -Force $modOut
$mb = [math]::Round((Get-Item $zip).Length / 1MB, 1)
Write-Host "packaged: $zip ($mb MB)"

# Deliberately does NOT auto-invoke verify-release.ps1: today, before the mod has a Nexus mod ID,
# verify-release.ps1 is EXPECTED to fail on "manifest has no UpdateKeys". Wiring it in as a hard
# gate here would make ordinary packaging throw on every run until that ID exists, which would
# just get "worked around" rather than fixed. Printing the explicit next step instead keeps
# packaging usable while still making it impossible to mistake this line for a verified release.
Write-Host "NOT YET VERIFIED. Run: powershell -NoProfile -File scripts/verify-release.ps1 -ZipPath `"$zip`""
