$ver = (Get-Date).ToString("yyyy.MM.dd.HHmm")

$buildPath = "build/Debug"

if (Test-Path $buildPath ) {
    remove-item $buildPath -Recurse -Force
}

./BuildDebug $ver
./BuildDebugUnity $ver