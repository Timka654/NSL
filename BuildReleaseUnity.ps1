$ver = $args[0]
dotnet build --version-suffix "$ver" --configuration Unity /p:Platform=x64 --output "build/Release/unity_dll_$ver" "NSL.Unity.sln"
dotnet pack --version-suffix "$ver" --configuration Unity --output "build/Release/unity_package_$ver" "NSL.Unity.sln"


$buildPath = "build/Release/unity_dll_$ver"

$patternHere  = 'UnityEngine'

$directoryInfo = [System.IO.DirectoryInfo]::new($buildPath)

foreach($item in ($directoryInfo.GetFiles("*", 1)))
{
    if($item.Name.Contains($patternHere))
    {
        Write-Output "Remove file $($item.FullName)"
        $item.Delete()
    }
}