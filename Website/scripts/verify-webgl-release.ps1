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
if ($metadata.result -ne 'Succeeded') { throw "Build result is not Succeeded: $($metadata.result)" }
if ($metadata.buildGuid -ne $ExpectedBuildGuid) { throw "Build GUID mismatch: $($metadata.buildGuid)" }
if ($metadata.unityVersion -ne '6000.3.20f1') { throw "Unity version mismatch: $($metadata.unityVersion)" }
if ([long]$metadata.totalBytes -ne 113881344) { throw "Unity-reported byte count mismatch: $($metadata.totalBytes)" }
if ([int]$metadata.totalWarnings -ne 0 -or [int]$metadata.totalErrors -ne 0) {
  throw "Unity warnings/errors are non-zero: $($metadata.totalWarnings)/$($metadata.totalErrors)"
}

$resolvedInventory = (Resolve-Path -LiteralPath $InventoryPath).Path
$record = Get-Content -LiteralPath $resolvedInventory -Raw | ConvertFrom-Json
if ($record.buildGuid -ne $ExpectedBuildGuid) { throw 'Recorded inventory Build GUID mismatch.' }
if ($record.sourceSha -ne $SourceCommitSha) { throw 'Recorded inventory source SHA mismatch.' }

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
  AggregateSha256 = [string]$record.aggregateSha256
  ComparedCopy = [bool]$CompareDirectory
} | ConvertTo-Json
