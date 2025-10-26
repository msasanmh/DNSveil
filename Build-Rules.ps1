# Build-Rules.ps1
$ErrorActionPreference = "Stop"
$urlsFile = ".\urls.txt"
$out = ".\malicious_rules.txt"
"" | Out-File $out -Encoding utf8
Add-Content $out "# Aggregated malicious_rules.txt — generated on $(Get-Date -AsUTC -Format 'yyyy-MM-ddTHH:mm:ssZ')"

Get-Content $urlsFile | ForEach-Object {
  $line = $_.Trim()
  if ($line -eq "" -or $line.StartsWith("#")) { return }
  Add-Content $out "# from $line"
  try {
    $resp = Invoke-RestMethod -Uri $line -UseBasicParsing -ErrorAction Stop
  } catch {
    Write-Warning "Failed to fetch $line"
    return
  }
  $resp -split "`n" | ForEach-Object {
    $ltrim = $_.Trim()
    if ($ltrim -eq "" -or $ltrim.StartsWith("#")) { return }
    # split fields
    $parts = -split $ltrim
    if ($parts.Count -ge 2 -and ($parts[0] -match '^\d+\.\d+\.\d+\.\d+$' -or $parts[0] -eq "0.0.0.0")) {
      $host = $parts[-1]
    } else {
      $host = $parts[0]
    }
    $host = $host.ToLower().Trim()
    if ($host -match "[/:@]") { return }
    if ($host -match '^\d+\.\d+\.\d+\.\d+$') { return }
    $host = $host.TrimStart('.')
    if ($host -notmatch '\.') { return }
    "$host|-;" | Out-File -Append -Encoding utf8 $out
  }
}

# dedupe while keeping comments
$lines = Get-Content $out
$seen = @{}
$outLines = New-Object System.Collections.Generic.List[string]
foreach ($ln in $lines) {
  if ($ln -match '^\s*#') { $outLines.Add($ln); continue }
  if (-not $seen.ContainsKey($ln)) { $seen[$ln] = $true; $outLines.Add($ln) }
}
$outLines | Set-Content $out -Encoding utf8
Write-Output "Generated $out with $((Get-Content $out | Select-String '\|-;').Count) rule lines. Review before importing."
