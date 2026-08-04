#requires -Version 5.1
<#
  .SYNOPSIS
  Uploads a release zip to Nexus Mods via the official v3 Upload API.

  .DESCRIPTION
  Completes the release chain that release.ps1 leaves manual: gets a built, verified zip onto the
  Nexus mod page without touching the website uploader. Two modes:

    # discover file IDs (also the quickest proof the API key works)
    .\upload-nexus.ps1 -ListFiles

    # upload as a new version of an existing mod file
    .\upload-nexus.ps1 -ZipPath ..\dist\ChattyValley-0.2.1.zip -Version 0.2.1 -FileId <id> `
        -Changelog "Removed createdump.exe from the sidecar." -ArchiveExisting -UpdateModVersion

    # or upload as a brand-new file on the mod page
    .\upload-nexus.ps1 -ZipPath ..\dist\ChattyValley-0.2.1.zip -Version 0.2.1 -NewFile

  Authentication is the personal API key from https://next.nexusmods.com/settings/api-keys, read
  from the NEXUS_APIKEY environment variable (or -ApiKey). The key is a credential: it never goes
  in this file, in the repo, or in a commit. See docs/release-integrity.md for why this repo is
  paranoid about what ships and how.

  The v3 flow this implements (mirrors Nexus-Mods/upload-action, which is the reference client):
    1. POST /v3/uploads/multipart          -> upload id + presigned part URLs + part size
    2. PUT each part to its presigned URL  -> collect ETags
    3. POST completion XML to the presigned complete URL
    4. POST /v3/uploads/{id}/finalise
    5. GET  /v3/uploads/{id} until state == "available"
    6. POST /v3/mod-files/{fileId}/versions   (or POST /v3/mod-files with -NewFile)
    7. POST /v3/mods/{modId}/changelogs       (only when -Changelog is given)
#>
[CmdletBinding(DefaultParameterSetName = 'Upload')]
param(
    [Parameter(ParameterSetName = 'ListFiles', Mandatory)]
    [switch] $ListFiles,

    [Parameter(ParameterSetName = 'Upload', Mandatory)]
    [string] $ZipPath,
    [Parameter(ParameterSetName = 'Upload', Mandatory)]
    [string] $Version,
    [Parameter(ParameterSetName = 'Upload')]
    [string] $FileId,
    [Parameter(ParameterSetName = 'Upload')]
    [switch] $NewFile,
    [Parameter(ParameterSetName = 'Upload')]
    [string] $DisplayName,
    [Parameter(ParameterSetName = 'Upload')]
    [string] $Description,
    [Parameter(ParameterSetName = 'Upload')]
    [string] $Changelog,
    [Parameter(ParameterSetName = 'Upload')]
    [switch] $ArchiveExisting,
    [Parameter(ParameterSetName = 'Upload')]
    [switch] $UpdateModVersion,
    [Parameter(ParameterSetName = 'Upload')]
    [switch] $PrimaryModManagerDownload,
    [Parameter(ParameterSetName = 'Upload')]
    [switch] $ShowRequirementsPopup,

    [string] $ApiKey,
    [string] $GameDomain = 'stardewvalley',
    [string] $GameScopedModId = '49886'
)
$ErrorActionPreference = 'Stop'

$apiBase = 'https://api.nexusmods.com/v3'

if ($PSCmdlet.ParameterSetName -eq 'Upload') {
    if (-not $FileId -and -not $NewFile) {
        throw "pass -FileId <id> to add a version to an existing mod file (run -ListFiles to find the id), or -NewFile to create a new file on the mod page."
    }
    if ($FileId -and $NewFile) {
        throw "-FileId and -NewFile are mutually exclusive: a version goes onto exactly one existing file, a new file goes onto none."
    }
}

if (-not $ApiKey) { $ApiKey = $env:NEXUS_APIKEY }
if (-not $ApiKey) {
    throw "no API key. Generate a personal key at https://next.nexusmods.com/settings/api-keys, then either pass -ApiKey or set it once for your account with: setx NEXUS_APIKEY `"<key>`" (new terminals only; also run `$env:NEXUS_APIKEY='<key>' for the current one)."
}

# Windows PowerShell 5.1 on .NET Framework does not negotiate TLS 1.2 by default on older
# configurations, and api.nexusmods.com requires it. -bor preserves whatever stronger protocols the
# OS already enables rather than clamping the process down to exactly TLS 1.2.
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

Add-Type -AssemblyName System.Net.Http

# One HttpClient for every request in the run. Its default timeout of 100 seconds is far too short
# for a ~100 MB part PUT on a residential uplink, so it is raised to an hour; per-request slowness
# is handled by the part retry loop below, not by the client timeout.
$http = New-Object System.Net.Http.HttpClient
$http.Timeout = [TimeSpan]::FromHours(1)

# The apikey header is attached per-request rather than as a client default header on purpose: the
# same client also PUTs to presigned storage URLs, and a presigned URL must receive exactly the
# headers the signature expects. Leaking the Nexus credential to the storage host would also be
# sending a secret somewhere it does not belong.
function Send-Request {
    param(
        [Parameter(Mandatory)] [string] $Method,
        [Parameter(Mandatory)] [string] $Url,
        [System.Net.Http.HttpContent] $Content,
        [switch] $Authed
    )
    $request = New-Object System.Net.Http.HttpRequestMessage ([System.Net.Http.HttpMethod]::new($Method), $Url)
    if ($Authed) {
        $request.Headers.Add('apikey', $script:ApiKey)
        $request.Headers.UserAgent.ParseAdd('chatty-valley-upload-nexus/0.1')
    }
    if ($Content) { $request.Content = $Content }
    try {
        return $script:http.SendAsync($request).GetAwaiter().GetResult()
    } finally {
        $request.Dispose()
    }
}

# JSON in, JSON out against the v3 API, with the response body surfaced on failure. Nexus returns
# its error detail in the body, and an exception that says only "400" would strand the caller.
function Invoke-NexusJson {
    param(
        [Parameter(Mandatory)] [string] $Method,
        [Parameter(Mandatory)] [string] $Path,
        [hashtable] $Body
    )
    $content = $null
    if ($null -ne $Body) {
        $json = $Body | ConvertTo-Json -Depth 5
        $content = New-Object System.Net.Http.StringContent ($json, [System.Text.Encoding]::UTF8, 'application/json')
    }
    $response = Send-Request -Method $Method -Url "$script:apiBase$Path" -Content $content -Authed
    try {
        $text = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        if (-not $response.IsSuccessStatusCode) {
            throw "$Method $Path failed: HTTP $([int]$response.StatusCode) - $text"
        }
        if ($text) { return $text | ConvertFrom-Json }
        return $null
    } finally {
        $response.Dispose()
    }
}

function Resolve-Mod {
    $mod = (Invoke-NexusJson -Method GET -Path "/games/$GameDomain/mods/$GameScopedModId").data
    Write-Host "mod resolved: $GameDomain/$GameScopedModId -> id $($mod.id) ($($mod.name))"
    return $mod
}

# ===========================================================================================
# -ListFiles: resolve the mod, print every mod file with its id. Doubles as the API key smoke
# test, since both calls fail loudly on a bad or missing key.
# ===========================================================================================

if ($ListFiles) {
    $mod = Resolve-Mod
    $files = (Invoke-NexusJson -Method GET -Path "/mods/$($mod.id)/files").data.mod_files
    if (-not $files) {
        Write-Host "no mod files found on this mod page."
        return
    }
    $files | ForEach-Object {
        [pscustomobject]@{
            FileId        = $_.id
            Name          = $_.name
            Active        = $_.is_active
            Versions      = $_.versions_count
            Archived      = $_.archived_count
            LastUploaded  = $_.last_file_uploaded_at
        }
    } | Format-Table -AutoSize
    Write-Host "pass a FileId above to upload a new version of that file."
    return
}

# ===========================================================================================
# Upload mode.
# ===========================================================================================

$zip = Resolve-Path $ZipPath
$zipLeaf = Split-Path $zip -Leaf
$sizeBytes = (Get-Item $zip).Length
$sizeMb = [math]::Round($sizeBytes / 1MB, 1)
if (-not $DisplayName) { $DisplayName = $zipLeaf }

# The changelog endpoint and -NewFile both need the mod's v3 id; a plain -FileId version upload
# does not. Resolving up front anyway costs one GET and confirms the key before the long upload.
$mod = Resolve-Mod

Write-Host "uploading $zipLeaf ($sizeMb MB) as version $Version"

# --- 1. create the multipart upload session -----------------------------------------------
# size_bytes goes over the wire as a string: that is what the reference client sends, and int64
# via ConvertTo-Json would be a number. Kept identical to the known-working client on purpose.
$session = (Invoke-NexusJson -Method POST -Path '/uploads/multipart' -Body @{
    filename   = $zipLeaf
    size_bytes = [string]$sizeBytes
}).data
$uploadId = $session.id
$partSize = [long]$session.part_size_bytes
$partUrls = @($session.part_presigned_urls)
Write-Host "upload session $uploadId ($($partUrls.Count) parts of $([math]::Round($partSize / 1MB, 1)) MB)"

# --- 2. PUT every part --------------------------------------------------------------------
# Sequential on purpose: a residential uplink is the bottleneck, so concurrency buys little and
# costs runspace plumbing under 5.1. Each part retries because losing a 744 MB upload to one
# transient socket error at part 60 of 75 is the failure mode that matters here.
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$etags = New-Object System.Collections.Generic.List[object]
$buffer = New-Object byte[] $partSize
$stream = [System.IO.File]::OpenRead($zip)
try {
    for ($i = 0; $i -lt $partUrls.Count; $i++) {
        $partNumber = $i + 1

        # FileStream.Read may legally return fewer bytes than asked even mid-file, so fill the
        # buffer in a loop rather than trust one call. The final part is simply whatever is left.
        $wanted = [int][math]::Min($partSize, $sizeBytes - ([long]$i * $partSize))
        $filled = 0
        while ($filled -lt $wanted) {
            $read = $stream.Read($buffer, $filled, $wanted - $filled)
            if ($read -le 0) { throw "unexpected end of file at part $partNumber (wanted $wanted bytes, got $filled)" }
            $filled += $read
        }

        $etag = $null
        for ($attempt = 1; $attempt -le 3; $attempt++) {
            try {
                $content = New-Object System.Net.Http.ByteArrayContent ($buffer, 0, $wanted)
                $content.Headers.ContentType = New-Object System.Net.Http.Headers.MediaTypeHeaderValue 'application/octet-stream'
                $response = Send-Request -Method PUT -Url $partUrls[$i] -Content $content
                try {
                    if (-not $response.IsSuccessStatusCode) {
                        $text = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                        throw "part $partNumber PUT failed: HTTP $([int]$response.StatusCode) - $text"
                    }
                    # The completion XML below is meaningless without the ETag, so a storage host
                    # or proxy that strips it must fail the part now, not at completion time.
                    if (-not $response.Headers.ETag) { throw "part $partNumber uploaded but no ETag came back" }
                    $etag = $response.Headers.ETag.Tag.Trim('"')
                } finally {
                    $response.Dispose()
                }
                break
            } catch {
                if ($attempt -eq 3) { throw }
                Write-Host "  part $partNumber attempt $attempt failed ($($_.Exception.Message)); retrying"
                Start-Sleep -Seconds (5 * $attempt)
            }
        }
        $etags.Add(@{ PartNumber = $partNumber; ETag = $etag })

        $doneMb = [math]::Round((([long]$i * $partSize) + $wanted) / 1MB, 1)
        $rate = [math]::Round($doneMb / [math]::Max($stopwatch.Elapsed.TotalSeconds, 1), 2)
        Write-Host "  part $partNumber/$($partUrls.Count) done ($doneMb / $sizeMb MB, $rate MB/s)"
    }
} finally {
    $stream.Dispose()
}

# --- 3. complete the multipart upload -----------------------------------------------------
$partsXml = ($etags | ForEach-Object { "  <Part>`n    <PartNumber>$($_.PartNumber)</PartNumber>`n    <ETag>$($_.ETag)</ETag>`n  </Part>" }) -join "`n"
$completeXml = "<CompleteMultipartUpload>`n$partsXml`n</CompleteMultipartUpload>"
$content = New-Object System.Net.Http.StringContent ($completeXml, [System.Text.Encoding]::UTF8, 'application/xml')
$response = Send-Request -Method POST -Url $session.complete_presigned_url -Content $content
try {
    if (-not $response.IsSuccessStatusCode) {
        $text = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        throw "multipart completion failed: HTTP $([int]$response.StatusCode) - $text"
    }
} finally {
    $response.Dispose()
}
Write-Host "all parts uploaded and completed in $([math]::Round($stopwatch.Elapsed.TotalMinutes, 1)) min"

# --- 4 + 5. finalise, then poll until the upload is ingested ------------------------------
Invoke-NexusJson -Method POST -Path "/uploads/$uploadId/finalise" | Out-Null

# Same pacing as the reference client: 2 s base, 1.5x backoff capped at 30 s, 60 attempts.
$state = ''
for ($attempt = 0; $attempt -lt 60; $attempt++) {
    $state = (Invoke-NexusJson -Method GET -Path "/uploads/$uploadId").data.state
    Write-Host "  upload state: $state"
    if ($state -eq 'available') { break }
    Start-Sleep -Seconds ([math]::Min(2 * [math]::Pow(1.5, $attempt), 30))
}
if ($state -ne 'available') { throw "upload $uploadId never reached state 'available'; last state was '$state'" }

# --- 6. attach the upload to the mod page -------------------------------------------------
if ($NewFile) {
    $body = @{
        upload_id                    = $uploadId
        mod_id                       = $mod.id
        name                         = $DisplayName
        version                      = $Version
        file_category                = 'main'
        primary_mod_manager_download = [bool]$PrimaryModManagerDownload
        update_mod_version           = [bool]$UpdateModVersion
    }
    if ($Description) { $body.description = $Description }
    $result = (Invoke-NexusJson -Method POST -Path '/mod-files' -Body $body).data
    Write-Host "new mod file created: id $($result.id)"
} else {
    $body = @{
        upload_id             = $uploadId
        name                  = $DisplayName
        version               = $Version
        file_category         = 'main'
        archive_existing_file = [bool]$ArchiveExisting
        update_mod_version    = [bool]$UpdateModVersion
    }
    if ($Description) { $body.description = $Description }
    if ($PrimaryModManagerDownload) { $body.primary_mod_manager_download = $true }
    if ($ShowRequirementsPopup) { $body.show_requirements_pop_up = $true }
    $result = (Invoke-NexusJson -Method POST -Path "/mod-files/$FileId/versions" -Body $body).data
    Write-Host "new version created on file ${FileId}: version id $($result.version.id)"
}

# --- 7. changelog, if provided ------------------------------------------------------------
if ($Changelog) {
    Invoke-NexusJson -Method POST -Path "/mods/$($mod.id)/changelogs" -Body @{
        version   = $Version
        changelog = $Changelog
    } | Out-Null
    Write-Host "changelog added for $Version"
}

Write-Host ""
Write-Host "done. Review the file on the mod page:"
Write-Host "  https://www.nexusmods.com/$GameDomain/mods/$GameScopedModId`?tab=files"
