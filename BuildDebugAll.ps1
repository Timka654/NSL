$ver = (Get-Date).ToString("yyyy.MM.dd.HHmm")

$buildPath = "build/Debug"

if (Test-Path $buildPath ) {
    remove-item $buildPath -Recurse -Force
}

./BuildDebug $ver
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
./BuildDebugUnity $ver
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }