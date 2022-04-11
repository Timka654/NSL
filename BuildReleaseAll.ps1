$ver = (Get-Date).ToString("yyyy.MM.dd.HHmm")

./BuildRelease $ver
./BuildReleaseUnity $ver