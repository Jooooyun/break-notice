param(
    [int]$Port = 8765,
    [string]$PythonPath = "python"
)

. (Join-Path $PSScriptRoot "ContestCommon.ps1")
$context = Get-ContestContext

if (-not (Test-Path -LiteralPath (Join-Path $context.BuildPath "index.html") -PathType Leaf)) {
    throw "Build index does not exist: $($context.BuildPath)"
}

$process = Start-Process `
    -FilePath $PythonPath `
    -ArgumentList @(
        "-m", "http.server", "$Port", "--bind", "127.0.0.1",
        "--directory", $context.BuildPath
    ) `
    -PassThru `
    -WindowStyle Hidden

$url = "http://127.0.0.1:$Port/"
$ready = $false
for ($attempt = 0; $attempt -lt 40; $attempt++) {
    if ($process.HasExited) {
        throw "Local HTTP server exited before becoming ready. Exit code: $($process.ExitCode)"
    }

    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -eq 200) {
            $ready = $true
            break
        }
    }
    catch {
        Start-Sleep -Milliseconds 250
    }
}

if (-not $ready) {
    Stop-Process -Id $process.Id -Force
    throw "Local HTTP server did not become ready at $url."
}

$pidPath = Join-Path $context.LogPath "web-server.pid"
Set-Content -LiteralPath $pidPath -Value $process.Id -Encoding ASCII

[PSCustomObject]@{
    Result = "READY"
    Url = $url
    ProcessId = $process.Id
    PidPath = $pidPath
    BuildPath = $context.BuildPath
}
