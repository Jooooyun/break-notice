[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string]$ArtifactDirectory,

  [Parameter(Mandatory = $true)]
  [ValidatePattern('^[0-9a-f]{32}$')]
  [string]$ExpectedBuildGuid,

  [Parameter(Mandatory = $true)]
  [ValidatePattern('^[0-9a-f]{40}$')]
  [string]$SourceCommitSha,

  [string]$InventoryPath,

  [string]$CompareDirectory
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($InventoryPath)) {
  $InventoryPath = Join-Path $PSScriptRoot "..\data\webgl-release-inventory-$ExpectedBuildGuid.json"
}

function Get-NormalizedRelativePath {
  param([string]$Root, [string]$FullName)
  $rootPath = [IO.Path]::GetFullPath($Root).TrimEnd('\')
  $filePath = [IO.Path]::GetFullPath($FullName)
  if (-not $filePath.StartsWith($rootPath + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw "Path escaped artifact root: $filePath"
  }
  $relative = $filePath.Substring($rootPath.Length + 1).Replace('\', '/')
  if ([string]::IsNullOrWhiteSpace($relative) -or $relative.StartsWith('/') -or $relative.Split('/') -contains '..') {
    throw "Unsafe relative path: $relative"
  }
  return $relative
}

function Get-ArtifactInventory {
  param([string]$Root)
  $resolvedRoot = (Resolve-Path -LiteralPath $Root).Path
  $items = foreach ($file in Get-ChildItem -LiteralPath $resolvedRoot -Recurse -File) {
    $relative = Get-NormalizedRelativePath -Root $resolvedRoot -FullName $file.FullName
    [pscustomobject]@{
      Path = $relative
      Bytes = [long]$file.Length
      Sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $file.FullName).Hash.ToLowerInvariant()
      FullName = $file.FullName
    }
  }
  return @($items | Sort-Object Path)
}

$resolvedArtifact = (Resolve-Path -LiteralPath $ArtifactDirectory).Path
$metadataPath = Join-Path $resolvedArtifact 'BuildMetadata.json'
if (-not (Test-Path -LiteralPath $metadataPath -PathType Leaf)) {
  throw 'BuildMetadata.json is missing.'
}

$metadata = Get-Content -LiteralPath $metadataPath -Raw | ConvertFrom-Json
$resolvedInventory = (Resolve-Path -LiteralPath $InventoryPath).Path
$record = Get-Content -LiteralPath $resolvedInventory -Raw | ConvertFrom-Json
if ($record.buildGuid -ne $ExpectedBuildGuid) { throw 'Recorded inventory Build GUID mismatch.' }
if ($record.sourceSha -ne $SourceCommitSha) { throw 'Recorded inventory source SHA mismatch.' }

$historyPath = Join-Path $PSScriptRoot '..\data\webgl-releases.json'
$history = Get-Content -LiteralPath $historyPath -Raw | ConvertFrom-Json
$releaseMatches = @($history.releases | Where-Object { $_.buildGuid -eq $ExpectedBuildGuid })
if ($releaseMatches.Count -ne 1) { throw "Release history must contain exactly one matching Build GUID: $ExpectedBuildGuid" }
$release = $releaseMatches[0]
if ($release.sourceSha -ne $SourceCommitSha) { throw 'Release history source SHA mismatch.' }
if ($release.unityVersion -ne $metadata.unityVersion) { throw "Unity version mismatch: $($metadata.unityVersion)" }
if ([long]$release.unityReportedBytes -ne [long]$metadata.totalBytes) {
  throw "Unity-reported byte count mismatch: $($metadata.totalBytes)"
}
if ($metadata.result -ne 'Succeeded') { throw "Build result is not Succeeded: $($metadata.result)" }
if ($metadata.buildGuid -ne $ExpectedBuildGuid) { throw "Build GUID mismatch: $($metadata.buildGuid)" }
if ([int]$metadata.totalWarnings -ne 0 -or [int]$metadata.totalErrors -ne 0) {
  throw "Unity warnings/errors are non-zero: $($metadata.totalWarnings)/$($metadata.totalErrors)"
}

if ($record.PSObject.Properties.Name -contains 'unityVersion' -and $record.unityVersion -ne $metadata.unityVersion) {
  throw 'Recorded inventory Unity version mismatch.'
}
if ($record.PSObject.Properties.Name -contains 'scene') {
  if (-not ($metadata.PSObject.Properties.Name -contains 'scene') -or $record.scene -ne $metadata.scene) {
    throw "Recorded Scene mismatch: $($metadata.scene)"
  }
}
if ($record.PSObject.Properties.Name -contains 'showcaseCheckpoint') {
  if (-not ($metadata.PSObject.Properties.Name -contains 'showcaseCheckpoint') -or
      $record.showcaseCheckpoint -ne $metadata.showcaseCheckpoint) {
    throw "Recorded showcase checkpoint mismatch: $($metadata.showcaseCheckpoint)"
  }
}

$actual = @(Get-ArtifactInventory -Root $resolvedArtifact)
$expected = @($record.files)
if ($actual.Count -ne [int]$record.fileCount -or $expected.Count -ne [int]$record.fileCount) {
  throw "File count mismatch. Actual=$($actual.Count) Recorded=$($record.fileCount)"
}

$required = @(
  'index.html',
  'BuildMetadata.json',
  'Build/Development.loader.js',
  'Build/Development.framework.js',
  'Build/Development.data',
  'Build/Development.wasm'
)
foreach ($path in $required) {
  if (-not ($actual.Path -ccontains $path)) { throw "Required WebGL file missing: $path" }
}

$expectedByPath = @{}
foreach ($file in $expected) {
  if ($file.path.StartsWith('/') -or $file.path.Split('/') -contains '..' -or $expectedByPath.ContainsKey($file.path)) {
    throw "Unsafe or duplicate recorded path: $($file.path)"
  }
  $expectedByPath[$file.path] = $file
}

foreach ($file in $actual) {
  if (-not $expectedByPath.ContainsKey($file.Path)) { throw "Unexpected file: $($file.Path)" }
  $expectedFile = $expectedByPath[$file.Path]
  if ($file.Bytes -ne [long]$expectedFile.bytes) { throw "Byte mismatch: $($file.Path)" }
  if ($file.Sha256 -cne [string]$expectedFile.sha256) { throw "SHA-256 mismatch: $($file.Path)" }
}

$totalBytes = [long](($actual | Measure-Object Bytes -Sum).Sum)
if ($totalBytes -ne [long]$record.httpBytes) { throw "Total byte mismatch: $totalBytes" }
if ([int]$release.fileCount -ne [int]$record.fileCount -or
    [long]$release.httpBytes -ne [long]$record.httpBytes -or
    [string]$release.aggregateSha256 -cne [string]$record.aggregateSha256) {
  throw 'Release history and recorded inventory evidence differ.'
}

$aggregateLines = @(
  $actual |
    Sort-Object @{ Expression = { $_.Path.ToLowerInvariant() } }, @{ Expression = { $_.Path } } |
    ForEach-Object { "$($_.Path)`t$($_.Sha256)" }
)
$aggregateInput = ($aggregateLines -join "`n") + "`n"
$aggregateBytes = [Text.Encoding]::UTF8.GetBytes($aggregateInput)
$aggregateAlgorithm = [Security.Cryptography.SHA256]::Create()
try {
  $aggregateSha256 = ([BitConverter]::ToString($aggregateAlgorithm.ComputeHash($aggregateBytes))).Replace('-', '').ToLowerInvariant()
} finally {
  $aggregateAlgorithm.Dispose()
}
if ($aggregateSha256 -cne [string]$record.aggregateSha256) {
  throw "Aggregate SHA-256 mismatch: $aggregateSha256"
}

if ($CompareDirectory) {
  $resolvedCompare = (Resolve-Path -LiteralPath $CompareDirectory).Path
  $copied = @(Get-ArtifactInventory -Root $resolvedCompare)
  if ($copied.Count -ne $actual.Count) { throw 'Copied file count differs from source.' }
  for ($index = 0; $index -lt $actual.Count; $index++) {
    if ($actual[$index].Path -cne $copied[$index].Path -or
        $actual[$index].Bytes -ne $copied[$index].Bytes -or
        $actual[$index].Sha256 -cne $copied[$index].Sha256) {
      throw "Copied artifact identity mismatch: $($actual[$index].Path)"
    }
  }
}

[pscustomobject]@{
  Result = 'PASS'
  BuildGuid = $ExpectedBuildGuid
  SourceSha = $SourceCommitSha
  UnityVersion = $metadata.unityVersion
  FileCount = $actual.Count
  UnityReportedBytes = [long]$metadata.totalBytes
  HttpBytes = $totalBytes
  AggregateSha256 = $aggregateSha256
  ComparedCopy = [bool]$CompareDirectory
} | ConvertTo-Json
