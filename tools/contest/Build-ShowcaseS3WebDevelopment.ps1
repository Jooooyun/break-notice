param(
    [string]$UnityEditorPath = "C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe"
)

. (Join-Path $PSScriptRoot "ContestCommon.ps1")
$context = Get-ContestContext -UnityEditorPath $UnityEditorPath -RequireWebGL
$logPath = Join-Path $context.LogPath "showcase-s3-web-development-build.log"

$arguments = @(
    "-batchmode",
    "-nographics",
    "-quit",
    "-projectPath", $context.ProjectPath,
    "-buildTarget", "WebGL",
    "-executeMethod", "BreakNotice.Contest.Editor.ContestShowcaseS3WebBuildAutomation.BuildWebDevelopment",
    "-logFile", $logPath
)

$process = Start-Process `
    -FilePath $context.UnityEditorPath `
    -ArgumentList $arguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden
if ($process.ExitCode -ne 0) {
    throw "Unity S3 Showcase Web Development build failed with exit code $($process.ExitCode). Log: $logPath"
}

foreach ($requiredPath in @(
    (Join-Path $context.BuildPath "index.html"),
    (Join-Path $context.BuildPath "Build"),
    (Join-Path $context.BuildPath "TemplateData"),
    (Join-Path $context.BuildPath "BuildMetadata.json")
)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "S3 Showcase Web build is missing required output: $requiredPath"
    }
}

$metadata = Get-Content -Raw -LiteralPath `
    (Join-Path $context.BuildPath "BuildMetadata.json") | ConvertFrom-Json
if ($metadata.showcaseCheckpoint -ne "S3") {
    throw "Build metadata does not identify the S3 Showcase checkpoint."
}

[PSCustomObject]@{
    Result = "PASS"
    BuildPath = $context.BuildPath
    LogPath = $logPath
    BuildGuid = $metadata.buildGuid
    Warnings = $metadata.totalWarnings
    Errors = $metadata.totalErrors
}
