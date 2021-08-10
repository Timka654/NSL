$ver = $args[0]
dotnet build --configuration UnityDebug "SSFUnity.sln"
dotnet pack --configuration UnityDebug --output "unity" --version-suffix "$ver" SSFUnity.sln