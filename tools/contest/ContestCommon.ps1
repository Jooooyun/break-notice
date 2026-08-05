Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ContestContext {
    param(
        [string]$UnityEditorPath = "C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe",
        [switch]$RequireWebGL
    )

    $repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
    $projectPath = Join-Path $repositoryRoot "Game"
    $projectVersionPath = Join-Path $projectPath "ProjectSettings\ProjectVersion.txt"

    if (-not (Test-Path -LiteralPath $UnityEditorPath -PathType Leaf)) {
        throw "Exact Unity executable not found: $UnityEditorPath"
    }

    $versionText = Get-Content -Raw -LiteralPath $projectVersionPath -Encoding UTF8
    if ($versionText -notmatch "m_EditorVersion:\s+6000\.3\.20f1" -or
        $versionText -notmatch "c9ba695d4f07") {
        throw "ProjectVersion.txt is not the authorized 6000.3.20f1 / c9ba695d4f07 baseline."
    }

    if ($UnityEditorPath -notmatch "6000\.3\.20f1") {
        throw "Unity path does not identify exact version 6000.3.20f1: $UnityEditorPath"
    }

    if ($RequireWebGL) {
        $webGlSupport = Join-Path (Split-Path $UnityEditorPath -Parent) "Data\PlaybackEngines\WebGLSupport"
        if (-not (Test-Path -LiteralPath $webGlSupport -PathType Container)) {
            throw "Exact Unity Web Build Support is unavailable: $webGlSupport"
        }
    }

    $logPath = Join-Path $projectPath "Logs\Contest"
    New-Item -ItemType Directory -Path $logPath -Force | Out-Null

    [PSCustomObject]@{
        RepositoryRoot = $repositoryRoot
        ProjectPath = $projectPath
        UnityEditorPath = $UnityEditorPath
        LogPath = $logPath
        BuildPath = Join-Path $projectPath "Builds\WebGL\Development"
    }
}

function Read-UnityTestResult {
    param([Parameter(Mandatory = $true)][string]$ResultPath)

    if (-not (Test-Path -LiteralPath $ResultPath -PathType Leaf)) {
        throw "Unity did not create the expected test result: $ResultPath"
    }

    [xml]$document = Get-Content -Raw -LiteralPath $ResultPath -Encoding UTF8
    $run = $document.'test-run'
    if ($null -eq $run) {
        throw "Unity test result has no test-run root: $ResultPath"
    }

    $summary = [PSCustomObject]@{
        Total = [int]$run.total
        Passed = [int]$run.passed
        Failed = [int]$run.failed
        Skipped = [int]$run.skipped
        Inconclusive = [int]$run.inconclusive
        ResultPath = $ResultPath
    }

    if ($summary.Failed -ne 0 -or $summary.Inconclusive -ne 0) {
        throw "Unity tests did not pass: $($summary | ConvertTo-Json -Compress)"
    }

    return $summary
}
