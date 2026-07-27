#requires -Version 5.1
<#
  .SYNOPSIS
  Runs the full Chatty Valley release chain: package, verify, checksum, draft GitHub release.

  .DESCRIPTION
  Today a human runs package-release.ps1 then verify-release.ps1 by hand and, in practice,
  sometimes forgets the verify step. This chains all three release scripts in order and adds the
  step that gets forgotten alongside them: printing a SHA256 the human can compare against what a
  player downloads.

  Does not push to the remote (pushing is the caller's decision, not this script's) and does not
  publish the release (it always creates a draft; publishing is a human decision made on GitHub).
  See -PreNexus below for the one verify failure this script is allowed to tolerate.
#>
[CmdletBinding()]
param(
    [string] $Version,
    [string] $Tag,
    [switch] $PreNexus,
    [switch] $SkipPackage
)

# Pulls every "FAIL  <message>" line out of verify-release.ps1's captured stdout (that exact
# two-space prefix is what verify-release.ps1 writes). A small pure function on purpose, so
# -PreNexus's exact-one-failure logic below can be unit tested directly against a captured
# transcript, or against a synthetic multi-failure transcript, without rebuilding the 745 MB zip
# for every test. Dot-source this file to get access to it without running a release.
#
# Returns via ",$result" deliberately, not a bare "$result": PowerShell unrolls an array onto the
# pipeline, and a zero-length array unrolled onto the pipeline hands the caller $null instead of
# an empty array (confirmed empirically). A clean verify pass produces zero FAIL lines, which is
# exactly the input this hits on every successful run, so an unguarded return here would make
# Test-PreNexusTolerable's Mandatory $FailLines parameter throw a binding error on the common case
# instead of the rare one. The comma forces the array through as one object, zero-length or not.
function Get-VerifyFailLines {
    param([Parameter(Mandatory)] [AllowEmptyCollection()] [string[]] $Lines)
    $result = @($Lines | Where-Object { $_ -cmatch '^FAIL  ' } | ForEach-Object { $_ -replace '^FAIL  ', '' })
    return ,$result
}

# -PreNexus tolerates exactly one known failure: "manifest has no UpdateKeys". That failure is
# correct and expected today because the Nexus mod ID does not exist until the mod page is
# created, and a placeholder UpdateKeys value would be a valid-looking key the gate would wrongly
# accept. This is deliberately NOT "count the failures and hope": a run that has the UpdateKeys
# failure ALONGSIDE any other failure must still stop, because that other failure is real and
# -PreNexus is not a general ignore-verify-errors switch. Checking the count alone would let a
# second, unrelated failure slip through a run that happens to also be missing UpdateKeys.
function Test-PreNexusTolerable {
    param([Parameter(Mandatory)] [AllowEmptyCollection()] [string[]] $FailLines)
    if ($FailLines.Count -ne 1) { return $false }
    return $FailLines[0] -cmatch 'manifest has no UpdateKeys'
}

# gh (like any native command) writes its own diagnostic text to stderr on failure, e.g.
# "No commit found for SHA..." or "release not found". Confirmed empirically: under
# $ErrorActionPreference = 'Stop', Windows PowerShell 5.1 treats that stderr WRITE ITSELF as a
# terminating error the instant it happens, regardless of where the stream is redirected
# (2>$null, 2>&1, and a file redirect all still terminate the script right there, before the
# throw below ever runs). That is a separate problem from the exit-code behavior noted elsewhere
# in this script (an exit code alone does not trigger Stop); this fires on the write, before
# $LASTEXITCODE is even checked, and would surface PowerShell's own noisy NativeCommandError text
# in place of this script's own clear failure messages on exactly the calls that need them most.
# The only fix confirmed to work is toggling $ErrorActionPreference to 'Continue' (PowerShell's
# own default) for the duration of the call. $LASTEXITCODE and any command output both survive the
# toggle intact for the caller to use immediately after.
function Invoke-NativeCommand {
    param([Parameter(Mandatory)] [scriptblock] $Command)
    $previousEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        & $Command
    } finally {
        $ErrorActionPreference = $previousEap
    }
}

# Dot-sourced (test harness use, e.g. ". .\release.ps1" then call the functions above directly):
# stop here so a test run gets the functions without preflight, packaging, or a gh call.
if ($MyInvocation.InvocationName -eq '.') { return }

$ErrorActionPreference = 'Stop'
$repo = Resolve-Path (Join-Path $PSScriptRoot '..')

# Every git/gh call below depends on running with the repo as the working directory (gh has no
# other way to resolve which GitHub repo it is talking to). package-release.ps1 and
# verify-release.ps1 are invoked by absolute path below and do not need this, but pin it anyway so
# this script behaves the same whether it is run from the repo root or from anywhere else.
Push-Location $repo
try {

# ===========================================================================================
# 1. Preflight. Everything here must fail fast, before packaging spends minutes building a
#    745 MB zip that a missing prerequisite would have wasted.
# ===========================================================================================

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "gh (GitHub CLI) not found on PATH. Install it from https://cli.github.com/ and retry."
}

