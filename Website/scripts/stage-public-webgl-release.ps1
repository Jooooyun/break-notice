[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [ValidatePattern('^[0-9a-f]{32}$')]
  [string]$ExpectedBuildGuid,

  [Parameter(Mandatory = $true)]
  [ValidatePattern('^[0-9a-f]{40}$')]
  [string]$SourceCommitSha,

  [string]$StagingRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.Net.Http

if ([string]::IsNullOrWhiteSpace($StagingRoot)) {
  $StagingRoot = Join-Path $PSScriptRoot '..\deployment-staging\webgl'
}

$websiteRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$resolvedStagingRoot = [IO.Path]::GetFullPath($StagingRoot)
$allowedStagingRoot = [IO.Path]::GetFullPath((Join-Path $websiteRoot 'deployment-staging\webgl'))
if (-not $resolvedStagingRoot.Equals($allowedStagingRoot, [StringComparison]::OrdinalIgnoreCase)) {
  throw "StagingRoot must be the ignored Website deployment staging directory: $allowedStagingRoot"
}

$historyPath = Join-Path $websiteRoot 'data\webgl-releases.json'
$history = Get-Content -LiteralPath $historyPath -Raw | ConvertFrom-Json
$releaseMatches = @($history.releases | Where-Object { $_.buildGuid -eq $ExpectedBuildGuid })
if ($releaseMatches.Count -ne 1) { throw "Release history must contain exactly one matching Build GUID: $ExpectedBuildGuid" }
$release = $releaseMatches[0]
if ($release.sourceSha -ne $SourceCommitSha) { throw 'Release history source SHA mismatch.' }

$releaseUri = [Uri]$release.publicUrl
if ($releaseUri.Scheme -ne 'https' -or -not [string]::IsNullOrEmpty($releaseUri.UserInfo)) {
  throw "Public release URL must be credential-free HTTPS: $releaseUri"
}
$expectedSuffix = "/play/releases/$ExpectedBuildGuid/"
if (-not $releaseUri.AbsolutePath.EndsWith($expectedSuffix, [StringComparison]::Ordinal)) {
  throw "Public release URL does not end with the immutable GUID route: $releaseUri"
}

$inventoryPath = Join-Path $websiteRoot "data\webgl-release-inventory-$ExpectedBuildGuid.json"
$inventory = Get-Content -LiteralPath $inventoryPath -Raw | ConvertFrom-Json
if ($inventory.buildGuid -ne $ExpectedBuildGuid -or $inventory.sourceSha -ne $SourceCommitSha) {
  throw 'Recorded inventory identity mismatch.'
}

$releaseRoot = Join-Path $resolvedStagingRoot "play\releases\$ExpectedBuildGuid"
if (Test-Path -LiteralPath $releaseRoot) {
  throw "Immutable release directory already exists; overwrite is prohibited: $releaseRoot"
}

$handler = [Net.Http.HttpClientHandler]::new()
$handler.AllowAutoRedirect = $false
$handler.AutomaticDecompression = [Net.DecompressionMethods]::None
$client = [Net.Http.HttpClient]::new($handler)
$client.DefaultRequestHeaders.TryAddWithoutValidation('Accept-Encoding', 'identity') | Out-Null

New-Item -ItemType Directory -Path $releaseRoot | Out-Null
try {
  foreach ($file in $inventory.files) {
    $relativePath = [string]$file.path
    if ($relativePath.StartsWith('/') -or $relativePath.Split('/') -contains '..') {
      throw "Unsafe recorded path: $relativePath"
    }

    $publicPath = if ($file.PSObject.Properties.Name -contains 'publicPath') {
      [string]$file.publicPath
    } else {
      $relativePath
    }
    if ($publicPath.StartsWith('/') -or $publicPath.Split('/') -contains '..') {
      throw "Unsafe recorded public path: $publicPath"
    }

    $sourceUri = [Uri]::new($releaseUri, $publicPath)
    if ($sourceUri.Host -ne $releaseUri.Host -or $sourceUri.Scheme -ne 'https') {
      throw "Public asset escaped release origin: $sourceUri"
    }

    $destination = [IO.Path]::GetFullPath((Join-Path $releaseRoot $relativePath.Replace('/', '\')))
    $allowedPrefix = [IO.Path]::GetFullPath($releaseRoot).TrimEnd('\') + '\'
    if (-not $destination.StartsWith($allowedPrefix, [StringComparison]::OrdinalIgnoreCase)) {
      throw "Destination escaped release root: $destination"
    }

    $response = $client.GetAsync($sourceUri, [Net.Http.HttpCompletionOption]::ResponseHeadersRead).GetAwaiter().GetResult()
    try {
      if ([int]$response.StatusCode -ne 200) { throw "HTTP $([int]$response.StatusCode): $sourceUri" }
      if ($response.Content.Headers.ContentEncoding.Count -ne 0) {
        throw "Unexpected Content-Encoding for immutable source: $sourceUri"
      }
      if ($null -ne $response.Content.Headers.ContentLength -and
          [long]$response.Content.Headers.ContentLength -ne [long]$file.bytes) {
        throw "Content-Length mismatch: $sourceUri"
      }

      $destinationDirectory = Split-Path -Parent $destination
      New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
      $inputStream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
      $outputStream = [IO.File]::Create($destination)
      try {
        $inputStream.CopyTo($outputStream)
      } finally {
        $outputStream.Dispose()
        $inputStream.Dispose()
      }
    } finally {
      $response.Dispose()
    }
  }

  $verifyScript = Join-Path $PSScriptRoot 'verify-webgl-release.ps1'
  & $verifyScript -ArtifactDirectory $releaseRoot -ExpectedBuildGuid $ExpectedBuildGuid -SourceCommitSha $SourceCommitSha | Out-Host
} catch {
  if (Test-Path -LiteralPath $releaseRoot) { Remove-Item -LiteralPath $releaseRoot -Recurse -Force }
  throw
} finally {
  $client.Dispose()
  $handler.Dispose()
}

[pscustomobject]@{
  Result = 'PASS'
  ReleaseDirectory = $releaseRoot
  Source = [string]$release.publicUrl
} | ConvertTo-Json
