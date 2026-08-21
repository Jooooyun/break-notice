param(
    [string]$UnityEditorPath = "C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe"
)

. (Join-Path $PSScriptRoot "ContestCommon.ps1")
$context = Get-ContestContext -UnityEditorPath $UnityEditorPath
$resultPath = Join-Path $context.LogPath "playmode-results.xml"
$logPath = Join-Path $context.LogPath "playmode.log"

$arguments = @(
    "-batchmode",
    "-nographics",
    "-projectPath", $context.ProjectPath,
    "-runTests",
    "-testPlatform", "PlayMode",
    "-testResults", $resultPath,
    "-logFile", $logPath
)

$process = Start-Process `
    -FilePath $context.UnityEditorPath `
    -ArgumentList $arguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden
if ($process.ExitCode -ne 0) {
    throw "Unity PlayMode test process failed with exit code $($process.ExitCode). Log: $logPath"
}

Read-UnityTestResult -ResultPath $resultPath
