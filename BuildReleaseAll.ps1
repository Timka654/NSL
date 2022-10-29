$ver = (Get-Date).ToString("yyyy.MM.dd.HHmm")
$buildPath = "build/Release"

if (Test-Path $buildPath ) {
    remove-item $buildPath -Recurse -Force
}

./BuildRelease $ver
./BuildReleaseUnity $ver


# $patternHere  = 'Binary'

# $directoryInfo = [System.IO.DirectoryInfo]::new($buildPath)

# foreach($item in ($directoryInfo.GetFiles("*", 1)))
# {
    # if($item.Name.Contains($patternHere))
    # {
        # Write-Output "Remove file $($item.FullName)"
        # $item.Delete()
    # }
# }

# $patternHere  = '.Node'

# $directoryInfo = [System.IO.DirectoryInfo]::new($buildPath)

# foreach($item in ($directoryInfo.GetFiles("*", 1)))
# {
    # if($item.Name.Contains($patternHere))
    # {
        # Write-Output "Remove file $($item.FullName)"
        # $item.Delete()
    # }
# }