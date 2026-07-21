$ver = (Get-Date).ToString("yyyy.MM.dd.HHmm")

if (Test-Path "build" ) {
	remove-item "build" -Recurse -Force
}

./BuildDebug $ver
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
./BuildRelease $ver
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }