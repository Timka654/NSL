$ver = $args[0]
dotnet pack --version-suffix "$ver" --configuration UnityDebug --output "package_unity" SSFUnity.sln