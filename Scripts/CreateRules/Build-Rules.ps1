#!/usr/bin/env pwsh
# build_rules.ps1 — DNSveil blocklist builder (job-based workers)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# -------------------------
# Paths
# -------------------------
$BaseDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$UrlsFile  = Join-Path $BaseDir 'urls.txt'
$OutRaw    = Join-Path $BaseDir 'raw_domains.txt'
$OutRules  = Join-Path $BaseDir 'malicious_rules.txt'
$FailedLog = Join-Path $BaseDir 'failed_fetches.txt'
$CacheDir  = Join-Path $BaseDir 'cache'

New-Item -ItemType Directory -Force -Path $CacheDir | Out-Null

'' | Set-Content $OutRaw
'' | Set-Content $OutRules
'' | Set-Content $FailedLog

# -------------------------
# Helpers (serial scope)
# -------------------------
function Sanitize-Line {
    param([string]$Line)
    ($Line -replace '^\uFEFF','' -replace '#.*$','').Trim()
}

function Convert-GitHubRaw {
    param([string]$Url)
    if ($Url -match '^https://github\.com/([^/]+)/([^/]+)/blob/([^/]+)/(.*)$') {
        return "https://raw.githubusercontent.com/$($Matches[1])/$($Matches[2])/$($Matches[3])/$($Matches[4])"
    }
    $Url
}

function Get-CachePath {
    param([string]$Url)
    $hash = [BitConverter]::ToString(
        [System.Security.Cryptography.SHA1]::Create().ComputeHash(
            [Text.Encoding]::UTF8.GetBytes($Url)
        )
    ).Replace('-', '').ToLower()
    Join-Path $CacheDir "$hash.txt"
}

# -------------------------
# Worker scriptblock
# -------------------------
$Worker = {
    param($Url, $CachePath)

    try {
        $resp = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 60
        if ($resp.Content -match '<(html|!doctype|head|body)') {
            throw "HTML response"
        }

        $resp.Content -split "`n" |
            ForEach-Object { $_ -replace "`r",'' } |
            ForEach-Object { $_ -replace '#.*$','' } |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ } |
            ForEach-Object { $_ -replace '^(0\.0\.0\.0|127\.0\.0\.1|::1)\s+','' } |
            ForEach-Object { $_ -replace '^https?://','' } |
            ForEach-Object { $_ -replace '/.*$','' } |
            Set-Content $CachePath
    }
    catch {
        "FAILED:$Url"
    }
}

# -------------------------
# Load URLs
# -------------------------
$Urls = Get-Content $UrlsFile |
    ForEach-Object { Sanitize-Line $_ } |
    Where-Object { $_ } |
    ForEach-Object { Convert-GitHubRaw $_ }

# -------------------------
# Dispatch jobs
# -------------------------
$Jobs = @()

foreach ($u in $Urls) {
    $cache = Get-CachePath $u
    if (-not (Test-Path $cache)) {
        Write-Host "Queueing $u"
        $Jobs += Start-Job -ScriptBlock $Worker -ArgumentList $u, $cache
    }
}

# -------------------------
# Wait + collect
# -------------------------
if ($Jobs.Count -gt 0) {
    Wait-Job $Jobs | Out-Null
}

foreach ($j in $Jobs) {
    $result = Receive-Job $j
    if ($result -and $result -like 'FAILED:*') {
        $result.Substring(7) | Add-Content $FailedLog
    }
    Remove-Job $j
}

# -------------------------
# Aggregate cache → raw
# -------------------------
Get-ChildItem $CacheDir -Filter '*.txt' |
    ForEach-Object { Get-Content $_.FullName } |
    Add-Content $OutRaw

# -------------------------
# Extract domains
# -------------------------
$Domains = Get-Content $OutRaw |
    Select-String -Pattern '([a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,}' -AllMatches |
    ForEach-Object { $_.Matches.Value } |
    ForEach-Object { $_.ToLowerInvariant() } |
    Sort-Object -Unique

$Domains | Set-Content $OutRaw
$Domains | ForEach-Object { "$_|-;" } | Set-Content $OutRules

Write-Host "Generated $OutRules with $($Domains.Count) rule lines."

if ((@(Get-Content $FailedLog -ErrorAction SilentlyContinue)).Count -gt 0) {
    Write-Host "Some sources failed. See $FailedLog"
}