# $ErrorActionPreference does NOT govern a native command's exit code, so check it explicitly,
# here and after every other native / gh call below. Every gh/git call in this script is wrapped
# in Invoke-NativeCommand; see that function's comment for why the wrapping is necessary, not just
# the $LASTEXITCODE check.
Invoke-NativeCommand { gh auth status *> $null }
if ($LASTEXITCODE -ne 0) {
    throw "gh is not authenticated. Run: gh auth login"
}

$headSha = (Invoke-NativeCommand { git rev-parse HEAD }).Trim()
if ($LASTEXITCODE -ne 0) { throw "git rev-parse HEAD failed; is $repo a git repository?" }

# gh release create needs the released commit to already exist on GitHub (a release tag has to
# point at a real commit on the remote), so check that here rather than let gh fail on it after
# the zip is already built. Queried directly against the GitHub API rather than local
# remote-tracking refs (git branch -r --contains), because remote-tracking refs go stale without
# a fetch and this needs to be right the first time, not after an extra step.
Invoke-NativeCommand { gh api "repos/{owner}/{repo}/commits/$headSha" --silent 2>$null }
if ($LASTEXITCODE -ne 0) {
    throw "current commit $headSha is not on the GitHub remote yet, so gh release create cannot target it. Push it first with: git push -u origin HEAD"
}

Write-Host "preflight OK: gh present, authenticated, and $headSha is on the remote"

# ===========================================================================================
# 2. Version, tag, and paths. Derived the same way package-release.ps1 derives them
#    (manifest.json is the single source of truth for $Version), so -SkipPackage against an
#    already-built zip resolves to the exact path package-release.ps1 would have produced.
# ===========================================================================================

if (-not $PSBoundParameters.ContainsKey('Version')) {
    $manifestPath = Join-Path $repo 'src/ChattyValley.Mod/manifest.json'
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    $Version = $manifest.Version
    if (-not $Version) { throw "could not read a Version from $manifestPath" }
}
if (-not $Tag) { $Tag = "v$Version" }

$outDir = Join-Path $repo 'dist'
$zip = Join-Path $outDir "ChattyValley-$Version.zip"

# ===========================================================================================
# 3. Package, unless re-running against an already-built zip.
# ===========================================================================================

if ($SkipPackage) {
    Write-Host "SkipPackage set: reusing existing zip, not rebuilding"
    if (-not (Test-Path $zip)) {
        throw "SkipPackage was set but $zip does not exist. Drop -SkipPackage to build it, or pass -Version to point at an already-built zip."
    }
} else {
    Write-Host "packaging $Version ..."
    $packageArgs = @()
    if ($PSBoundParameters.ContainsKey('Version')) { $packageArgs += @('-Version', $Version) }
    powershell.exe -NoProfile -File (Join-Path $PSScriptRoot 'package-release.ps1') @packageArgs
    if ($LASTEXITCODE -ne 0) { throw "package-release.ps1 failed with exit code $LASTEXITCODE" }
    if (-not (Test-Path $zip)) { throw "package-release.ps1 reported success but $zip does not exist" }
}

# ===========================================================================================
# 4. Verify. Run as a separate process so its stdout, stderr, and exit code are all genuinely
#    independent of this script's own state, then captured deliberately as two SEPARATE streams:
#    stdout into a variable, stderr into a temp file. Merging them with 2>&1 is banned here: under
#    Windows PowerShell 5.1, redirecting a native process's stderr into the success stream wraps
#    each stderr line in a NativeCommandError record, and mixing plain strings with ErrorRecord
#    objects in the same array would corrupt the plain-string FAIL-line matching in
#    Get-VerifyFailLines above. Confirmed empirically before relying on it.
# ===========================================================================================

Write-Host "verifying $zip ..."
$verifyStderrPath = Join-Path ([System.IO.Path]::GetTempPath()) "chatty-valley-verify-stderr-$PID.txt"
try {
    # Wrapped in @(...) at the capture site for the same reason Get-VerifyFailLines returns via
    # ",$result": a verify run that emits zero lines of stdout (e.g. it dies before printing
    # anything) would otherwise collapse to $null instead of an empty array, and $null is not a
    # valid [string[]] to hand to Get-VerifyFailLines below.
    $verifyStdout = @(& powershell.exe -NoProfile -File (Join-Path $PSScriptRoot 'verify-release.ps1') -ZipPath $zip 2>$verifyStderrPath)
    $verifyExitCode = $LASTEXITCODE
    $verifyStderr = if (Test-Path $verifyStderrPath) { Get-Content $verifyStderrPath -Raw } else { '' }
} finally {
    if (Test-Path $verifyStderrPath) { Remove-Item -Force $verifyStderrPath }
}

$verifyStdout | ForEach-Object { Write-Host $_ }

