param(
    [string]$BaseUrl = "http://127.0.0.1:8765/"
)

. (Join-Path $PSScriptRoot "ContestCommon.ps1")
$context = Get-ContestContext
$indexPath = Join-Path $context.BuildPath "index.html"

if (-not (Test-Path -LiteralPath $indexPath -PathType Leaf)) {
    throw "Web build index does not exist: $indexPath"
}

$files = @(Get-ChildItem -LiteralPath $context.BuildPath -Recurse -File | Sort-Object FullName)
if ($files.Count -eq 0) {
    throw "Web build contains no files: $($context.BuildPath)"
}

$httpChecks = @()
foreach ($file in $files) {
    $relativePath = $file.FullName.Substring($context.BuildPath.Length).TrimStart('\', '/')
    $relativeUrl = $relativePath.Replace('\', '/')
    $uri = [Uri]::new([Uri]$BaseUrl, $relativeUrl)
    $response = Invoke-WebRequest -Uri $uri.AbsoluteUri -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -ne 200) {
        throw "HTTP verification failed for $($uri.AbsoluteUri): $($response.StatusCode)"
    }

    $httpChecks += [PSCustomObject]@{
        Path = $relativeUrl
        Status = $response.StatusCode
    }
}

$hashLines = foreach ($file in $files) {
    $relativePath = $file.FullName.Substring($context.BuildPath.Length).TrimStart('\', '/')
    $relativePath = $relativePath.Replace('\', '/')
    $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    "$relativePath`t$hash"
}

$hashPayload = [Text.Encoding]::UTF8.GetBytes(($hashLines -join "`n") + "`n")
$sha256 = [Security.Cryptography.SHA256]::Create()
try {
    $aggregateHash = ([BitConverter]::ToString($sha256.ComputeHash($hashPayload))).Replace("-", "").ToLowerInvariant()
}
finally {
    $sha256.Dispose()
}

$result = [PSCustomObject]@{
    Result = "PASS"
    BaseUrl = $BaseUrl
    FileCount = $files.Count
    TotalBytes = [long](($files | Measure-Object -Property Length -Sum).Sum)
    AggregateSha256 = $aggregateHash
    HttpChecks = $httpChecks
}

$evidencePath = Join-Path $context.LogPath "web-release-verification.json"
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $evidencePath -Encoding UTF8
$result
