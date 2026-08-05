param(
    [string]$UnityEditorPath = "C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe"
)

. (Join-Path $PSScriptRoot "ContestCommon.ps1")
$context = Get-ContestContext -UnityEditorPath $UnityEditorPath -RequireWebGL
$logPath = Join-Path $context.LogPath "web-development-build.log"

$arguments = @(
    "-batchmode",
    "-nographics",
    "-quit",
    "-projectPath", $context.ProjectPath,
    "-buildTarget", "WebGL",
    "-executeMethod", "BreakNotice.Contest.Editor.ContestC0Automation.BuildWebDevelopment",
    "-logFile", $logPath
)

$process = Start-Process `
    -FilePath $context.UnityEditorPath `
    -ArgumentList $arguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden
if ($process.ExitCode -ne 0) {
    throw "Unity Web Development build failed with exit code $($process.ExitCode). Log: $logPath"
}

foreach ($requiredPath in @(
    (Join-Path $context.BuildPath "index.html"),
    (Join-Path $context.BuildPath "Build"),
    (Join-Path $context.BuildPath "TemplateData"),
    (Join-Path $context.BuildPath "BuildMetadata.json")
)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Web build is missing required output: $requiredPath"
    }
}

[PSCustomObject]@{
    Result = "PASS"
    BuildPath = $context.BuildPath
    LogPath = $logPath
}