if ($verifyExitCode -ne 0) {
    $failLines = Get-VerifyFailLines -Lines $verifyStdout
    if ($PreNexus -and (Test-PreNexusTolerable -FailLines $failLines)) {
        Write-Host ""
        Write-Host "PreNexus WARNING: verify-release.ps1 failed on exactly the expected pre-Nexus check (manifest has no UpdateKeys)."
        Write-Host "That is expected until the Nexus mod page exists and a real UpdateKeys value can be added. Continuing to build a DRAFT release."
        Write-Host ""
    } else {
        if ($verifyStderr) { Write-Host $verifyStderr }
        throw "verify-release.ps1 failed with exit code $verifyExitCode (see FAIL lines above). -PreNexus only tolerates the single missing-UpdateKeys failure, and this run does not qualify."
    }
} elseif ($PreNexus) {
    Write-Host "PreNexus set but verify-release.ps1 passed cleanly: -PreNexus is a no-op this run."
}

Write-Host "verify OK (or tolerated under -PreNexus): $zip"

# ===========================================================================================
# 5. Checksum. The entire point is that a human compares this against a checksum of the file
#    they download from Nexus, so print it on its own line, clearly labelled, easy to copy.
# ===========================================================================================

$hash = (Get-FileHash -Path $zip -Algorithm SHA256).Hash
$sizeBytes = (Get-Item $zip).Length
$sizeMb = [math]::Round($sizeBytes / 1MB, 1)
$zipLeaf = Split-Path $zip -Leaf

Write-Host ""
Write-Host "=================================================================="
Write-Host "SHA256 ($zipLeaf):"
Write-Host "  $hash"
Write-Host "=================================================================="
Write-Host ""

# ===========================================================================================
# 6. Draft GitHub release. Draft only, always. There is no publish path here on purpose:
#    publishing is a human decision made by reviewing the draft on GitHub.
# ===========================================================================================

$releaseNotesPath = Join-Path ([System.IO.Path]::GetTempPath()) "chatty-valley-release-notes-$PID.md"
$releaseTitle = "Chatty Valley $Version"
$body = @"
$releaseTitle

Early access build for Windows. One villager is wired up so far: Linus. Expect rough edges.

Install
1. Install SMAPI (https://smapi.io).
2. Unzip this download into your Stardew Valley Mods folder.
3. Launch the game through SMAPI, not the game's own executable.
4. Talk to Linus and press C to start a free chat.

Verify your download
SHA256: $hash
Size: $sizeMb MB ($sizeBytes bytes)

Compare the SHA256 above against a checksum of the file you actually downloaded before you
install it. If they do not match, download it again rather than run it.
"@
Set-Content -Path $releaseNotesPath -Value $body -NoNewline -Encoding utf8

try {
    # Re-run detection: does a release for this tag already exist. *> $null discards the JSON
    # payload; only $LASTEXITCODE is used (0 means the release exists, nonzero means it does not).
    Invoke-NativeCommand { gh release view $Tag --json tagName *> $null }
    $tagExists = ($LASTEXITCODE -eq 0)

    if ($tagExists) {
        # A release for this tag already exists: this is a re-run, most likely against a rebuilt
        # zip. Never silently leave the user with a release whose asset is from an older build.
        # `gh release upload --clobber` deletes any existing asset of the same name before
        # uploading, in one call, so the asset can never end up stale or duplicated; this is
        # preferred over a separate delete-asset step precisely so there is no gap between delete
        # and upload where a re-run could fail and leave the release with no asset at all.
        Write-Host "release $Tag already exists: replacing its asset and notes with this build"
        Invoke-NativeCommand { gh release upload $Tag $zip --clobber }
        if ($LASTEXITCODE -ne 0) { throw "gh release upload --clobber failed with exit code $LASTEXITCODE" }
        Invoke-NativeCommand { gh release edit $Tag --title $releaseTitle --notes-file $releaseNotesPath }
        if ($LASTEXITCODE -ne 0) { throw "gh release edit failed with exit code $LASTEXITCODE" }
    } else {
        Invoke-NativeCommand { gh release create $Tag $zip --target $headSha --title $releaseTitle --notes-file $releaseNotesPath --draft }
        if ($LASTEXITCODE -ne 0) { throw "gh release create failed with exit code $LASTEXITCODE" }
    }
} finally {
    if (Test-Path $releaseNotesPath) { Remove-Item -Force $releaseNotesPath }
}

# ===========================================================================================
# 7. Report back.
# ===========================================================================================

$releaseUrl = Invoke-NativeCommand { gh release view $Tag --json url --jq '.url' }
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "release created, but could not read back its URL. Check the Releases page on GitHub directly."
} else {
    Write-Host ""
    Write-Host "draft release ready: $releaseUrl"
}
Write-Host ""
Write-Host "still manual: review the draft on GitHub and publish it yourself when ready. Once the"
Write-Host "Nexus mod page exists, add the real UpdateKeys value to manifest.json so future runs no"
Write-Host "longer need -PreNexus."

} finally {
    Pop-Location
}
