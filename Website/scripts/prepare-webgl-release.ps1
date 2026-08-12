[CmdletBinding(SupportsShouldProcess = $true)]
param(
  [Parameter(Mandatory = $true)]
  [string]$ArtifactDirectory,

  [Parameter(Mandatory = $true)]
  [ValidatePattern('^[0-9a-f]{32}$')]
  [string]$ExpectedBuildGuid,

  [Parameter(Mandatory = $true)]
  [ValidatePattern('^[0-9a-f]{40}$')]
  [string]$SourceCommitSha,

  [switch]$PromoteLatest,

  [string]$StagingRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($StagingRoot)) {
  $StagingRoot = Join-Path $PSScriptRoot '..\deployment-staging\webgl'
}

$websiteRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$resolvedStagingRoot = [IO.Path]::GetFullPath($StagingRoot)
$allowedStagingRoot = [IO.Path]::GetFullPath((Join-Path $websiteRoot 'deployment-staging\webgl'))
if (-not $resolvedStagingRoot.Equals($allowedStagingRoot, [StringComparison]::OrdinalIgnoreCase)) {
  throw "StagingRoot must be the ignored Website deployment staging directory: $allowedStagingRoot"
}

$releaseRoot = Join-Path $resolvedStagingRoot "play\releases\$ExpectedBuildGuid"
if (Test-Path -LiteralPath $releaseRoot) {
  throw "Immutable release directory already exists; overwrite is prohibited: $releaseRoot"
}

$verifyScript = Join-Path $PSScriptRoot 'verify-webgl-release.ps1'
& $verifyScript -ArtifactDirectory $ArtifactDirectory -ExpectedBuildGuid $ExpectedBuildGuid -SourceCommitSha $SourceCommitSha | Out-Host

if (-not $PSCmdlet.ShouldProcess($releaseRoot, 'Create immutable WebGL deployment staging')) { return }

New-Item -ItemType Directory -Path $releaseRoot | Out-Null
try {
  foreach ($item in Get-ChildItem -LiteralPath $ArtifactDirectory -Force) {
    Copy-Item -LiteralPath $item.FullName -Destination $releaseRoot -Recurse
  }
  & $verifyScript -ArtifactDirectory $ArtifactDirectory -ExpectedBuildGuid $ExpectedBuildGuid -SourceCommitSha $SourceCommitSha -CompareDirectory $releaseRoot | Out-Host

  if ($PromoteLatest) {
    $historyPath = Join-Path $websiteRoot 'data\webgl-releases.json'
    $history = Get-Content -LiteralPath $historyPath -Raw | ConvertFrom-Json
    if ($history.latestBuildGuid -ne $ExpectedBuildGuid) {
      throw 'Tracked latestBuildGuid must match the promoted build before deployment staging.'
    }
  }
} catch {
  if (Test-Path -LiteralPath $releaseRoot) { Remove-Item -LiteralPath $releaseRoot -Recurse -Force }
  throw
}

[pscustomobject]@{
  Result = 'PASS'
  ReleaseDirectory = $releaseRoot
  PromotedLatest = [bool]$PromoteLatest
} | ConvertTo-Json
