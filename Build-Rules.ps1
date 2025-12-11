<#
Build-Rules.ps1 — robust aggregator for Windows PowerShell
Produces raw_domains.txt and malicious_rules.txt (domain|-;)
Requires PowerShell 5+
Place urls.txt in same folder (one URL per line; inline # comments allowed)
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptDir

$urlsFile = Join-Path $scriptDir 'urls.txt'
$outRules = Join-Path $scriptDir 'malicious_rules.txt'
$outRaw = Join-Path $scriptDir 'raw_domains.txt'
$failedLog = Join-Path $scriptDir 'failed_fetches.txt'
$tmpDir = Join-Path $scriptDir 'tmp_fetch'
if (Test-Path $tmpDir) { Remove-Item -Recurse -Force $tmpDir }
New-Item -ItemType Directory -Path $tmpDir | Out-Null

# Clear outputs
'' | Out-File $outRules -Encoding utf8
'' | Out-File $outRaw -Encoding utf8
'' | Out-File $failedLog -Encoding utf8

function Sanitize-Url([string]$line) {
  if ([string]::IsNullOrWhiteSpace($line)) { return $null }
  $noBOM = $line -replace "^\uFEFF",""
  $idx = $noBOM.IndexOf('#')
  if ($idx -ge 0) { $noBOM = $noBOM.Substring(0, $idx) }
  $trimmed = $noBOM.Trim()
  if ($trimmed -eq '') { return $null }
  return $trimmed
}

function Fetch-One([string]$url, [string]$outRaw) {
  try {
    # Invoke-WebRequest returns HTML object sometimes; use -UseBasicParsing for raw content
    $web = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 60 -ErrorAction Stop -Headers @{ 'User-Agent' = 'blocklist-fetcher/1.0' }
    $content = $web.Content
  } catch {
    "$url" | Out-File -FilePath $failedLog -Append -Encoding utf8
    return
  }

  # If content appears to be HTML, skip
  if ($content -match '<(html|!doctype|head|body)' ) {
    "HTML page (skipped): $url" | Out-File -FilePath $failedLog -Append -Encoding utf8
    return
  }

  # Normalize and extract host-like lines
  $lines = $content -split "`n"
  foreach ($l in $lines) {
    $s = $l.Trim()
    if ($s -eq '' -or $s.StartsWith('#')) { continue }
    # remove leading hosts ip fields
    $s = $s -replace '^(0\.0\.0\.0|127\.0\.0\.1|::1)\s+',''
    # remove scheme + path
    $s = $s -replace '^https?://',''
    $s = $s -replace '/.*$',''
    $s = $s.Trim()
    if ($s -match '^[A-Za-z0-9][A-Za-z0-9\.\-]{2,}\.[A-Za-z]{2,}$') {
      $s.ToLower() | Out-File -FilePath $outRaw -Append -Encoding utf8
    }
  }
}

# Read URLs
if (-not (Test-Path $urlsFile)) { Write-Error "urls.txt not found in $scriptDir"; exit 1 }
$urls = Get-Content $urlsFile | ForEach-Object { Sanitize-Url $_ } | Where-Object { $_ -ne $null }

if ($urls.Count -eq 0) { Write-Error "No URLs found in urls.txt"; exit 1 }

# Fetch sequentially (PowerShell parallelism is more complex; do sequential to be robust)
foreach ($u in $urls) {
  Write-Host "Fetching $u"
  Fetch-One -url $u -outRaw $outRaw
}

# Extract and dedupe domains
Get-Content $outRaw |
  ForEach-Object { $_.ToLower().Trim() } |
  Where-Object { $_ -match '([A-Za-z0-9]([A-Za-z0-9\-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}' } |
  Sort-Object -Unique |
  Where-Object { $_.Length -ge 4 } |
  Tee-Object -FilePath "$tmpDir\domains_sorted.txt" |
  ForEach-Object { "$_|-;" } |
  Set-Content -Path $outRules

# Move sorted raw list to outRaw
Get-Content "$tmpDir\domains_sorted.txt" | Set-Content -Path $outRaw

Write-Host "Generated $outRules with $((Get-Content $outRules).Count) rule lines."
if (Test-Path $failedLog -and (Get-Content $failedLog).Count -gt 0) {
  Write-Host "Some sources failed or were skipped. See $failedLog"
}

Pop-Location
