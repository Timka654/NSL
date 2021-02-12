$ver = (Get-Date).ToString("yyyy.MM.dd.hhmm")

./BuildDebug $ver
./BuildRelease $ver