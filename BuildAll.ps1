$ver = (Get-Date).ToString("yyyy.MM.dd.HHmm")

./BuildDebug $ver
./BuildRelease $ver